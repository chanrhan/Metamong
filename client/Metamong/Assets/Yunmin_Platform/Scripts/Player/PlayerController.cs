using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10.0f;
    private float speedLimit = 15.0f;
    [SerializeField] private float jumpForce = 10.0f;
    private bool isOnGround = true;
    private Rigidbody myRigid;

    private void Awake()
    {
        myRigid = GetComponent<Rigidbody>();
    }

    void Start()
    {

    }

    void Update()
    {
        MovePosition();
        TryJump();
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
}
