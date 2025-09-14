using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]

public class OtherPlayerData
{
    public float bgmVolume = 0.5f;
    public float sfxVolume = 0.8f;
    public bool isBgmMute = false;
    public bool isSfxMute = false;

    public int towerTextSpeed = 0;

    public OtherPlayerData()
    {
        bgmVolume = 0.5f;
        sfxVolume = 0.8f;
        isBgmMute = false;
        isSfxMute = false;
        towerTextSpeed = 0;
    }
}
