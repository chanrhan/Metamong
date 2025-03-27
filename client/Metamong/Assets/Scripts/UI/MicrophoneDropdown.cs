using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class MicrophoneDropdown : MonoBehaviour
{
    private Dropdown dropdown;

    public string microphoneDefaultLabel = "Default System Device";

    private IEnumerable<string> AvailableInputDevices => Microphone.devices;

    void Awake()
    {
        dropdown = GetComponent<Dropdown>();
        if(dropdown != null)
        {
            dropdown.options = AvailableInputDevices
                .Select(text => new Dropdown.OptionData(text))
                .ToList();
            // dropdown.value = dropdown.options
            //     .FindIndex(op => op.text == microphoneDefaultLabel);
            dropdown.onValueChanged.AddListener(OnMicrophoneChanged);

            // Debug.Log("Mics: "+string.Join(",", dropdown.options.Select(v=>v.text)));
        }
    }

    private void OnMicrophoneChanged(int index){
        if (dropdown == null) return;

        var opt = dropdown.options[index];
        string selectedMicDevice = opt.text == microphoneDefaultLabel ? null : opt.text;
        Debug.Log("Selected Mic Device: " + selectedMicDevice);

        if(!VivoxService.Instance.AvailableInputDevices.Select(v=>v.DeviceName).Contains(selectedMicDevice)){
            Debug.LogWarning("Device is not found! : " + selectedMicDevice);
            return;
        }
        
        VivoxManager.Instance.InputDeviceValueChanged(selectedMicDevice);
        STTManager.Instance.OnInputDeviceChanged(selectedMicDevice);
    }


}
