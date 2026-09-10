using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
// 로비 UI
/// </summary>
public class LobbyUI : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField usernameInput;
    [SerializeField]
    private TMP_InputField joinCodeInput;
    [SerializeField]
    private Button loginClientButton;
    [SerializeField]
    private Button loginHostButton;

    [SerializeField]
    private Image progressBar;

    private float progressGage = 0f;

    
    
    private void Awake() {
        loginClientButton.onClick.AddListener(()=>{
            Login(false);
        });

        loginHostButton.onClick.AddListener(()=>{
            Login(true);
        });
    }

    void Start()
    {
        int randomInt = UnityEngine.Random.Range(0, 30);
        char r = (char)(65 + randomInt);
        usernameInput.text = $"{r}{r}{r}";
    }

    private IEnumerator StartProgressCoroutine(){
        progressBar.fillAmount = 0f;
        while(progressBar.fillAmount < 1f){
            if(progressBar == null){
                yield break;
            }
            if(progressBar.fillAmount >= progressGage){
                yield return new WaitForSeconds(0.1f);
            }
            progressBar.fillAmount += 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void SetLoginProgress(int amount){
        if(progressGage == 0){
            if(progressBar){
                StartCoroutine(StartProgressCoroutine());
            }
        }
        progressGage = (float) amount / 100;
    }
    

    /// <summary>
    /// 로그인하는 함수
    /// 클라이언트 정보에 username과 호스트 여부를 등록 
    /// </summary>
    /// <param name="isHost">호스트 여부</param>
    private void Login(bool isHost){
        ClientManager.Instance.ClientInfo.username = usernameInput.text;
        ClientManager.Instance.ClientInfo.isHost = isHost;
        ClientManager.Instance.JoinCode = joinCodeInput.text;

        joinCodeInput.enabled = false;
        usernameInput.enabled =false;

        AuthenticationUtils.Login();
    }

    
}
