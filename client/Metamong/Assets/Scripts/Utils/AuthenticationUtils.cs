
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthenticationUtils
{
    private static bool isLogined = false;

    public static async Task Authenticate(){
        LobbyUIManager.Instance.SetLoginProgress(10);
        await UnityServices.InitializeAsync();

        LobbyUIManager.Instance.SetLoginProgress(20);

        AuthenticationService.Instance.SignedIn += ()=>{
            Debug.Log("Signed In: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
        LobbyUIManager.Instance.SetLoginProgress(30);
    }

    public static async void Login(){
        if(isLogined){
            Debug.Log("이미 로그인 상태입니다.");
        }else{
            await Authenticate();
            isLogined = true;

            // Vivox
            await VivoxManager.Instance.InitializeVivox();

            LobbyUIManager.Instance.SetLoginProgress(100);
        }

        // Scene Load
        GameSceneManager.Instance.LoadInGameScene();
    }
}
