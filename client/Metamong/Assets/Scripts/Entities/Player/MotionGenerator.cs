using LLMUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public enum LogState
{
    Failed,
    Ignored,
    Successed
}

/// <summary>
/// 모션 생성의 전체 프로세스를 담당하는 싱글톤 클래스
/// 자식 컴포넌트로 Llama, SBert를 보유해야함 
/// </summary>
public class MotionGenerator : MonobehaviourSingleton<MotionGenerator>
{
    [SerializeField]
    private int llamaCancelAfterSeconds = 2;
    private CancellationTokenSource cts = new CancellationTokenSource();

    private Llama llama;
    private SBERT sBERT;

    private bool _timeout;
    private long _llmTime;
    private long _sbertTime;

    protected override void Awake()
    {
        base.Awake();
        sBERT = GetComponentInChildren<SBERT>();
        llama = GetComponentInChildren<Llama>();
    }

    public void PlayTakingMotion(bool isPlayer = true, NetworkCharacter nc = null)
    {
        if (isPlayer)
        {
            nc = ClientManager.Instance?.PlayerController;
        }
        if (nc.IsTalking || nc.IsAnimationBlocked)
        {
            return;
        }
        nc.IsTalking = true;
    }

    /// <summary>
    /// 모션을 생성하는 함수, Player / NPC 모두 통용
    /// 1. 응답을 가지고 Llama로 전처리 (비동기)
    /// 2. 1번 결과를 가직조 SBert로 모션 키워드 추출 
    /// 3. 모션 실행 
    /// </summary>
    /// <param name="text">입력 텍스트</param>
    /// <param name="isPlayer">플레이어 여부</param>
    /// <param name="nc">플레이어가 아닐 경우, 호출하는 NPC 객체</param>
    public async Task<(LogState, FileLogVO.LogContextItem)> Generate(string text, bool isPlayer = true, NetworkCharacter nc = null)
    {
        if (text.Equals(""))
        {
            return (
                LogState.Failed,
                null
            );
        }
        // 플레이어일 경우 
        if (isPlayer)
        {
            nc = ClientManager.Instance?.PlayerController;
        }

        // 모션이 실행중이면, 처리 자원 낭비 방지를 위해 입력을 막음 
        if (nc.IsAnimationBlocked)
        {
            return (
                LogState.Ignored,
                null
            );
        }
        nc.IsAnimationBlocked = true;
        _timeout = false;
        _llmTime = 0;


        // 주변 플레이어/NPC에게 메세지 전송 (이거는 이 함수랑 분리해야될거같은데, 일단 나중에 20250504)
        nc.SendMessageToOthers(text);

        string response = text;
        if (isPlayer)
        {
            // 취소 토큰
            // 일정 시간이 지나면 자동으로 특정 Task를 중단시킨다 
            cts = new CancellationTokenSource();
            cts.CancelAfter(llamaCancelAfterSeconds * 1000);
            try
            {
                // 1. Llama를 통해 text를 전처리 
                // canlcelToken을 통해 처리 지연 부하 방지 

                // 타이머 디버그용 (Llama의 처리가 얼마나 걸리는지 측정)
                TimerUtils.Start();
                response = await GetResultFromLlama(text, cts.Token);
                _llmTime = TimerUtils.LogAndReset();
            }
            catch (OperationCanceledException)
            {
                // 토큰 만료 시 (=Llama의 처리가 너무 오래 걸렸을 경우)
                Debug.Log($"[chan] timeout: {cts.Token.IsCancellationRequested}");
                nc.IsAnimationBlocked = false;
                _timeout = true;
            }
            finally
            {
                // 토큰 해제 
                cts.Dispose();
            }
        }

        TimerUtils.Start();
        // SBert를 통해 모션 키워드 추출 
        // 0번 인덱스: face Clip / 1번 인덱스: action Clip
        string[] keywords = GetMotionKeywords(response);

        // 모션 애니메이션 실행 
        nc.PlayMotion(keywords[0], keywords[1]);
        _sbertTime = TimerUtils.LogAndReset();

        // Whisper-Motion 기록용 (Log)
        if (!isPlayer)
        {
            return (
                LogState.Failed,
                null
            );
        }
        return (
                LogState.Successed,
                new FileLogVO.LogContextItem
                {
                    segment = text,
                    llmResponse = response,
                    llmTime = _llmTime,
                    llmTimeout = _timeout,
                    sbertTime = _sbertTime,
                    faceClipName = keywords[0],
                    actionClipName = keywords[1]
                }
            );
    }

    private async Task<string> GetResultFromLlama(string seg, CancellationToken token)
    {
        ClientInfo clientInfo = ClientManager.Instance.ClientInfo;

        //ChatManager.Instance.InputChat(clientInfo.username, message);
        string requestText = clientInfo.username + ": " + seg;


        string response = await llama.Chat(requestText);

        llama.AddChatLog(clientInfo.username, seg);

        // Debug.Log("Response: " + response);
        // Debug.Log($"[chan] {segmentMotionSet.segment} : {response}");

        return response;
    }

    public string[] GetMotionKeywords(string motions)
    {
        string[] keywords = new string[2];
        keywords[0] = sBERT.CompareWordText(motions, true);
        keywords[1] = sBERT.CompareWordText(motions, false);

        return keywords;
    }
}
