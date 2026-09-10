using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Channels;
using Unity.Services.Vivox;
using UnityEngine;

[Serializable]
public class Channel3DSetting
{
    [SerializeField]
    private int audibleDistance = 32;
    [SerializeField]
    private int conversationalDistance = 1;
    [SerializeField]
    private float audioFadeIntensityByDistance = 1.0f;
    [SerializeField]
    private AudioFadeModel audioFadeModel = AudioFadeModel.InverseByDistance;

    public Channel3DProperties GetChannel3DSetting(){
        return new Channel3DProperties(audibleDistance, conversationalDistance, audioFadeIntensityByDistance, audioFadeModel);
    }
}
