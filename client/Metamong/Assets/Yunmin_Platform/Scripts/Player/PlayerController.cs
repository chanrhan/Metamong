using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10.0f;
    private float speedLimit = 15.0f;
    [SerializeField] private float jumpForce = 10.0f;
    private bool isOnGround = true;
    private Rigidbody myRigid;
    
    //애니메이션 관련
    private Animator myAnim;
    private bool isTalkingNow = false;
    private bool isTyping = false;

    private void Awake()
    {
        myRigid = GetComponent<Rigidbody>();
        myAnim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
    }

    void Update()
    {
        TryTalking();
        if (!isTyping)
        {
            MovePosition();
            TryJump();
        }
    }

    private void MovePosition()
    {
        Vector3 nowVel = new Vector3(myRigid.velocity.x, 0, myRigid.velocity.z);
        if (nowVel.magnitude < speedLimit)
        {
            Vector3 moveVec = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical")).normalized;
            myRigid.AddForce(moveVec * moveSpeed, ForceMode.Force);            
        }
    }

    private void TryJump()
    {
        if (isOnGround)
        {
            if (Input.GetButtonDown("Jump"))
            {
                Jump();
            }
        }
    }
    private void Jump()
    {
        myRigid.AddForce(Vector3.up* jumpForce, ForceMode.Impulse);
        isOnGround = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
        }
    }

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
