using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour, IListenable
{
    //NPC 컴포넌트 및 변수
    [SerializeField]
    private string npcName = "BasicNPC";

    public string NpcName
    {
        get => npcName;
    }

    private NavMeshAgent myAgent;

    //otherPlayer의 대화텍스트 관련 변수들. STT가 구현되어 채팅의 입력이 구현되면 삭제 예정
    private string[] talkTextArray =
    {
        "안녕 난 NPC야",
        "오늘 정말 날씨가 레알 좋은것 같아.",
        "흣~챠!",
        "오늘은 뭔가 재미난게 없을까나",
    };
    [SerializeField]
    private int talkIndex = 0;              //talkTextArray의 현재 출력된 텍스트를 가리키는 인덱스
    public float talkSpeedSecond = 1.0f;    //talkTextIndex 증가 속도.
    private IEnumerator myTalkCoroutine;    //일정 주기로 talkTextArray의 텍스트를 출력하는 코루틴

    //대화 관련
    public float speekRange = 5.0f;         //다른 플레이어에게 채팅 전달 범위
    public bool isGeneratingAnswer = false;
    public GameObject detectedPlayer;

    private void Awake()
    {
        myAgent = GetComponent<NavMeshAgent>();
        myTalkCoroutine = TalkCoroutine();
        talkTextArray[0] = $"안녕 난{npcName}(이)라고 해.";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if(myTalkCoroutine != null) StopCoroutine(myTalkCoroutine);
            StartCoroutine(myTalkCoroutine);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            StopCoroutine(myTalkCoroutine);
        }
    }

    /// <summary>
    /// talkSpeedSecond 초 단위로 대화텍스트를 출력하는 코루틴. STT와 같은 대화텍스트 입력 기능이 구현되면 삭제 예정
    /// </summary>
    /// <returns></returns>
    private IEnumerator TalkCoroutine()
    {
        while (true)
        {
            SendMessageToOthers();
            yield return new WaitForSeconds(talkSpeedSecond);
        }
    }

    /// <summary>
    /// 주변 플레이어에게 자신의 대화 텍스트를 전달하는 메서드. STT와 같은 대화 텍스트 입력 기능이 구현되면 수정할 예정
    /// 주변의 플레이어는 Conversable 레이어와 "Player"태그를 가지고 있어야 한다.
    /// </summary>
    public void SendMessageToOthers()
    {
        RaycastHit[] hitPlayers = Physics.SphereCastAll(transform.position, speekRange, Vector3.up, 0.0f, 64); //64 = Conversable Layer(2^7)
        foreach (RaycastHit hit in hitPlayers)
        {
            if (hit.transform.CompareTag("Player") && ChatManager.Instance != null)
            {
                ChatManager.Instance.InputChat(npcName, GenerateAnswer());
            }
        }
        isGeneratingAnswer = false;
        talkIndex = (talkIndex + 1) % talkTextArray.Length;
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
    public void ListenMessage(GameObject partnerObj, string message)
    {
        detectedPlayer = partnerObj;
        myAgent.isStopped = true;
        isGeneratingAnswer = true;

    }

    /// <summary>
    /// 대화를 생성하는 메서드. 대화 텍스트를 생성하여 반환한다. 나중에 더 추가할 예정
    /// </summary>
    /// <returns> 대화를 건 플레이어에게 전달할 메시지</returns>
    public string GenerateAnswer()
    {
        return talkTextArray[talkIndex];
    }

    /// <summary>
    /// 대화를 종료했음을 알 수 있도록 변수를 설정하는 메서드.
    /// Npc의 BehaviorTree에서 사용하도록 만듦.
    /// </summary>
    public void EndConversation()
    {
        Debug.Log("대화를 종료함");
        myAgent.isStopped = false;
        detectedPlayer = null;
    }
}