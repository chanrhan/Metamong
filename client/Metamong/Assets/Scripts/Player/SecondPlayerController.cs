using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondPlayerController : MonoBehaviour
{
    [SerializeField]
    private string playerName = "Player2";
    private string[] talkTextArray =
    {
        "Aaaa","bbb","ccc"
    };
    [SerializeField]
    private int talkIndex = 0;
    public float talkSpeedSecond = 1.0f;
    private IEnumerator myTalkCoroutine;
    public float speekRange = 5.0f;

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

    private IEnumerator TalkCoroutine()
    {
        while (true)
        {
            SendMessageToOthers();
            yield return new WaitForSeconds(talkSpeedSecond);
        }
    }

    private void SendMessageToOthers()
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