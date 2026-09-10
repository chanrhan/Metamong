using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChannelInfoPanel : MonoBehaviour
{
    [SerializeField]
    private Button copyChannelCodeButton;

    private TextMeshProUGUI channelCode;

    public string ChannelCode{
        set{
            channelCode.text = value;
        }  
    }

    void Awake()
    {
        if(copyChannelCodeButton != null){
            channelCode = copyChannelCodeButton.GetComponentInChildren<TextMeshProUGUI>();

            copyChannelCodeButton.onClick.AddListener(()=>{
                GUIUtility.systemCopyBuffer = channelCode.text;
                Debug.Log("채널 코드가 클립보드에 복사되었습니다!");
            });
        }       
    }
}
