using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonobehaviourSingleton<LobbyUIManager>
{
    [SerializeField]
    private LobbyUI lobbyUI;

    public void SetLoginProgress(int amount){
        lobbyUI.SetLoginProgress(amount);
    }

}
