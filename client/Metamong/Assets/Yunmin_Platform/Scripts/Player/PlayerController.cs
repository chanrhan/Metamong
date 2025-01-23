using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    //이동관련
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

    void Update()
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
        Vector3 nowVel = new Vector3(myRigid.velocity.x, 0, myRigid.velocity.z);
        if (nowVel.sqrMagnitude < speedLimit * speedLimit)
        {
            Vector3 moveVec = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical")).normalized;
            myRigid.AddForce(moveVec * moveSpeed, ForceMode.Force);
        }
        SetWalkingAnim();
    }

    //걷기 애니메이션 출력 함수
    private void SetWalkingAnim()
    {
        if (myRigid.velocity.sqrMagnitude < 1.0f)
        {
            myAnim.SetBool("isWalking", false);
        }
        else
        {
            myAnim.SetBool("isWalking", true);
            Debug.Log("Walking");
        }
    }

    private void CharacterRotate()
    {
        Vector3 moveVec = new Vector3(myRigid.velocity.x, 0.0f, myRigid.velocity.z).normalized;
        if (moveVec != Vector3.zero)
            transform.forward = moveVec;
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
