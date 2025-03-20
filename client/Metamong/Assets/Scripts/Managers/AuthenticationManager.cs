using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticationManager : MonobehaviourSingleton<AuthenticationManager>
{

    [SerializeField]
    private Image progressBar;

    private float progressGage = 0f;


    private IEnumerator StartProgressCoroutine(){
        progressBar.fillAmount = 0f;
        while(progressBar.fillAmount < 1f){
            if(progressBar.fillAmount > progressGage){
                yield return new WaitForSeconds(0.5f);
            }
            progressBar.fillAmount += 0.02f;
            yield return new WaitForSeconds(0.02f);
        }
    }

    public void SetLoginProgress(int amount){
        if(progressGage == 0){
            StartCoroutine(StartProgressCoroutine());
        }
        progressGage = (float) amount / 100;
    }


    public async void Login(){
        SetLoginProgress(10);
        await UnityServices.InitializeAsync();

        SetLoginProgress(20);

        AuthenticationService.Instance.SignedIn += ()=>{
            Debug.Log("Signed In: " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        SetLoginProgress(30);

        // Relay

        // Vivox
        await VivoxManager.Instance.InitializeVivox();

        SetLoginProgress(95);

        // Scene Load
        GameSceneManager.Instance.LoadInGameScene();
    }
}
