using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviour
{
    public void Disconnect(){
        CustomNetworkManager.Instance.Disconnect();
        SceneManager.LoadScene("Login");
    }
}
