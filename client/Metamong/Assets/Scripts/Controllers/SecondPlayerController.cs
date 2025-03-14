using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete]
public class SecondPlayerController : MonoBehaviour
{
    //일반 정보
    [SerializeField]
    private string playerName = "Player2";      //플레이어의 이름

    //otherPlayer의 대화텍스트 관련 변수들. STT가 구현되어 채팅의 입력이 구현되면 삭제 예정
    private string[] talkTextArray =            
    {
        "헬로헬로",
        "오늘 정말 날씨가 레알 좋은것 같아.",
        "그건 좀 실망인데",
        "나 오늘 밥먹었는데 카레 나왔어",
        "그래서 뭐가 문제야?",
        "혼날래?",
        "ㅇ베베베ㅔ베베베베베베베ㅔ베베베베베베베베베ㅔ베베베베베ㅔㅂ",
        "진짜 개웃기네",
        "어어엌크크크크크크",
    };

    [SerializeField]
    private int talkIndex = 0;              //talkTextArray의 현재 출력된 텍스트를 가리키는 인덱스
    public float talkSpeedSecond = 1.0f;    //talkTextIndex 증가 속도.
    private IEnumerator myTalkCoroutine;    //일정 주기로 talkTextArray의 텍스트를 출력하는 코루틴

    //대화 관련
    public float speekRange = 5.0f;         //다른 플레이어에게 채팅 전달 범위

    private void Awake()
    {
        myTalkCoroutine = TalkCoroutine();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
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
        RaycastHit[] hitPlayers = Physics.SphereCastAll(transform.position, speekRange, Vector3.up, 0.0f, 64); //64 = Conversable ���̾�(2^7)
        foreach(RaycastHit hit in hitPlayers)
        {
            if (hit.transform.CompareTag("Player") && ChatManager.Instance != null)
            {
                ChatManager.Instance.InputChat(playerName, talkTextArray[talkIndex]);
            }
        }
        talkIndex = (talkIndex + 1) % talkTextArray.Length;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, speekRange);
    }
}