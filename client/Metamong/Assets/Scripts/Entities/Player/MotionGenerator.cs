using LLMUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
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

public class ActionFaceMotionSet
{
    public string actionClipName;
    public string faceClipName;
    public double actionScore;
    public double faceScore;

    public ActionFaceMotionSet()
    {
        
    }

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
    [Header("Llama")]
    [SerializeField]
    private bool onLlama = false;

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

    [Header("Parallel")]
    private float SEG_WAIT_TIME = 1f;
    private WhisperResult waitedWhisperResult;
    [SerializeField]
    private float currSegWaitTime = 0f;
    [SerializeField]
    private float currMotionWaitTime = 0f;
    private FileLogVO.LogContextItem waitedMotionLog;

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
        if (nc.IsTalking || nc.IsAnimPlaying)
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
        if (nc.IsAnimPlaying)
        {
            return;
        }
        nc.IsAnimPlaying = true;

        ActionFaceMotionSet actionFaceMotionSet = GetMotionKeywords(text);

        // 모션 애니메이션 실행 
        nc.PlayMotion(actionFaceMotionSet.faceClipName, actionFaceMotionSet.actionClipName);
    }

    private IEnumerator WaitSegmentCoroutine()
    {
        currSegWaitTime = 0f;
        while (currSegWaitTime < SEG_WAIT_TIME)
        {
            if (!onLlama)
            {
                if (waitedWhisperResult != null)
                {
                    FileLogVO.LogContextItem log = new FileLogVO.LogContextItem
                    {
                        segment = waitedWhisperResult.Result,
                        whisperInferTime = waitedWhisperResult.inferTime,
                        whisperFinishedInferTime = waitedWhisperResult.finsihedInferTime
                    };

                    DoLlama(log);
                }
                break;
            }
            currSegWaitTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        currSegWaitTime = 0f;
        waitedWhisperResult = null;
    }
    
    private IEnumerator WaitMotionCoroutine()
    {
        NetworkCharacter nc = ClientManager.Instance?.PlayerController;
        currMotionWaitTime = 0f;
        while (currMotionWaitTime < SEG_WAIT_TIME)
        {
            if (!nc.IsAnimPlaying)
            {
                if (waitedMotionLog != null)
                {
                    nc.PlayMotion(waitedMotionLog.faceClipName, waitedMotionLog.actionClipName);
                    waitedMotionLog.whileIgnored = ignoredSegements;
                    logContextItems.Add(waitedMotionLog); 
                    ignoredSegements.Clear();
                }
                break;
            }
            currMotionWaitTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        currMotionWaitTime = 0f;
        waitedMotionLog = null;
    }



    /// <summary>
    /// 모션을 생성하는 함수, Player / NPC 모두 통용
    /// 1. 응답을 가지고 Llama로 전처리 (비동기)
    /// 2. 1번 결과를 가직조 SBert로 모션 키워드 추출 
    /// 3. 모션 실행 
    /// 
    /// 1. Llama가 실행 중이라면, 임시 대기 세그먼트에 저장 (대체, 하나의 세그먼트만 저장할거임)
    /// 1-1. 최대 1초? 정도까지는 보관하고 있음 (이 기간동안 llama 처리가 끝나면 해당 세그먼트를 처리)
    /// 2. Llama가 실행 중이 아니라면, Llama 처리를 진행 
    /// 3. SBert 모션 매칭 
    /// 4. 모션 매칭이 끝났을 때, 아바타 모션이 재생 중이라면, 임시 대기 모션에 저장 (최대 1초까지 저장)
    /// 4-1. 아바타 모션 재생이 끝났을 때, 임시 대기 모션이 존재한다면, 해당 모션 재생 
    /// 5. 아바타 모션이 재생 중이 아니라면, 매칭된 모션 재생 
    /// 
    /// 
    public void Generate(WhisperResult whisperResult)
    {
        if (string.IsNullOrWhiteSpace(whisperResult.Result))
        {
            return;
        }

        ignoredSegements.Add(whisperResult.Result);

        // Llama가 실행 중이면, 일단 저장함
        // 1초 내로 끝나지 않을 시, 버림 
        if (onLlama)
        {
            waitedWhisperResult = whisperResult;
            if (currSegWaitTime == 0f)
            {
                StartCoroutine(WaitSegmentCoroutine());
            }
            else
            {
                currSegWaitTime = 0f;
            }
            return;
        }

        FileLogVO.LogContextItem log = new FileLogVO.LogContextItem
        {
            segment = whisperResult.Result,
            whisperInferTime = whisperResult.inferTime,
            whisperFinishedInferTime = whisperResult.finsihedInferTime
        };

        DoLlama(log);
    }

    private async void DoLlama(FileLogVO.LogContextItem log)
    {
        onLlama = true;

        ActionFaceMotionSet _actionFaceMotionSet = new ActionFaceMotionSet();
        string _llmResponse = null;
        long _llmTime = 0;
        long _sbertTime = 0;
        bool _timeout = false;
        string text = log.segment;
        NetworkCharacter nc = ClientManager.Instance?.PlayerController;

        // 주변 플레이어/NPC에게 메세지 전송 (이거는 이 함수랑 분리해야될거같은데, 일단 나중에 20250504)
        nc.SendMessageToOthers(text);
        _emotion = emotionClassifier.Predict(text);

        cts = new CancellationTokenSource();
        cts.CancelAfter(llmTimeoutLimit);
        
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
            nc.IsAnimPlaying = false;
            llama.ClearChatLogs();
        }
        onLlama = false;

        if (!_timeout)
        {
            if (cts.Token.IsCancellationRequested)
            {
                Debug.Log($"[chan] Cancelled before SBERT");
                nc.IsAnimPlaying = false;
                return;
            }
            TimerUtils.Start();
            // SBert를 통해 모션 키워드 추출 
            // 0번 인덱스: face Clip / 1번 인덱스: action Clip
            _actionFaceMotionSet = GetMotionKeywords(_llmResponse);
            _sbertTime = TimerUtils.LogAndReset(false);

            log.llmResponse = _llmResponse;
            log.emotion = _emotion;
            log.llmTime = _llmTime;
            log.llmTimeout = _timeout;
            log.sbertTime = _sbertTime;
            log.faceClipName = _actionFaceMotionSet.faceClipName;
            log.faceScore = _actionFaceMotionSet.faceScore;
            log.actionClipName = _actionFaceMotionSet.actionClipName;
            log.actionScore = _actionFaceMotionSet.actionScore;
            log.totalElapsedTime = log.whisperInferTime + _llmTime + _sbertTime;
            // 모션 키워드 생성 완료 


            // 모션이 실행중이면, 처리 자원 낭비 방지를 위해 입력을 막음 
            if (nc.IsAnimPlaying)
            {
                // llama.AddChatLog(ClientManager.Instance.ClientInfo.username, text);
                // ignoredSegements.Add(text);
                if (currMotionWaitTime == 0f)
                {
                    StartCoroutine(WaitMotionCoroutine());
                }
                else
                {
                    currMotionWaitTime = 0f;
                }
                waitedMotionLog = log;
                return;
            }
            else
            {
                // 모션 애니메이션 실행 
                nc.PlayMotion(_actionFaceMotionSet.faceClipName, _actionFaceMotionSet.actionClipName);
            }
        }


        // Whisper-Motion 기록용 (Log)
        log.whileIgnored = ignoredSegements;
        logContextItems.Add(log);
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