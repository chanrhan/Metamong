using System.Collections;
using UnityEngine;

public class UIManager : MonobehaviourSingleton<UIManager>
{
    [SerializeField]
    private Voice3DChannelUI voice3DChannelUI;
    private float voice3DChannelUIUpdateInterval = 0.1f;

    void Start()
    {
        StartCoroutine(Update3DVoiceChannelUICoroutine());       
    }

    private IEnumerator Update3DVoiceChannelUICoroutine(){
        while(true){
            voice3DChannelUI.UpdateUI();
            yield return new WaitForSeconds(voice3DChannelUIUpdateInterval);
        }
    }
    
}
