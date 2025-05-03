using LLMUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class MotionGenerator : MonobehaviourSingleton<MotionGenerator>
{
    private List<SegmentMotionSet> segmentMotionSets = new List<SegmentMotionSet>();
    private CancellationTokenSource cts = new CancellationTokenSource();

    private SBERTMotionMapper sBERT;
    private LlmManager lLMManager;
    private static bool isBlocked;

    protected override void Awake()
    {
        base.Awake();
        sBERT = GetComponentInChildren<SBERTMotionMapper>();
        lLMManager = GetComponentInChildren<LlmManager>();
    }

    public async void Generate(string whisperSegment)
    {
        if (isBlocked)
        {
            return;
        }
        isBlocked = true;
        ClientManager.Instance.PlayerController.ChatToOther(whisperSegment);

        SegmentMotionSet segmentMotionSet = new SegmentMotionSet(whisperSegment);

        cts = new CancellationTokenSource();
        cts.CancelAfter(2000);
        try
        {
            string response = await GetResultFromLlama(whisperSegment, cts.Token);
            string[] keywords = sBERT.GetMotionKeys(response);

            ClientManager.Instance.PlayerController.PlayMotion(keywords[0], keywords[1]);
        }
        catch (OperationCanceledException e)
        {
            Debug.Log($"[chan] timeout: {cts.Token.IsCancellationRequested}");
            segmentMotionSet.timeout = true;
        }
        finally
        {
            cts.Dispose();
        }
        
        // print(segmentMotionSet.ToString());
        segmentMotionSets.Add(segmentMotionSet);
    }

    private async Task<string> GetResultFromLlama(string seg, CancellationToken token)
    {
        ClientInfo clientInfo = ClientManager.Instance.ClientInfo;

        //ChatManager.Instance.InputChat(clientInfo.username, message);
        string requestText = clientInfo.username + ": " + seg;
        TimerUtils.Start();
        string response = await lLMManager.Chat(requestText);
        TimerUtils.LogAndReset();
        LlmManager.Instance.AddChatLog(clientInfo.username, seg);

        // Debug.Log("Response: " + response);
        // Debug.Log($"[chan] {segmentMotionSet.segment} : {response}");

        return response;
    }

}
