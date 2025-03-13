using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour, IListenable
{
    //일반 정보 관련
    //private string playerName = "Me";       //플레이어의 이름

    //이동관련
    [SerializeField] private float moveSpeed = 10.0f;       //플레이어 이동속도
    [SerializeField] private float speedLimit = 10.0f;      //플레이어 최대 이동속도
    [SerializeField] private float jumpForce = 10.0f;       //플레이어 점프력
    private bool isOnGround = true;                         //플레이어 땅에 닿았는지 여부
    private Rigidbody myRigid;      
    private Collider myCollider;

    //대화 관련
    private float speekRange = 3.0f;    //다른 플레이어에게 채팅 전달 범위

    //애니메이션 관련
    private Animator myAnim;
    //private bool isTalkingNow = false;
    private bool isTyping = false;      //삭제 예정. 표정 키워드를 Input으로 입력중에 활성화 됨.

    private void Awake()
    {
        // Debug.Log("Awake: " + OwnerClientId + ", IsOwner" + IsOwner);
        myRigid = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
        myAnim = GetComponentInChildren<Animator>();

        
    }

    void Start()
    {
        if(IsOwner){
            Debug.Log("I am Owner : " + OwnerClientId);
            CameraController.Instance.SetTargetPlayer(gameObject);
        }
    }

    private void FixedUpdate()
    {
        CheckOnGround();
    }

    private void Update()
    {
        Debug.Log("Update: " + OwnerClientId + ", IsOwner" + IsOwner);
        // 자신의 플레이어(소유자)가 아니라면 조작 안됨 
        if(!IsOwner){
            Debug.Log(IsOwner);
            return;
        }

        TryTalking();
        if (!isTyping)
        {
            MovePosition();
            CharacterRotate();
            TryJump();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            SendMessageToOthers();
        }
    }

    /// <summary>
    /// 플레이어를 이동시키는 메서드. 플레이어의 이동속도를 제한하고, 애니메이션 출력까지 한다.
    /// </summary>
    private void MovePosition()
    {
        float nowVel = Mathf.Pow(myRigid.velocity.x, 2) + Mathf.Pow(myRigid.velocity.z, 2);
        if (nowVel < speedLimit * speedLimit)
        {
            Vector3 moveVec = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical")).normalized;
            myRigid.AddForce(moveVec * moveSpeed, ForceMode.Force);
            nowVel = Mathf.Pow(myRigid.velocity.x, 2) + Mathf.Pow(myRigid.velocity.z, 2);
        }

        SetWalkingAnim(nowVel);
    }

    /// <summary>
    /// 플레이어 걷기 애니메이션 출력함수. 일정속도 이하면 걷기 애니메이션 출력을 하지 않음.
    /// </summary>
    /// <param name="speedSqure"> 이동속도의 제곱값</param>
    private void SetWalkingAnim(float speedSqure)
    {
        if (speedSqure < 1.0f)
        {
            myAnim.SetBool("isWalking", false);
        }
        else
        {
            myAnim.SetBool("isWalking", true);
            Debug.Log("Walking");
        }
    }

    /// <summary>
    /// 캐릭터를 회전시키는 메서드. 캐릭터의 실제 이동벡터에 맞게 회전시킴.
    /// </summary>
    private void CharacterRotate()
    {
        Vector3 moveVec = new Vector3(myRigid.velocity.x, 0.0f, myRigid.velocity.z);
        if (moveVec.sqrMagnitude > 0.1f)
        {
            transform.forward = moveVec;
        }
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
        //isOnGround =  Physics.Raycast(myCollider.bounds.center, Vector3.down, 1.05f);
        isOnGround = Physics.BoxCast(myCollider.bounds.center, Vector3.one * 0.2f, Vector3.down, Quaternion.identity, 1.05f);
    }

    /// <summary>
    /// 플레이어의 대화 애니메이션을 출력하는 함수. 나중에 대화 기능이 더 구현되면 수정 예정
    /// </summary>
    private void TryTalking()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartTalking();
        }
        else if (Input.GetKeyUp(KeyCode.T))
        {
            StopTalking();
        }
    }

    /// <summary>
    /// 플레이어의 주변 플레이어에게 채팅을 보내는 함수. 
    /// 채팅을 전달받은 플레이어는 OtherPlayer 태그, Conversable 레이어로 설정해야함.
    /// </summary>
    private void SendMessageToOthers() 
    {
        RaycastHit[] hitPlayers = Physics.SphereCastAll(transform.position, speekRange, Vector3.up, 0.0f, 64); //64 = Conversable 레이어(2^7)

        foreach (RaycastHit hit in hitPlayers)
        {
            if(ChatManager.Instance != null)
            {
                if (hit.rigidbody.CompareTag("OtherPlayer") && ChatManager.Instance != null)
                {
                    hit.transform.GetComponent<IListenable>().ListenMessage(gameObject, "안녕!");
                }
            }

        }
    }

    public void SendMessageToOthers(string message) 
    {
        RaycastHit[] hitPlayers = Physics.SphereCastAll(transform.position, speekRange, Vector3.up, 0.0f, 64); //64 = Conversable 레이어(2^7)
        foreach (RaycastHit hit in hitPlayers)
        {
            if(ChatManager.Instance != null)
            {
                if (hit.rigidbody.CompareTag("OtherPlayer") && ChatManager.Instance != null)
                {
                    hit.transform.GetComponent<IListenable>().ListenMessage(gameObject, message);
                }
            }

        }
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
    public void MakeFace(string expressionName)
    {
        myAnim.Play(expressionName, 2);
    }
    public void MakeMotion(string motionName)
    {
        myAnim.Play(motionName, 0);
    }



    /// <summary>
    /// 표정 키워드 InputField를 작성중인지 채크하는 함수. 추후 AI가 생성한 키워드를 입력으로 넣을 수 있을 때 되면 삭제 예정.
    /// </summary>
    /// <param name="flag">표정 키워드 입력 InputField의 값이 바뀌면 true가 입력됨.</param>
    public void SetIsTypingParameter(bool flag)
    {
        isTyping = flag;
    }

    public void ListenMessage(GameObject partnerObj, string message)
    {
        throw new System.NotImplementedException();
    }

    void IListenable.SendMessageToOthers()
    {
        SendMessageToOthers();
    }

}
