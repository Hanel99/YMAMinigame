#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
// using Renci.SshNet; // SSH.NET (SFTP)

public class BuildToolEditorWindow : OdinEditorWindow
{
    public enum EnvOption { Dev, Live }
    public enum PlatformOption { Android, Windows }

    [Title("\uD83E\uDDF1 Addressables + 앱 빌드 툴", TitleAlignment = TitleAlignments.Centered)]

    [EnumToggleButtons, LabelText("빌드 환경")]
    public EnvOption Environment = EnvOption.Dev;

    [EnumToggleButtons, LabelText("플랫폼")]
    public PlatformOption Platform = PlatformOption.Android;

    [LabelText("에셋 번들 버전"), DelayedProperty]
    public string AssetVersion = "v0.5.0";
    [LabelText("앱 버전"), DelayedProperty]
    public string AppVersion = "0.5.0";

    [FolderPath(AbsolutePath = true), LabelText("출력 폴더")]
    public string OutputPath = "Builds/";

    [Title("\uD83D\uDCE1 NAS 업로드 (SFTP)")]
    [ToggleLeft]
    public bool UploadToNAS = false;


    // private
    private const string BUILD_COUNT_KEY = "BuildCount";




    [MenuItem("Tools/Util Window/앱 에셋 통합 빌드 툴")]
    private static void OpenWindow()
    {
        GetWindow<BuildToolEditorWindow>("통합 빌드 툴");
    }

    [Button("\uD83D\uDCE6 Addressables만 빌드", ButtonSizes.Large)]
    private void BuildAddressablesOnly()
    {
        SetupAndBuildAddressables(Platform, Environment, AssetVersion);
    }

    [Button("\uD83D\uDE80 앱 빌드 + Addressables 빌드", ButtonSizes.Large)]
    private void BuildFull()
    {
        SetupAndBuildAddressables(Platform, Environment, AssetVersion);
        string targetPath = BuildPlayerApp();

        if (UploadToNAS)
        {
            string folderToUpload = Path.GetDirectoryName(targetPath);
            UploadToNASWithSFTP(folderToUpload);
        }
    }

    private void SetupAndBuildAddressables(PlatformOption platform, EnvOption env, string version)
    {
        string envStr = env.ToString().ToLower();
        string platformStr = platform.ToString().ToLower();

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var profileSettings = settings.profileSettings;
        string currentProfileId = settings.activeProfileId;
        string currentProfileName = profileSettings.GetProfileName(currentProfileId);


        profileSettings.SetValue(currentProfileId, "Platform", platformStr);
        profileSettings.SetValue(currentProfileId, "Env", envStr);
        profileSettings.SetValue(currentProfileId, "AssetVersion", version);

        SetDefineSymbol(env.ToString().ToUpper());


        // 1. 현재 플랫폼과 다르면 에디터 전환
        BuildTarget buildTarget = GetBuildTarget(platform);
        if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
            EditorUserBuildSettings.SwitchActiveBuildTarget(GetBuildTargetGroup(buildTarget), buildTarget);

        // 2. Addressables 빌드 수행
        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent();
        Debug.Log($"✅ Addressables 빌드 완료: {platformStr}/{envStr}/{version}");
    }

    private void SetDefineSymbol(string symbol)
    {
        BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group));

        List<string> defineList = new List<string>(defines.Split(';'));
        defineList.RemoveAll(s => s == "DEV" || s == "LIVE");
        defineList.Add(symbol);

        NamedBuildTarget namedTarget = NamedBuildTarget.FromBuildTargetGroup(group);
        PlayerSettings.SetScriptingDefineSymbols(namedTarget, string.Join(";", defineList));
        Debug.Log($"🧩 디파인 심볼 설정: {string.Join(";", defineList)}");
    }

    private string BuildPlayerApp()
    {
        string buildPath = GetBuildPath();

        // 앱 버전 설정
        PlayerSettings.bundleVersion = AppVersion;
        PlayerSettings.productName = "YMA Mini Game";

#if UNITY_ANDROID
        string jsonPath = "D:/workspace/YMAMiniGames/YMAMinigame/Assets/Resources/Config/AppConfig.json";
        if (File.Exists(jsonPath))
        {
            string jsonText = File.ReadAllText(jsonPath);
            var jsonData = JsonUtility.FromJson<GameConfig>(jsonText);

            // PlayerSettings.Android.keystoreName = jsonData.keystorePath;
            PlayerSettings.Android.keystorePass = jsonData.keystorePassword;
            PlayerSettings.Android.keyaliasName = jsonData.keyAlias;
            PlayerSettings.Android.keyaliasPass = jsonData.keyPassword;
        }
#endif

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = buildPath,
            target = GetBuildTarget(Platform),
            options = BuildOptions.None
        };

#if UNITY_ANDROID
        SetupAndroidSettings();
#elif UNITY_STANDALONE_WIN
        SetupWindowsSettings();
#endif

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            IncrementBuildCountForToday();
            EditorUtility.DisplayDialog("✅ 빌드 성공", $"경로: {buildPath}", "확인");
            Debug.Log($"✅ 앱 빌드 완료: {summary.totalSize / 1048576f:0.00} MB");

            // 빌드 폴더 열기
            EditorUtility.RevealInFinder(Path.GetDirectoryName(buildPath));
        }
        else
        {
            EditorUtility.DisplayDialog("❌ 빌드 실패", summary.result.ToString(), "확인");
            Debug.LogError("❌ 앱 빌드 실패");
        }

        return buildPath;
    }

    private void SetupWindowsSettings()
    {
        // Windows 전용 설정
        PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Standalone, Il2CppCodeGeneration.OptimizeSpeed);
        // PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard_2_0);

        // Windows 아키텍처
        PlayerSettings.SetArchitecture(NamedBuildTarget.Standalone, 1); // x64

        // 최적화 설정
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Standalone, ManagedStrippingLevel.Medium);

        // 그래픽 설정
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new UnityEngine.Rendering.GraphicsDeviceType[]
        {
        UnityEngine.Rendering.GraphicsDeviceType.Direct3D11,
        UnityEngine.Rendering.GraphicsDeviceType.Vulkan
        });

        Debug.Log("Windows settings applied");
    }

    private void SetupAndroidSettings()
    {
        // Android 필수 설정
        PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Android, Il2CppCodeGeneration.OptimizeSpeed);
        // PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Android, ApiCompatibilityLevel.NET_Standard_2_0);

        // Android 아키텍처 (ARM64 필수 - Google Play 요구사항)
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // 최적화 설정
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.High);
        PlayerSettings.stripEngineCode = true;

        // Android 빌드 설정
        EditorUserBuildSettings.buildAppBundle = false; // APK 빌드 (AAB를 원하면 true)
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        EditorUserBuildSettings.development = false;

        // Android API 레벨 설정
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23; // API 23 (Android 6.0)
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto; // 최신 API 자동

        // 그래픽 설정
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[]
        {
        UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3,
        UnityEngine.Rendering.GraphicsDeviceType.Vulkan
        });

        Debug.Log("Android settings applied");
    }


    private string GetBuildPath()
    {
        string name = GenerateBuildFilePath();
        string basePath = Path.Combine(OutputPath, Platform.ToString().ToLower(), Environment.ToString().ToLower(), AssetVersion, name);
        Directory.CreateDirectory(basePath);
#if LIVE
        name = "YMAMiniGame";
#endif
        return Platform == PlatformOption.Android ? Path.Combine(basePath, $"{name}.apk") : Path.Combine(basePath, $"{name}.exe");
    }

    private string[] GetEnabledScenes()
    {
        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenes.Add(scene.path);
        }
        return scenes.ToArray();
    }

    private BuildTarget GetBuildTarget(PlatformOption platform)
    {
        return platform == PlatformOption.Android ? BuildTarget.Android : BuildTarget.StandaloneWindows64;
    }

    private BuildTargetGroup GetBuildTargetGroup(BuildTarget target)
    {
        return target switch
        {
            BuildTarget.Android => BuildTargetGroup.Android,
            BuildTarget.StandaloneWindows => BuildTargetGroup.Standalone,
            BuildTarget.StandaloneWindows64 => BuildTargetGroup.Standalone,
            _ => BuildTargetGroup.Unknown
        };
    }

    private void UploadToNASWithSFTP(string folderPath)
    {

    }

    // 빌드 파일명 생성 함수
    private string GenerateBuildFilePath()
    {
        StringBuilder sb = new StringBuilder();

#if DEV
        sb.Append("DEV");
#elif LIVE
        sb.Append("LIVE");
#endif

        // 현재 날짜를 YYMMDD 형식으로 변환
        sb.Append("_");
        sb.Append(DateTime.Now.ToString("yyMMdd"));

        // 빌드 카운트 가져오기
        int buildCount = GetBuildCountForToday() + 1; // 다음 빌드 번호
        sb.Append("_");
        sb.Append(buildCount.ToString("D3")); // 3자리로 포맷팅

        return sb.ToString();
    }

    // 빌드 카운트 가져오기
    private int GetBuildCountForToday()
    {
        string today = DateTime.Now.ToString("yyMMdd");
        string key = $"{BUILD_COUNT_KEY}_{today}";
        return EditorPrefs.GetInt(key, 0);
    }

    // 빌드 카운트 증가
    private void IncrementBuildCountForToday()
    {
        string today = DateTime.Now.ToString("yyMMdd");
        string key = $"{BUILD_COUNT_KEY}_{today}";
        int currentCount = EditorPrefs.GetInt(key, 0);
        EditorPrefs.SetInt(key, currentCount + 1);
    }


}

#endif
