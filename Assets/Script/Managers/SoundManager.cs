using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSfxSource;


    [Header("Volume Settings")]
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

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

        // 볼륨 설정
        UpdateVolumes();
    }

    private AudioClip GetBGM(BGMType type)
    {
        return GameResourceManager.instance.GetBGM(type);
    }

    private AudioClip GetSFX(SFXType type)
    {
        return GameResourceManager.instance.GetSFX(type);
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

        var bgmClip = GetBGM(bgmType);
        if (bgmClip != null)
        {
            bgmSource.clip = bgmClip;
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



    #endregion

    #region 단발성 SFX 관리

    // 단발성 SFX 재생 (enum으로)
    public void PlaySFX(SFXType sfxType)
    {
        if (sfxType == SFXType.None) return;

        var sfxClip = GetSFX(sfxType);
        if (sfxClip != null)
        {
            sfxSource.PlayOneShot(sfxClip);
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

        var sfxClip = GetSFX(sfxType);
        if (sfxClip != null)
        {
            loopSfxSource.clip = sfxClip;
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

    #endregion
}