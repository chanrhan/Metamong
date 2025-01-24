using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //이동관련
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float speedLimit = 10.0f;
    [SerializeField] private float jumpForce = 10.0f;
    private bool isOnGround = true;
    private Rigidbody myRigid;
    private Collider myCollider;
    
    //애니메이션 관련
    private Animator myAnim;
    //private bool isTalkingNow = false;
    private bool isTyping = false;

    private void Awake()
    {
        myRigid = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
        myAnim = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        CheckOnGround();
    }

    private void Update()
    {
        TryTalking();
        if (!isTyping)
        {
            MovePosition();
            CharacterRotate();
            TryJump();
        }
    }

    //이동 메서드
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

    //걷기 애니메이션 출력 함수
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

    //캐릭터 회전 메서드
    private void CharacterRotate()
    {
        Vector3 moveVec = new Vector3(myRigid.velocity.x, 0.0f, myRigid.velocity.z);
        if (moveVec.sqrMagnitude > 0.1f)
            transform.forward = moveVec;
    }

    //점프 시도 함수
    private void TryJump()
    {
        if (Input.GetButtonDown("Jump") && isOnGround)
        {
            Jump();
        }
    }

    //캐릭터 점프 함수
    private void Jump()
    {
        myRigid.AddForce(Vector3.up* jumpForce, ForceMode.Impulse);
        isOnGround = false;
    }

    //캐릭터가 땅에 닿는지 체크
    private void CheckOnGround()
    {
        //isOnGround =  Physics.Raycast(myCollider.bounds.center, Vector3.down, 1.05f);
        isOnGround = Physics.BoxCast(myCollider.bounds.center, Vector3.one * 0.2f, Vector3.down, Quaternion.identity, 1.05f);
    }

    //대화하기 기능 시도 함수
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

    //대화 기능 함수
    private void StartTalking()
    {
        myAnim.SetBool("isTalking", true);
    }
    private void StopTalking()
    {
        myAnim.SetBool("isTalking", false);
    }
    public void MakeFace(InputField textBox)    //인풋필드 UI용
    {
        myAnim.Play(textBox.text, 2);
        textBox.text = "";
        textBox.onEndEdit.Invoke("");
    }

    public void MakeFace(string expressionName) //나중에 사용할 부분
    {
        myAnim.Play(expressionName, 2);
    }

    public void SetIsTypingParameter(bool flag)
    {
        isTyping = flag;
    }
}
