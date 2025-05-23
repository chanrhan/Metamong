using LLMUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
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
    [Header("Chat")]
    [SerializeField]
    private int maxChatLogLength = 10;

    [Header("Timeout")]
    [SerializeField]
    private int llmTimeoutLimit = 2000;
    private CancellationTokenSource cts = new CancellationTokenSource();

    private Llama llama;
    private SBERT sbert;
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

    private Queue<string> chatHistory = new Queue<string>(10);

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
            AddChatLog(ClientManager.Instance.ClientInfo.username, text);
            ignoredSegements.Add(text);
            return;
        }
        nc.IsAnimationBlocked = true;

        // 주변 플레이어/NPC에게 메세지 전송 (이거는 이 함수랑 분리해야될거같은데, 일단 나중에 20250504)
        nc.SendMessageToOthers(text);

        // 1. Llama를 통해 text를 전처리 

        // 타이머 디버그용 (Llama의 처리가 얼마나 걸리는지 측정)

        string[] _keywords = null;
        string _llmResponse = null;
        long _llmTime = 0;
        long _sbertTime = 0;

        cts = new CancellationTokenSource();
        cts.CancelAfter(llmTimeoutLimit);
        bool _timeout = false;
        TimerUtils.Start();
        try
        {
            _llmResponse = await GetResultFromLlama(text, cts.Token);
            _llmTime = TimerUtils.LogAndReset();
            _timeout = false;
        }
        catch (OperationCanceledException)
        {
            _llmTime = TimerUtils.LogAndReset(false);
            Debug.Log($"[chan] Timeout({_llmTime}) : {text}");

            _timeout = true;
            nc.IsAnimationBlocked = false;
            ClearChatLogs();
        }

        if (!_timeout)
        {
            if (cts.Token.IsCancellationRequested)
            {
                Debug.Log($"[chan] Cancelled before SBERT");
                nc.IsAnimationBlocked = false;
                return;
            }
            TimerUtils.Start();
            // SBert를 통해 모션 키워드 추출 
            // 0번 인덱스: face Clip / 1번 인덱스: action Clip
            _keywords = GetMotionKeywords(_llmResponse);
            _sbertTime = TimerUtils.LogAndReset(false);

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

    }

    private async Task<string> GetResultFromLlama(string seg, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        ClientInfo clientInfo = ClientManager.Instance.ClientInfo;

        string requestText = clientInfo.username + ": " + seg;
        string logs = GenerateChatLogs();
        Debug.Log($"[yun] {logs}마지막 발화 {requestText}");
        string response = await llama.Chat(logs + "마지막 발화 " + requestText, token);
        AddChatLog(clientInfo.username, seg);

        return response;
    }

    public string[] GetMotionKeywords(string motions)
    {
        string[] keywords = new string[2];
        MotionInfo actMotion = sbert.GetActMotionInfo(motions);

        //keywords[0] = sbert.CompareWordText(motions, true);
        keywords[0] = actMotion.clipNames[UnityEngine.Random.Range(0, actMotion.clipNames.Length)];
        keywords[1] = sbert.CompareWordText(motions, false);
        Debug.Log("DURA : " + keywords[0]);

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
    
    // Chat Log
    public void AddChatLog(string playerId, string msg)
    {
        while (chatHistory.Count >= maxChatLogLength)
        {
            chatHistory.Dequeue();
        }
        Debug.Log($"[yun] Add Chat {playerId}: {msg}");
        chatHistory.Enqueue($"{playerId}:{msg}");
    }

    public string GenerateChatLogs()
    {
        StringBuilder strBuilder = new StringBuilder("");
        foreach(string str in chatHistory)
        {
            strBuilder.Append($"{str}\n");
        }
        
        return strBuilder.ToString();
    }

    public void ClearChatLogs()
    {
        chatHistory.Clear();
    }
}
