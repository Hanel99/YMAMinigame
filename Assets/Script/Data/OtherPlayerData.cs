using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


[Serializable]

public class OtherPlayerData
{
    public bool isFullScreen = false;
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;

    public float bgmVolume = 0.5f;
    public float sfxVolume = 0.8f;
    public bool isBgmMute = false;
    public bool isSfxMute = false;

    public int towerTextSpeed = 0;

    public OtherPlayerData()
    {
        isFullScreen = false;
        resolutionWidth = 1920;
        resolutionHeight = 1080;

        bgmVolume = 0.5f;
        sfxVolume = 0.8f;
        isBgmMute = false;
        isSfxMute = false;
        towerTextSpeed = 0;
    }
}
