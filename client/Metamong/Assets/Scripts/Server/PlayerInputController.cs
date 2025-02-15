using Unity.Netcode;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    private Rigidbody rigid;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();       
        Debug.Log($"IsOwner: {IsOwner}");
        if(IsOwner){
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
            Debug.Log(movePos);
            // rigid.velocity = movePos * 10 * Time.deltaTime;
            rigid.MovePosition(rigid.position + movePos * 10 * Time.deltaTime);
        }
    }

    
}
