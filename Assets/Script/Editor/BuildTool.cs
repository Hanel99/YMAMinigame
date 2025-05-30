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
    public enum EnvOption { Dev, Test, Real }
    public enum PlatformOption { Android, Windows }

    [Title("\uD83E\uDDF1 Addressables + 앱 빌드 툴", TitleAlignment = TitleAlignments.Centered)]

    [EnumToggleButtons, LabelText("빌드 환경")]
    public EnvOption Environment = EnvOption.Dev;

    [EnumToggleButtons, LabelText("플랫폼")]
    public PlatformOption Platform = PlatformOption.Android;

    [LabelText("에셋 번들 버전"), DelayedProperty]
    public string Version = "v1.0.0";

    [FolderPath(AbsolutePath = true), LabelText("출력 폴더")]
    public string OutputPath = "Builds/";

    [Title("\uD83D\uDCE1 NAS 업로드 (SFTP)")]
    [ToggleLeft]
    public bool UploadToNAS = false;

    [ShowIf("UploadToNAS")]
    [LabelText("SFTP 호스트")]
    public string SftpHost = "192.168.0.100";

    [ShowIf("UploadToNAS")]
    [LabelText("SFTP 포트")]
    public int SftpPort = 22;

    [ShowIf("UploadToNAS")]
    [LabelText("SFTP 사용자명")]
    public string SftpUser = "admin";

    [ShowIf("UploadToNAS")]
    [LabelText("SFTP 비밀번호")]
    public string SftpPassword = "password";

    [ShowIf("UploadToNAS")]
    [LabelText("NAS 업로드 경로")]
    public string SftpRemotePath = "/volume1/builds/";

    [Button("\uD83D\uDCE6 Addressables만 빌드", ButtonSizes.Large)]
    private void BuildAddressablesOnly()
    {
        SetupAndBuildAddressables(Platform, Environment, Version);
    }

    [Button("\uD83D\uDE80 앱 빌드 + Addressables 빌드", ButtonSizes.Large)]
    private void BuildFull()
    {
        SetupAndBuildAddressables(Platform, Environment, Version);
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
            SetupAndBuildAddressables(platform, Environment, Version);
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

        AddressableAssetSettings.BuildPlayerContent();
        Debug.Log($"✅ Addressables 빌드 완료: {platformStr}/{envStr}/{version}");
    }

    private void SetDefineSymbol(string symbol)
    {
        BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group));

        List<string> defineList = new List<string>(defines.Split(';'));
        defineList.RemoveAll(s => s == "DEV" || s == "TEST" || s == "REAL");
        defineList.Add(symbol);

        NamedBuildTarget namedTarget = NamedBuildTarget.FromBuildTargetGroup(group);
        PlayerSettings.SetScriptingDefineSymbols(namedTarget, string.Join(";", defineList));
        Debug.Log($"🧩 디파인 심볼 설정: {string.Join(";", defineList)}");
    }

    private string BuildPlayerApp()
    {
        string buildPath = GetBuildPath();

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
        string basePath = Path.Combine(OutputPath, Platform.ToString().ToLower(), Environment.ToString().ToLower(), Version);
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
        Debug.Log($"\uD83D\uDCE1 SFTP 업로드 시작: {folderPath} → {SftpRemotePath}");

        // using (var sftp = new SftpClient(SftpHost, SftpPort, SftpUser, SftpPassword))
        // {
        //     try
        //     {
        //         sftp.Connect();

        //         foreach (var file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
        //         {
        //             string relativePath = file.Substring(folderPath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        //             string remotePath = Path.Combine(SftpRemotePath, relativePath).Replace("\\", "/");

        //             string remoteDir = Path.GetDirectoryName(remotePath).Replace("\\", "/");
        //             EnsureRemoteDirectory(sftp, remoteDir);

        //             using (var fileStream = File.OpenRead(file))
        //             {
        //                 sftp.UploadFile(fileStream, remotePath, true);
        //                 Debug.Log($"\uD83D\uDCC4 업로드 완료: {remotePath}");
        //             }
        //         }

        //         sftp.Disconnect();
        //         EditorUtility.DisplayDialog("\uD83D\uDCE1 NAS 업로드 완료", $"NAS 경로: {SftpRemotePath}", "확인");
        //     }
        //     catch (System.Exception ex)
        //     {
        //         Debug.LogError($"❌ SFTP 업로드 실패: {ex.Message}");
        //         EditorUtility.DisplayDialog("❌ NAS 업로드 실패", ex.Message, "확인");
        //     }
        // }
    }

    // private void EnsureRemoteDirectory(SftpClient client, string path)
    // {
    //     string[] parts = path.Split('/');
    //     string current = "";
    //     foreach (var part in parts)
    //     {
    //         if (string.IsNullOrEmpty(part)) continue;
    //         current += "/" + part;
    //         if (!client.Exists(current))
    //             client.CreateDirectory(current);
    //     }
    // }

    protected override void OnGUI()
    {
        // EditorGUILayout.HelpBox(
        //     $"\uD83D\uDEE0️ 현재 세팅\n▶ 플랫폼: {Platform}\n▶ 환경: {Environment}\n▶ 버전: {Version}\n▶ Scripting Define: {Environment.ToString().ToUpper()}",
        //     MessageType.Info);

        // base.OnImGUI();
    }

    [MenuItem("Tools/\uD83E\uDDF1 통합 빌드 툴 (Odin + SFTP)")]
    private static void OpenWindow()
    {
        GetWindow<BuildToolEditorWindow>("통합 빌드 툴");
    }
}

#endif
