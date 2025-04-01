
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthenticationManager : MonobehaviourSingleton<AuthenticationManager>
{


    public async Task Authenticate(){
        LobbyUIManager.Instance.SetLoginProgress(10);
        await UnityServices.InitializeAsync();

        LobbyUIManager.Instance.SetLoginProgress(20);

        AuthenticationService.Instance.SignedIn += ()=>{
            Debug.Log("Signed In: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        LobbyUIManager.Instance.SetLoginProgress(30);
    }

    public async void Login(){
        await Authenticate();

        // Vivox
        await VivoxManager.Instance.InitializeVivox();

        LobbyUIManager.Instance.SetLoginProgress(100);

        // Scene Load
        GameSceneManager.Instance.LoadInGameScene();
    }
}
