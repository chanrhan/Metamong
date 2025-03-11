using Unity.Netcode;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    private Rigidbody rigid;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();       
        if(IsOwner){ // 자신의 플레이어는 빨간색으로 알기 쉽게 표시 
            GetComponent<MeshRenderer>().material.color = Color.red;
        }
    }

    private void Update() {
        
        if(!IsOwner){
            return;
        }

        Vector3 movePos = Vector3.zero;
        
        if(Input.GetKey(KeyCode.W)){
            movePos += Vector3.forward;
        }
        if(Input.GetKey(KeyCode.A)){
            movePos += Vector3.left;
        }
        if(Input.GetKey(KeyCode.S)){
            movePos += Vector3.back;
        }
        if(Input.GetKey(KeyCode.D)){
            movePos += Vector3.right;
        }

        if(movePos != Vector3.zero){
            Move(movePos);
        }
    }

    /// <summary>
    /// Rigidbody를 통해서 플레이어를 이동시키는 함수
    /// </summary>
    /// <param name="vector">이동시킬 벡터 값</param>
    private void Move(Vector3 vector){
        rigid.MovePosition(rigid.position + vector * 10 * Time.deltaTime);
    }

    
}
