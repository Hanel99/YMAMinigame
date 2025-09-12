using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]

public class LocalPlayerData
{
    public float bgmVolume = 0.5f;
    public float sfxVolume = 0.8f;
    public bool isBgmMute = false;
    public bool isSfxMute = false;

    public LocalPlayerData()
    {
        bgmVolume = 0.5f;
        sfxVolume = 0.8f;
        isBgmMute = false;
        isSfxMute = false;
    }
}
