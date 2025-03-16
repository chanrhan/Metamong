using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Vivox;
using UnityEngine;

public class UIManager : MonobehaviourSingleton<UIManager>
{
    [SerializeField]
    private Voice3DChannelUI voice3DChannelUI;

    void Update()
    {
        voice3DChannelUI.UpdateParticipantsVoiceUI();
    }

    
}
