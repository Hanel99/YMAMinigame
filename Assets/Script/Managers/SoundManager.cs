using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSfxSource;

    [Header("BGM Clips")]
    [SerializeField] private BGMClipData[] bgmClips;

    [Header("SFX Clips")]
    [SerializeField] private SFXClipData[] sfxClips;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    // BGM과 SFX 딕셔너리 (enum으로 접근하기 위함)
    private Dictionary<BGMType, AudioClip> bgmDict = new Dictionary<BGMType, AudioClip>();
    private Dictionary<SFXType, AudioClip> sfxDict = new Dictionary<SFXType, AudioClip>();

    // Inspector에서 설정하기 위한 직렬화 가능한 클래스들
    [System.Serializable]
    public class BGMClipData
    {
        public BGMType bgmType;
        public AudioClip clip;
    }

    [System.Serializable]
    public class SFXClipData
    {
        public SFXType sfxType;
        public AudioClip clip;
    }


    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        InitializeSoundManager();
        DontDestroyOnLoad(this.gameObject);
    }

    private void InitializeSoundManager()
    {
        // AudioSource가 할당되지 않은 경우 자동 생성
        if (bgmSource == null)
        {
            GameObject bgmGO = new GameObject("BGM Source");
            bgmGO.transform.SetParent(transform);
            bgmSource = bgmGO.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFX Source");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        if (loopSfxSource == null)
        {
            GameObject loopSfxGO = new GameObject("Loop SFX Source");
            loopSfxGO.transform.SetParent(transform);
            loopSfxSource = loopSfxGO.AddComponent<AudioSource>();
            loopSfxSource.loop = true;
            loopSfxSource.playOnAwake = false;
        }

        // 오디오 클립들을 딕셔너리에 저장
        InitializeAudioDictionaries();

        // 볼륨 설정
        UpdateVolumes();
    }

    private void InitializeAudioDictionaries()
    {
        // BGM 딕셔너리 초기화
        bgmDict.Clear();
        foreach (var bgmData in bgmClips)
        {
            if (bgmData.clip != null && bgmData.bgmType != BGMType.None)
            {
                if (bgmDict.ContainsKey(bgmData.bgmType))
                {
                    Debug.LogWarning($"Duplicate BGM type: {bgmData.bgmType}");
                }
                else
                {
                    bgmDict.Add(bgmData.bgmType, bgmData.clip);
                }
            }
        }

        // SFX 딕셔너리 초기화
        sfxDict.Clear();
        foreach (var sfxData in sfxClips)
        {
            if (sfxData.clip != null && sfxData.sfxType != SFXType.None)
            {
                if (sfxDict.ContainsKey(sfxData.sfxType))
                {
                    Debug.LogWarning($"Duplicate SFX type: {sfxData.sfxType}");
                }
                else
                {
                    sfxDict.Add(sfxData.sfxType, sfxData.clip);
                }
            }
        }
    }

    // 볼륨 업데이트
    public void UpdateVolumes()
    {
        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;
        loopSfxSource.volume = sfxVolume;
    }

    #region BGM 관리

    // BGM 재생 (enum으로)
    public void PlayBGM(BGMType bgmType)
    {
        if (bgmType == BGMType.None) return;

        if (bgmDict.ContainsKey(bgmType))
        {
            bgmSource.clip = bgmDict[bgmType];
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM type '{bgmType}' not found!");
        }
    }

    // BGM 재생 (클립으로) - 기존 호환성 유지
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // BGM 정지
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // BGM 페이드아웃
    public void FadeOutBGM(float fadeTime = 1f)
    {
        StartCoroutine(FadeOutCoroutine(bgmSource, fadeTime));
    }

    // 현재 재생 중인 BGM 타입 가져오기
    public BGMType GetCurrentBGMType()
    {
        if (bgmSource.clip == null) return BGMType.None;

        foreach (var kvp in bgmDict)
        {
            if (kvp.Value == bgmSource.clip)
            {
                return kvp.Key;
            }
        }
        return BGMType.None;
    }

    #endregion

    #region 단발성 SFX 관리

    // 단발성 SFX 재생 (enum으로)
    public void PlaySFX(SFXType sfxType)
    {
        if (sfxType == SFXType.None) return;

        if (sfxDict.ContainsKey(sfxType))
        {
            sfxSource.PlayOneShot(sfxDict[sfxType]);
        }
        else
        {
            Debug.LogWarning($"SFX type '{sfxType}' not found!");
        }
    }

    // 단발성 SFX 재생 (클립으로) - 기존 호환성 유지
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }

    #endregion

    #region 루프 SFX 관리

    // 루프 SFX 재생 (enum으로)
    public void PlayLoopSFX(SFXType sfxType)
    {
        if (sfxType == SFXType.None) return;

        if (sfxDict.ContainsKey(sfxType))
        {
            loopSfxSource.clip = sfxDict[sfxType];
            loopSfxSource.Play();
        }
        else
        {
            Debug.LogWarning($"Loop SFX type '{sfxType}' not found!");
        }
    }

    // 루프 SFX 재생 (클립으로) - 기존 호환성 유지
    public void PlayLoopSFX(AudioClip clip)
    {
        if (clip == null) return;

        loopSfxSource.clip = clip;
        loopSfxSource.Play();
    }

    // 루프 SFX 정지
    public void StopLoopSFX()
    {
        loopSfxSource.Stop();
    }

    // 루프 SFX 페이드아웃
    public void FadeOutLoopSFX(float fadeTime = 1f)
    {
        StartCoroutine(FadeOutCoroutine(loopSfxSource, fadeTime));
    }

    #endregion

    #region 전체 제어

    // 모든 사운드 정지
    public void StopAllSounds()
    {
        StopBGM();
        sfxSource.Stop();
        StopLoopSFX();
    }

    // 모든 사운드 일시정지
    public void PauseAllSounds()
    {
        bgmSource.Pause();
        sfxSource.Pause();
        loopSfxSource.Pause();
    }

    // 모든 사운드 재개
    public void ResumeAllSounds()
    {
        bgmSource.UnPause();
        sfxSource.UnPause();
        loopSfxSource.UnPause();
    }

    #endregion

    #region 유틸리티

    // 페이드아웃 코루틴
    private IEnumerator FadeOutCoroutine(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;
        float currentTime = 0;

        while (currentTime < fadeTime)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, currentTime / fadeTime);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // 볼륨 복원
    }

    // 볼륨 설정 (런타임에서 사용)
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
        loopSfxSource.volume = sfxVolume;
    }

    // 특정 타입의 클립이 등록되어 있는지 확인
    public bool HasBGM(BGMType bgmType)
    {
        return bgmDict.ContainsKey(bgmType);
    }

    public bool HasSFX(SFXType sfxType)
    {
        return sfxDict.ContainsKey(sfxType);
    }

    #endregion
}