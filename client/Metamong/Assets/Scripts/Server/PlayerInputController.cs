using Unity.Netcode;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    public static PlayerInputController Instance {get; private set;}

    [SerializeField]
    private float speed = 1;

    private Transform myPlayerTransform;

    public Transform MyPlayerTransform{
        get{ return myPlayerTransform;}
        set{
            Debug.Log(value);
            myPlayerTransform = value;
        }
    }

     private void Awake() {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void FixedUpdate() {
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
            if(IsServer || IsHost){
                MovePlayerPosition(movePos);
            }else{
                SendTransformServerRpc(movePos); 
            }
        }
    }

    [ServerRpc]
    public void SendTransformServerRpc(Vector3 vector){
        MovePlayerPosition(vector);
    }

    public void MovePlayerPosition(Vector3 vector){
        // Debug.Log($"Send Transform: {vector}");
        MyPlayerTransform.Translate(vector * 0.01f * speed, Space.World);
    }
}
