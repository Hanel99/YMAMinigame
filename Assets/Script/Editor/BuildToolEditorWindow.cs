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

    [Button("\uD83D\uDD01 Addressables 다중 플랫폼 빌드", ButtonSizes.Large)]
    private void BuildAddressablesForAllPlatforms()
    {
        var originalTarget = EditorUserBuildSettings.activeBuildTarget;

        foreach (PlatformOption platform in System.Enum.GetValues(typeof(PlatformOption)))
        {
            BuildTarget buildTarget = GetBuildTarget(platform);
            if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
                EditorUserBuildSettings.SwitchActiveBuildTarget(GetBuildTargetGroup(buildTarget), buildTarget);

            Debug.Log($"\uD83D\uDD04 빌드 대상 변경: {platform}");
            SetupAndBuildAddressables(platform, Environment, AssetVersion);
        }

        EditorUserBuildSettings.SwitchActiveBuildTarget(GetBuildTargetGroup(originalTarget), originalTarget);
        Debug.Log("✅ Addressables 다중 플랫폼 빌드 완료");
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

#if UNITY_ANDROID
        string jsonPath = "D:/workspace/YMAMiniGames/YMAMinigame/Assets/Resources/Config/AppConfig.json";
        if (File.Exists(jsonPath))
        {
            string jsonText = File.ReadAllText(jsonPath);
            var jsonData = JsonUtility.FromJson<GameConfig>(jsonText);

            PlayerSettings.Android.keystoreName = jsonData.keystorePath;
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

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
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

    private string GetBuildPath()
    {
        string basePath = Path.Combine(OutputPath, Platform.ToString().ToLower(), Environment.ToString().ToLower(), AssetVersion);
        Directory.CreateDirectory(basePath);

        return Platform == PlatformOption.Android
            ? Path.Combine(basePath, "App.apk")
            : Path.Combine(basePath, "App.exe");
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



    [MenuItem("Tools/Util Window/앱 에셋 통합 빌드 툴")]
    private static void OpenWindow()
    {
        GetWindow<BuildToolEditorWindow>("통합 빌드 툴");
    }
}

#endif
