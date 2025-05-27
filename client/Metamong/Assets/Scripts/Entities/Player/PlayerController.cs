using UnityEngine;
using System;
using System.Threading;
using TMPro;
using System.Drawing.Text;


public class PlayerController : NetworkCharacter
{
    //이동관련
    [SerializeField] private float moveSpeed = 10.0f;       //플레이어 이동속도
    [SerializeField] private float speedLimit = 10.0f;      //플레이어 최대 이동속도
    [SerializeField] private float jumpForce = 10.0f;       //플레이어 점프력
    private bool isOnGround = true;                         //플레이어 땅에 닿았는지 여부
    [SerializeField] private TMP_Text userName;
    
    private Vector3 currMoveVec = new Vector3(0, 0, 0);
    [SerializeField] private Transform firstViewPos;

    private void Start()
    {
        
        if (IsOwner)
        {
            userName = GetComponentInChildren<TMP_Text>();
            CameraController.Instance.SetTargetPlayer(gameObject, firstViewPos);
            ClientManager.Instance.MyPlayerObject = gameObject;
            ClientManager.Instance.ClientInfo.clientId = OwnerClientId;
            transform.position = new Vector3(-10, 0, -5);
            userName.text = ClientManager.Instance.ClientInfo.username;
        }
    }

    private void FixedUpdate()
    {
        if(!IsOwner){
            return;
        }
        if(CameraController.Instance.IsThirdView) RotateCamara();
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
            TryJump();

            // 테스트용 
            // if (Input.GetKeyDown(KeyCode.G))
            // {
            //     SendMessageToOthers();
            // }
        }
        
    }

    /// <summary>
    /// 플레이어를 이동시키는 메서드. 플레이어의 이동속도를 제한하고, 애니메이션 출력까지 한다.
    /// </summary>
    private void MovePosition()
    {
        Vector3 moveVec = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));


        if (moveVec != Vector3.zero)
        {
            Vector3 tempVec
                = CameraController.Instance.IsThirdView ? CameraController.Instance.nowWatchingVec : transform.forward;
            currMoveVec.x = tempVec.x * moveVec.z + tempVec.z * moveVec.x;
            currMoveVec.z = tempVec.z * moveVec.z - tempVec.x * moveVec.x;

            myRigid.MovePosition(myRigid.position + currMoveVec.normalized * moveSpeed);
            avatarAnim.StartWalking();
        }
        else
        {
            avatarAnim.StopWalking();
        }
    }

    
    /// <summary>
    /// 캐릭터를 회전시키는 메서드. 캐릭터의 실제 이동벡터에 맞게 회전시킴.
    /// </summary>
    private void RotateCamara()
    {
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
        avatarAnim.StartJump();
    }

    /// <summary>
    /// 플레이어 오브젝트가 땅에 닿았는지 체크하는 함수.
    /// </summary>
    private void CheckOnGround()
    {
        isOnGround =  Physics.Raycast(myCollider.bounds.center, Vector3.down, 1.05f);
    }

    /// <summary>
    /// 플레이어의 대화 애니메이션을 출력하는 함수. 나중에 대화 기능이 더 구현되면 수정 예정
    /// </summary>
    private void TryTalking()
    {
       /* if (Input.GetKeyDown(KeyCode.T))
        {
            avatarAnim.StartTalking();
        }
        else if (Input.GetKeyUp(KeyCode.T))
        {
            avatarAnim.StopTalking();
        }*/
    }

    /// <summary>
    /// 플레이어의 주변 플레이어에게 채팅을 보내는 함수. 
    /// 채팅을 전달받은 플레이어는 OtherPlayer 태그, Conversable 레이어로 설정해야함.
    /// </summary>
    public override void SendMessageToOthers(string text) 
    {
        if(TryGetAroundNetworkTargets(null, out NetworkTarget[] targets)){
            PacketSendHandler.ChatText(text, targets);
        }
    }

    /// <summary>
    ///  키워드를 통해 알맞은 애니메이션을 출력하는 함수
    /// </summary>
    /// <param name="faceClipName">표정 애니메이션 클립명</param>
    /// <param name="actionClipName">액션 애니메이션 클립명</param>
    public override void PlayMotion(string faceClipName, string actionClipName)
    {
        avatarAnim.PlayFaceAndActionAnimation(faceClipName, actionClipName);
    }

    public override void ListenMessage(GameObject partnerObj, string message)
    {
        throw new System.NotImplementedException();
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
    /// 표정 키워드 InputField를 작성중인지 채크하는 함수. 추후 AI가 생성한 키워드를 입력으로 넣을 수 있을 때 되면 삭제 예정.
    /// </summary>
    /// <param name="flag">표정 키워드 입력 InputField의 값이 바뀌면 true가 입력됨.</param>
    // public void SetIsTypingParameter(bool flag)
    // {
    //     isTyping = flag;
    // }

    /// <summary>
    /// 플레이어의 대화중 애니메이션을 출력하는 함수.
    /// </summary>
    // private void StartTalking()
    // {
    //     myAnim.SetBool("isTalking", true);
    // }

    // /// <summary>
    // /// 플레이어의 대화중 애니메이션을 정지하는 함수.
    // /// </summary>
    // private void StopTalking()
    // {
    //     myAnim.SetBool("isTalking", false);
    // }

    /// <summary>
    /// 표정 키워드 InputField로 전달받은 키워드를 통해 알맞은 애니메이션을 출력하는 함수
    /// </summary>
    /// <param name="textBox">재생할 애니메이션 키워드가 담긴 InputField 객체</param>
    // public void MakeFace(InputField textBox)
    // {
    //     myAnim.Play(textBox.text, 2);
    //     textBox.text = "";
    //     textBox.onEndEdit.Invoke("");
    // }

    // public async void SendMessageToOthers(string message)
    // {
    //     if(TryGetAroundAll(out NetworkTarget[] targets)){
    //         PacketSendHandler.ChatText(message, targets);
    //     }

    //     ClientInfo clientInfo = ClientManager.Instance.ClientInfo;

    //     ChatManager.Instance.InputChat(clientInfo.username, message);
    //     string response = await Llama.Instance.Chat(clientInfo.username + ": " +message, HandleReply, ReplyCompleted, false);
    //     Llama.Instance.AddChatLog(clientInfo.username,message);

    //     Debug.Log("Response: " + response);

    //     OnActionTextUpdated?.Invoke(response, default);
    // }
}
