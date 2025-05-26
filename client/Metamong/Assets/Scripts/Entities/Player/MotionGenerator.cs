using LLMUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
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

public struct ActionFaceMotionSet
{
    public string actionClipName;
    public string faceClipName;
    public double actionScore;
    public double faceScore;

    public ActionFaceMotionSet(string action, string face, double actionScore = 0.0, double faceScore = 0.0)
    {
        this.actionScore = actionScore;
        this.faceScore = faceScore;
        actionClipName = action;
        faceClipName = face;
    }
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
    private CancellationTokenSource cts = new CancellationTokenSource();

    private Llama llama;
    private SBERT sbert;
    private EmotionClassifier emotionClassifier;
    private float _stepSec;
    private float _keepSec;
    private float _lengthSec;
    private string _emotion = "중립";
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
    public string Emotion
    {
        set => _emotion = value;
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
        emotionClassifier = GetComponentInChildren<EmotionClassifier>();
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

        ActionFaceMotionSet actionFaceMotionSet = GetMotionKeywords(text);

        // 모션 애니메이션 실행 
        nc.PlayMotion(actionFaceMotionSet.faceClipName, actionFaceMotionSet.actionClipName);
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
            llama.AddChatLog(ClientManager.Instance.ClientInfo.username, text);
            ignoredSegements.Add(text);
            return;
        }
        nc.IsAnimationBlocked = true;

        // 주변 플레이어/NPC에게 메세지 전송 (이거는 이 함수랑 분리해야될거같은데, 일단 나중에 20250504)
        nc.SendMessageToOthers(text);

        _emotion = emotionClassifier.Predict(text);

        // 1. Llama를 통해 text를 전처리 

        // 타이머 디버그용 (Llama의 처리가 얼마나 걸리는지 측정)

        // string[] _keywords = null;
        string _llmResponse = null;
        long _llmTime = 0;
        long _sbertTime = 0;
        ActionFaceMotionSet _actionFaceMotionSet = new ActionFaceMotionSet();

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
            llama.ClearChatLogs();
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
            _actionFaceMotionSet = GetMotionKeywords(_llmResponse);
            _sbertTime = TimerUtils.LogAndReset(false);

            // 모션 애니메이션 실행 
            nc.PlayMotion(_actionFaceMotionSet.faceClipName, _actionFaceMotionSet.actionClipName);
        }

        // Whisper-Motion 기록용 (Log)
        logContextItems.Add(new FileLogVO.LogContextItem
        {
            segment = text,
            whisperInferTime = whisperResult.inferTime,
            whisperFinishedInferTime = whisperResult.finsihedInferTime,
            whileIgnored = new List<string>(ignoredSegements),
            llmResponse = _llmResponse,
            emotion = _emotion,
            llmTime = _llmTime,
            llmTimeout = _timeout,
            sbertTime = _sbertTime,
            faceClipName = _actionFaceMotionSet.faceClipName,
            faceScore = _actionFaceMotionSet.faceScore,
            actionClipName = _actionFaceMotionSet.actionClipName,
            actionScore = _actionFaceMotionSet.actionScore,
            totalElapsedTime = whisperResult.inferTime + _llmTime + _sbertTime
        });
        ignoredSegements.Clear();
    }

    private async Task<string> GetResultFromLlama(string seg, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        ClientInfo clientInfo = ClientManager.Instance.ClientInfo;

        string requestText = clientInfo.username + ": " + seg;

        string response = await llama.Chat(requestText, token);
        llama.AddChatLog(clientInfo.username, seg);

        return response;
    }

    public ActionFaceMotionSet GetMotionKeywords(string motions)
    {
        // string[] keywords = new string[2];
        MotionInfo actMotion = sbert.GetActMotionInfo(motions, _emotion); // action
        ScoreMotion faceMotion = sbert.CompareWordText(motions, false); // face

        return new ActionFaceMotionSet
        {
            actionClipName = actMotion.clipNames[UnityEngine.Random.Range(0, actMotion.clipNames.Length)],
            faceClipName = faceMotion.motionKey,
            actionScore = actMotion.bestScore,
            faceScore = faceMotion.score
        };
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
            logContextItems = logContextItems,
        };

        FileLogUtils.Overwrite(vo, logFileName);
        logContextItems.Clear();
    }
}