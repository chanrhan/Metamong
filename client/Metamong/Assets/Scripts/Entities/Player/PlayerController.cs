using LLMUnity;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;
using System.Threading;


public class PlayerController : NetworkCharacter
{
    //이동관련
    [SerializeField] private float moveSpeed = 10.0f;       //플레이어 이동속도
    [SerializeField] private float speedLimit = 10.0f;      //플레이어 최대 이동속도
    [SerializeField] private float jumpForce = 10.0f;       //플레이어 점프력
    private bool isOnGround = true;                         //플레이어 땅에 닿았는지 여부
    private Rigidbody myRigid;      
    private Collider myCollider;
    private Vector3 currMoveVec = new Vector3(0,0,0);

    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(0.2f));


    //애니메이션 관련
    private Animator myAnim;
    private AvatarAnimation avatarAnim;
    private bool isTyping = false;      //삭제 예정. 표정 키워드를 Input으로 입력중에 활성화 됨.
    

    public event Action<string, SegmentMotionSet> OnActionTextUpdated;

    private void Awake()
    {
        myRigid = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
        avatarAnim = GetComponent<AvatarAnimation>();
    }

    private void Start()
    {
        if(IsOwner){
            CameraController.Instance.SetTargetPlayer(gameObject);
            ClientManager.Instance.MyPlayerObject = gameObject;
            ClientManager.Instance.ClientInfo.clientId = OwnerClientId;
            transform.position = new Vector3(-10,0,-5);
        }
        STTManager.Instance.PlayerController = this;
    }

    private void FixedUpdate()
    {
        if(!IsOwner){
            return;
        }
        CharacterRotate();
        CheckOnGround();
        if (!ChatManager.Instance.IsTyping)
        {
            MovePosition();
        }
        else
        {
            // myAnim.SetBool("isWalking",false);
            avatarAnim.StopWalking();
        }
    }

    private void Update()
    {
        // 자신의 플레이어(소유자)가 아니라면 조작 안됨 
        if(!IsOwner){
            return;
        }

        TryTalking();
        if (!ChatManager.Instance.IsTyping)
        {
            //CharacterRotate();
            //CheckOnGround();
            TryJump();

            // 테스트용 
            if (Input.GetKeyDown(KeyCode.G))
            {
                SendMessageToOthers();
            }
        }
        
    }

    /// <summary>
    /// 플레이어를 이동시키는 메서드. 플레이어의 이동속도를 제한하고, 애니메이션 출력까지 한다.
    /// </summary>
    private void MovePosition()
    {
        // float nowVel = Mathf.Pow(myRigid.velocity.x, 2) + Mathf.Pow(myRigid.velocity.z, 2);
        // if (nowVel < speedLimit * speedLimit)
        // {
        //     Vector3 moveVec = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical")).normalized;
        //     //myRigid.AddForce(moveVec * moveSpeed, ForceMode.Force);
        //     myRigid.MovePosition(myRigid.position + moveVec * moveSpeed * Time.deltaTime);
        //     nowVel = Mathf.Pow(myRigid.velocity.x, 2) + Mathf.Pow(myRigid.velocity.z, 2);
        // }

        // SetWalkingAnim(nowVel);
        Vector3 moveVec = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));


        if (moveVec != Vector3.zero)
        {
            Vector3 tempVec = CameraController.Instance.nowWatchingVec;
            currMoveVec.x = tempVec.x * moveVec.z + tempVec.z * moveVec.x;
            currMoveVec.z = tempVec.z * moveVec.z - tempVec.x * moveVec.x;

            myRigid.MovePosition(myRigid.position + currMoveVec.normalized * moveSpeed);
            // myAnim.SetBool("isWalking",true);
            avatarAnim.StartWalking();
        }
        else
        {
            // myAnim.SetBool("isWalking", false);
            avatarAnim.StopWalking();
        }
    }

    /// <summary>
    /// 플레이어 걷기 애니메이션 출력함수. 일정속도 이하면 걷기 애니메이션 출력을 하지 않음.
    /// </summary>
    /// <param name="speedSqure"> 이동속도의 제곱값</param>
    // private void SetWalkingAnim(float speedSqure)
    // {
    //     if (speedSqure < 1.0f)
    //     {
    //         myAnim.SetBool("isWalking", false);
    //     }
    //     else
    //     {
    //         myAnim.SetBool("isWalking", true);
    //         // Debug.Log("Walking");
    //     }
    // }

    /// <summary>
    /// 캐릭터를 회전시키는 메서드. 캐릭터의 실제 이동벡터에 맞게 회전시킴.
    /// </summary>
    private void CharacterRotate()
    {
        // Vector3 moveVec = new Vector3(myRigid.velocity.x, 0.0f, myRigid.velocity.z);

        // if (moveVec.sqrMagnitude > 0.1f)
        // {
        //     transform.forward = moveVec;
        // }
        transform.forward = Vector3.Slerp(transform.forward, currMoveVec, 0.4f);
    }

    /// <summary>
    /// 플레이어 오브젝트가 점프를 할수 있는 지를 체크하는 함수.
    /// </summary>
    private void TryJump()
    {
        if (Input.GetButtonDown("Jump") && isOnGround)
        {
            Jump();
        }
    }

    /// <summary>
    /// 플레이어 오브젝트를 점프시키는 메서드
    /// </summary>
    private void Jump()
    {
        myRigid.AddForce(Vector3.up* jumpForce, ForceMode.Impulse);
        isOnGround = false;
    }

    /// <summary>
    /// 플레이어 오브젝트가 땅에 닿았는지 체크하는 함수.
    /// </summary>
    private void CheckOnGround()
    {
        isOnGround =  Physics.Raycast(myCollider.bounds.center, Vector3.down, 1.05f);
        //isOnGround = Physics.BoxCast(myCollider.bounds.center, Vector3.one * 0.2f, Vector3.down, Quaternion.identity, 1.05f);
    }

    /// <summary>
    /// 플레이어의 대화 애니메이션을 출력하는 함수. 나중에 대화 기능이 더 구현되면 수정 예정
    /// </summary>
    private void TryTalking()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            // StartTalking();
            avatarAnim.StartTalking();
        }
        else if (Input.GetKeyUp(KeyCode.T))
        {
            // StopTalking();
            avatarAnim.StopTalking();
        }
    }

    /// <summary>
    /// 플레이어의 주변 플레이어에게 채팅을 보내는 함수. 
    /// 채팅을 전달받은 플레이어는 OtherPlayer 태그, Conversable 레이어로 설정해야함.
    /// </summary>
    public override void SendMessageToOthers() 
    {
        if(TryGetAroundAll(out NetworkTarget[] targets)){
            string msg = "Hello, My name is " + ClientManager.Instance.ClientInfo.username;
            
            PacketSendHandler.ChatText(msg, targets);
        }
    }

    public async void SendMessageToOthers(string message)
    {
        if(TryGetAroundAll(out NetworkTarget[] targets)){
            PacketSendHandler.ChatText(message, targets);
        }

        ClientInfo clientInfo = ClientManager.Instance.ClientInfo;
            
        ChatManager.Instance.InputChat(clientInfo.username, message);
        string response = await LlmManager.Instance.Chat(clientInfo.username + ": " +message, HandleReply, ReplyCompleted, false);
        LlmManager.Instance.AddChatLog(clientInfo.username,message);
            
        Debug.Log("Response: " + response);
            
        OnActionTextUpdated?.Invoke(response, default);
    }
    
    // 모션 매핑 프로세스
    
    public async Task SendResultToLlama(SegmentMotionSet segmentMotionSet, CancellationToken token)
    {
        // (Test) 모션 실행 도중 입력되는 세그먼트는 무조건 무시 
        if (avatarAnim.IsBlocked)
        {
            // Debug.Log($"[chan] Ignore : {segmentMotionSet.segment}");
            return;
        }
        avatarAnim.IsBlocked = true;
        

        string messageSegment = segmentMotionSet.segment;
        // 채팅 메세지는 NPC한테만 보내기
        if(TryGetAroundNPC(out NetworkTarget[] targets)){
            PacketSendHandler.ChatText(messageSegment, targets);
        }
        
        ClientInfo clientInfo = ClientManager.Instance.ClientInfo;

        //ChatManager.Instance.InputChat(clientInfo.username, message);
        TimerUtils.Start();
        string response = await LlmManager.Instance.Chat(clientInfo.username + ": " + messageSegment, HandleReply, ReplyCompleted, false);
        TimerUtils.LogAndReset();
        LlmManager.Instance.AddChatLog(clientInfo.username,messageSegment);

        // Debug.Log("Response: " + response);
        // Debug.Log($"[chan] {segmentMotionSet.segment} : {response}");
        
            
        OnActionTextUpdated?.Invoke(response, segmentMotionSet);
    }

    void HandleReply(string reply)
    {
        //Debug.Log("Extracted Actions: " + reply);
    }
    void ReplyCompleted()
    {
       // Debug.Log("Reply Completed");
    }

    /// <summary>
    /// 플레이어의 대화중 애니메이션을 출력하는 함수.
    /// </summary>
    private void StartTalking()
    {
        myAnim.SetBool("isTalking", true);
    }

    /// <summary>
    /// 플레이어의 대화중 애니메이션을 정지하는 함수.
    /// </summary>
    private void StopTalking()
    {
        myAnim.SetBool("isTalking", false);
    }

    /// <summary>
    /// 표정 키워드 InputField로 전달받은 키워드를 통해 알맞은 애니메이션을 출력하는 함수
    /// </summary>
    /// <param name="textBox">재생할 애니메이션 키워드가 담긴 InputField 객체</param>
    public void MakeFace(InputField textBox)
    {
        myAnim.Play(textBox.text, 2);
        textBox.text = "";
        textBox.onEndEdit.Invoke("");
    }

    /// <summary>
    /// 키워드를 통해 알맞은 애니메이션을 출력하는 함수
    /// </summary>
    /// <param name="expressionName">출력할 표정의 키워드</param>
    public void PlayMotion(string faceClipName, string actionClipName)
    {
        avatarAnim.PlayFaceAndActionAnimation(faceClipName, actionClipName);
    }

    /// <summary>
    /// 표정 키워드 InputField를 작성중인지 채크하는 함수. 추후 AI가 생성한 키워드를 입력으로 넣을 수 있을 때 되면 삭제 예정.
    /// </summary>
    /// <param name="flag">표정 키워드 입력 InputField의 값이 바뀌면 true가 입력됨.</param>
    public void SetIsTypingParameter(bool flag)
    {
        isTyping = flag;
    }

    public override void ListenMessage(GameObject partnerObj, string message)
    {
        throw new System.NotImplementedException();
    }

}
