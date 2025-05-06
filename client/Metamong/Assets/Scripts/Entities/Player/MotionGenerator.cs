using LLMUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Whisper;

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
    [Header("Timeout")]
    [SerializeField]
    private int llmTimeoutLimit = 2000;
    // private CancellationTokenSource cts = new CancellationTokenSource();

    private Llama llama;
    private SBERT sbert;

    private bool _timeout;
    private long _llmTime;
    private long _sbertTime;
    private string _llmResponse;
    private string[] _keywords;
    private float _stepSec;
    private float _keepSec;
    private float _lengthSec;
    public float StepSec
    {
        set => _stepSec = value;
    }
    public float KeepSec
    {
        set => _keepSec = value;
    }
    public float LengthSec
    {
        set => _lengthSec = value;
    }

    // Log
    [Header("Log")]
    [SerializeField]
    private string logFileName = "whisper_log";

    private static List<string> ignoredSegements = new List<string>();
    private List<FileLogVO.LogContextItem> logContextItems = new List<FileLogVO.LogContextItem>();

    protected override void Awake()
    {
        base.Awake();
        sbert = GetComponentInChildren<SBERT>();
        llama = GetComponentInChildren<Llama>();
    }

    public void PlayTakingMotion()
    {
        NetworkCharacter nc = ClientManager.Instance?.PlayerController;
        if (nc.IsTalking || nc.IsAnimationBlocked)
        {
            return;
        }
        nc.IsTalking = true;
    }

    public void GenerateNpcMotion(string text, NetworkCharacter nc)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }
        if (nc.IsAnimationBlocked)
        {
            return;
        }
        nc.IsAnimationBlocked = true;

        string[] keywords = GetMotionKeywords(text);

        // 모션 애니메이션 실행 
        nc.PlayMotion(keywords[0], keywords[1]);
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
    public async void Generate(WhisperResult whisperResult)
    {
        string text = whisperResult.Result;

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        NetworkCharacter nc = ClientManager.Instance?.PlayerController;

        // 모션이 실행중이면, 처리 자원 낭비 방지를 위해 입력을 막음 
        if (nc.IsAnimationBlocked)
        {
            ignoredSegements.Add(text);
            return;
        }
        nc.IsAnimationBlocked = true;

        // 주변 플레이어/NPC에게 메세지 전송 (이거는 이 함수랑 분리해야될거같은데, 일단 나중에 20250504)
        nc.SendMessageToOthers(text);

        // 1. Llama를 통해 text를 전처리 

        // 타이머 디버그용 (Llama의 처리가 얼마나 걸리는지 측정)
        TimerUtils.Start();
        _llmResponse = await GetResultFromLlama(text);
        _llmTime = TimerUtils.LogAndReset();
        _timeout = _llmResponse == null;

        if (!_timeout)
        {
            TimerUtils.Start();
            // SBert를 통해 모션 키워드 추출 
            // 0번 인덱스: face Clip / 1번 인덱스: action Clip
            _keywords = GetMotionKeywords(_llmResponse);
            _sbertTime = TimerUtils.LogAndReset();

            // 모션 애니메이션 실행 
            nc.PlayMotion(_keywords[0], _keywords[1]);
        }

        // Whisper-Motion 기록용 (Log)
        logContextItems.Add(new FileLogVO.LogContextItem
        {
            segment = text,
            whisperInferTime = whisperResult.inferTime,
            whileIgnored = new List<string>(ignoredSegements),
            llmResponse = _llmResponse,
            llmTime = _llmTime,
            llmTimeout = _timeout,
            sbertTime = _sbertTime,
            faceClipName = _keywords?[0],
            actionClipName = _keywords?[1],
            totalElapsedTime = whisperResult.inferTime + _llmTime + _sbertTime
        });
        ignoredSegements.Clear();
        nc.IsAnimationBlocked = false;
    }

    private async Task<string> GetResultFromLlama(string seg)
    {
        ClientInfo clientInfo = ClientManager.Instance.ClientInfo;

        string requestText = clientInfo.username + ": " + seg;

        var chatTask = llama.Chat(requestText);

        // LLM의 과도하게 긴 처리를 방지하기 위해 최대 처리 시간 제한 Task 생성 
        var delayTask = Task.Delay(TimeSpan.FromMilliseconds(llmTimeoutLimit));

        // LLM Task 와 Delay Task 중 먼저 끝날때까지 기다림 
        var finished = await Task.WhenAny(chatTask, delayTask);
        if (finished != chatTask) // LLM Task가 먼저 끝나지 않았다면, 
        {
            Debug.Log($"[chan] timeout : {seg}");
            return null;
        }
        string response = await chatTask;

        llama.AddChatLog(clientInfo.username, seg);
        return response;
    }

    public string[] GetMotionKeywords(string motions)
    {
        string[] keywords = new string[2];
        keywords[0] = sbert.CompareWordText(motions, true);
        keywords[1] = sbert.CompareWordText(motions, false);

        return keywords;
    }

    public void Log()
    {
        if (logContextItems.Count == 0)
        {
            return;
        }

        FileLogVO vo = new FileLogVO
        {
            stepSec = _stepSec,
            keepSec = _keepSec,
            lengthSec = _lengthSec,
            logContextItems = logContextItems
        };

        FileLogUtils.Overwrite(vo, logFileName);
        logContextItems.Clear();
    }
}
