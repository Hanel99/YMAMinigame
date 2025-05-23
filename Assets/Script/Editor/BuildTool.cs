#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Linq;

public class BuildToolEditorWindow : OdinEditorWindow
{
    [MenuItem("Tools/Odin Build Tool")]
    private static void ShowWindow()
    {
        GetWindow<BuildToolEditorWindow>("Build Tool");
    }

    [LabelText("Enable DevTest Symbol")]
    [PropertyOrder(-1)]
    public bool enableDevTest;

    [PropertySpace(SpaceBefore = 10)]
    [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
    public void BuildWindows()
    {
        BuildWithTarget(BuildTarget.StandaloneWindows64);
    }

    [Button(ButtonSizes.Large), GUIColor(0, 0.6f, 1)]
    public void BuildAndroid()
    {
        BuildWithTarget(BuildTarget.Android);
    }

    private void BuildWithTarget(BuildTarget target)
    {
        var namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target));

        // 현재 심볼 가져오기
        string currentSymbols = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
        var symbolList = currentSymbols.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

        if (enableDevTest)
        {
            // DevTest가 없으면 추가
            if (!symbolList.Contains("DevTest"))
                symbolList.Add("DevTest");
        }
        else
        {
            // DevTest가 있으면 제거
            symbolList.RemoveAll(s => s == "DevTest");
        }

        string newSymbols = string.Join(";", symbolList);


        PlayerSettings.SetScriptingDefineSymbols(namedTarget, newSymbols);







        // 빌드 위치 선택
        string path = EditorUtility.SaveFolderPanel($"Choose location for {target} build", "", "");
        if (string.IsNullOrEmpty(path)) return;

        // 빌드 설정
        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = path + (target == BuildTarget.Android ? "/Game.apk" : "/Game.exe"),
            target = target,
            options = BuildOptions.None
        };


        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
            Debug.Log($"✅ Build succeeded: {summary.totalSize} bytes");
        else
            Debug.LogError("❌ Build failed");
    }
}
#endif
