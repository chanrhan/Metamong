using LLMUnity;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NpcAI : NetworkCharacter
{
    //NpcAI 관련 컴포넌트 및 변수
    [SerializeField]
    private string npcName = "BasicNPC";
    private NpcAnimationController myAnimationController;
    private Coroutine dailyActionCoroutine;
    private ChatCompletionWithSummary chatCompletionWithSummary;
    private string inputmessage;
    public string NpcName
    {
        get => npcName;
    }

    
    public bool isGeneratingAnswer = true;
    public bool isMessageListened = false;
    public GameObject detectedPlayer;
    public string answerText;
    //private LLM llm;
    //private LLMCharacter llmCharacter;

    private void Awake()
    {
        myAnimationController = GetComponent<NpcAnimationController>();
        chatCompletionWithSummary = FindObjectOfType<ChatCompletionWithSummary>();


    }

    private void Update()
    {
        if(!IsServer){
            return;
        }

        if(detectedPlayer == null)
        {
            //일상 애니메이션 파트
            if(dailyActionCoroutine == null)
            {
                // Debug.Log("새 일상 코루틴 시작");
                dailyActionCoroutine = StartCoroutine(StartDailyActionCoroutine());
            }
        }
        else
        {
            //대화 파트
            LookAtPlayer();
            DetectPlayer();
        }
    }

    /// <summary>
    /// 주변 플레이어에게 자신의 대화 텍스트를 전달하는 메서드. STT와 같은 대화 텍스트 입력 기능이 구현되면 수정할 예정
    /// 주변의 플레이어는 Conversable 레이어와 "Player"태그를 가지고 있어야 한다.
    /// </summary>
    public override void SendMessageToOthers()
    {
        if(TryGetAroundNetworkTargets(out NetworkTarget[] targets)){
            string msg = chatCompletionWithSummary.ResponseText;
            ServerPacketReceiveHandler.TalkByNPC(msg, targets);
            isMessageListened = false;
        }
    }

   

    private void DetectPlayer()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 2.0f, Vector3.up, 0.0f, 64);
        for (int idx = 0; idx < hits.Length; idx++)
        {
            if (hits[idx].transform.gameObject == detectedPlayer)
            {
                // Debug.Log("플레이어 근처에 있음");
                return;
            }
        }
        EndConversation();
    }

    /// <summary>
    /// 탐지한 플레이어(detectedPlayer)를 바라보게 하는 메서드
    /// </summary>
    private void LookAtPlayer()
    {
        Quaternion dest
            = Quaternion.LookRotation(new Vector3(
                detectedPlayer.transform.position.x - transform.position.x,
                0.0f,
                detectedPlayer.transform.position.z - transform.position.z)
            );

        transform.rotation = Quaternion.Lerp(transform.rotation, dest, 0.1f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, speekRange);
    }

    /// <summary>
    /// NPC에게 메세지를 전달하는 메서드. 외부에서 사용하도록 설계함.
    /// </summary>
    /// <param name="message">NPC에게 전달할 메세지</param>
    public override void ListenMessage(GameObject partnerObj, string message)
    {
        if (isMessageListened) return;
        
        StopDailyCoroutine();
        detectedPlayer = partnerObj;
        isMessageListened = true;
        isGeneratingAnswer = true;
        StartCoroutine(GeneratingAnswerCoroutine(message));
    }

    /// <summary>
    /// 대답을 생성하는 코루틴. 5초간 대기 후 대답을 생성함. 그 후 SendMessageToOthers()를 호출함.
    /// </summary>
    /// <returns></returns>
    // public IEnumerator GeneratingAnswerCoroutine()

    public IEnumerator GeneratingAnswerCoroutine(string msg)
    {
        //myAnimationController.MyAnimator.SetBool("isThinking", true);
        //myAnimationController.MyAnimator.Play("ThinkingStart", 0);

        chatCompletionWithSummary.AddHistory("user", msg);
        yield return StartCoroutine(chatCompletionWithSummary.RequestChatCompletionAndMaybeSummarize(msg));

        // while(chatCompletionWithSummary.IsWaitingForResponse){
        //     Debug.Log("대화 생성 중");
        //     yield return null;
        // }

        //myAnimationController.MyAnimator.SetBool("isThinking", false);
        isGeneratingAnswer = false;
        //answerText = talkTextArray[talkIndex];
        //talkIndex = (talkIndex + 1) % talkTextArray.Length;

        SendMessageToOthers();
    }

    /// <summary>
    /// 대화를 종료했음을 알 수 있도록 변수를 설정하는 메서드.
    /// Npc의 BehaviorTree에서 사용하도록 만듦.
    /// </summary>
    public void EndConversation()
    {
        //myAgent.isStopped = false;
        detectedPlayer = null;
    }

    /// <summary>
    /// 일상적인 행동을 하게 해주는 코루틴. 랜덤으로 위치이동 or 애니메이션을 재생하게 만듬. 그 후엔 3초간 대기함.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartDailyActionCoroutine()
    {
        int randomNumber = Random.Range(1, 2);

        if(randomNumber == 0) //랜덤 위치 이동
        {
            myAnimationController.StopAllMovement();
            myAnimationController.MyAgent.SetDestination(new Vector3(Random.Range(-5.0f, 5.0f), 0.0f, Random.Range(-5.0f, 5.0f)));
            while (myAnimationController.MyAgent.pathPending){ yield return null; }

            myAnimationController.MyAnimator.SetBool("isWalking", true);
            while (myAnimationController.MyAgent.remainingDistance > 0.001f) { yield return null; }
            myAnimationController.MyAnimator.SetBool("isWalking", false);
        }
        else if (randomNumber == 1) //랜덤 애니메이션 재생
        {
            myAnimationController.PlayRandomAnimation(0);

            while (myAnimationController.MyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.98)
            {

                yield return null;
            } 
        }

        yield return new WaitForSeconds(3.0f);
        dailyActionCoroutine = null;
    }

    /// <summary>
    /// 현재 진행중인 일상 코루틴을 중단시키는 메서드. 애니메이션까지 중단시킨다.
    /// </summary>
    private void StopDailyCoroutine()
    {
        myAnimationController.StopAllMovement();
        if (dailyActionCoroutine != null)
        {
            StopCoroutine(dailyActionCoroutine);
            dailyActionCoroutine = null;
        }
    }
    public void MakeFace(string face)
    {
        myAnimationController.MyAnimator.Play(face, 2);
    }

    public void MakeMotion(string motion)
    {
        //myAnimationController.MyAnimator.SetTrigger("StopTrigger");
        myAnimationController.MyAnimator.Play(motion, 0);
    }

}