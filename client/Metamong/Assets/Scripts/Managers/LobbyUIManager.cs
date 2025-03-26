using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonobehaviourSingleton<LobbyUIManager>
{
    [SerializeField]
    private Image progressBar;

    private float progressGage = 0f;

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

}
