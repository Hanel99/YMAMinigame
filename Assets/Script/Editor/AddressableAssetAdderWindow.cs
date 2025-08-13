using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.IO;

public class AddressableAssetAdderWindow : OdinEditorWindow
{
    // ========== 어드레서블 일반 등록 박스 ==========
    [PropertySpace(SpaceAfter = 20)]
    [BoxGroup("어드레서블 일반 등록")]
    [FolderPath(AbsolutePath = true)]
    [LabelText("에셋 폴더 경로 (Assets 하위)")]
    public string assetFolderPath = "Assets/MyAssets";

    [BoxGroup("어드레서블 일반 등록")]
    [LabelText("어드레서블 그룹 이름")]
    public string groupName = "MyGroup";

    [BoxGroup("어드레서블 일반 등록")]
    [ValueDropdown(nameof(GetAllAddressableLabels))]
    [LabelText("어드레서블 레이블")]
    public string label = "<새 레이블 입력>";

    [BoxGroup("어드레서블 일반 등록")]
    [ShowIf("@label == \"<새 레이블 입력>\"")]
    [LabelText("새 레이블 이름")]
    public string newLabel = "NewLabel";


    [BoxGroup("어드레서블 일반 등록")]
    [Button("어드레서블 등록", ButtonSizes.Large), GUIColor(0.2f, 0.7f, 1f)]
    [PropertySpace(SpaceAfter = 20)]

    public void RegisterAssetsToAddressables()
    {
        // ... 기존 코드 그대로
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings가 설정되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(assetFolderPath))
        {
            Debug.LogError("에셋 폴더 경로를 지정하세요.");
            return;
        }

        string relativePath = assetFolderPath;
        if (assetFolderPath.StartsWith(Application.dataPath))
        {
            relativePath = "Assets" + assetFolderPath.Substring(Application.dataPath.Length);
        }

        string[] assetGuids = AssetDatabase.FindAssets("", new[] { relativePath });

        if (assetGuids.Length == 0)
        {
            Debug.LogWarning("해당 폴더에서 에셋을 찾지 못했습니다: " + relativePath);
            return;
        }

        AddressableAssetGroup group = settings.FindGroup(groupName);
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, false, false, null,
                typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
        }

        string labelToApply = GetFinalLabel();

        if (!settings.GetLabels().Contains(labelToApply))
        {
            settings.AddLabel(labelToApply);
        }

        foreach (string guid in assetGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path)) continue;

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = System.IO.Path.GetFileNameWithoutExtension(path);

            if (!entry.labels.Contains(labelToApply))
                entry.SetLabel(labelToApply, true);
        }

        AssetDatabase.SaveAssets();
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
        Debug.Log($"✅ 어드레서블 등록 완료: {assetGuids.Length}개 에셋 (레이블: {labelToApply})");
    }

    // ========== 팝업 자동 등록 박스 ==========
    [BoxGroup("팝업 자동 등록")]
    [FolderPath(AbsolutePath = true)]
    [LabelText("팝업 프리팹 루트 폴더 (Assets 하위)")]
    public string popupRootFolder = "Assets/Prefab/";

    [BoxGroup("팝업 자동 등록")]
    [LabelText("팝업 그룹 이름")]
    public string popupGroupName = "PopupGroup";

    [BoxGroup("팝업 자동 등록")]
    [ValueDropdown(nameof(GetAllAddressableLabels))]
    [LabelText("팝업 레이블")]
    public string popupLabel = "<새 레이블 입력>";

    [BoxGroup("팝업 자동 등록")]
    [ShowIf("@popupLabel == \"<새 레이블 입력>\"")]
    [LabelText("새 팝업 레이블 이름")]
    public string newPopupLabel = "PopupLabel";

    [BoxGroup("팝업 자동 등록")]
    [Button("팝업 프리팹 Addressable 등록", ButtonSizes.Large), GUIColor(0.9f, 0.6f, 0.2f)]
    public void RegisterPopupPrefabsToAddressables()
    {
        // ... 기존 코드 그대로
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings가 설정되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(popupRootFolder))
        {
            Debug.LogError("팝업 프리팹 루트 폴더를 지정하세요.");
            return;
        }

        string relativePath = popupRootFolder;
        if (popupRootFolder.StartsWith(Application.dataPath))
        {
            relativePath = "Assets" + popupRootFolder.Substring(Application.dataPath.Length);
        }

        List<string> popupPrefabGuids = new List<string>();
        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new[] { relativePath });
        foreach (var guid in allGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path).ToLower().Contains("popup"))
            {
                popupPrefabGuids.Add(guid);
            }
        }

        if (popupPrefabGuids.Count == 0)
        {
            Debug.LogWarning("popup이 포함된 프리팹을 찾지 못했습니다: " + relativePath);
            return;
        }

        AddressableAssetGroup group = settings.FindGroup(popupGroupName);
        if (group == null)
        {
            group = settings.CreateGroup(popupGroupName, false, false, false, null,
                typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
        }

        string labelToApply = popupLabel == "<새 레이블 입력>" ? newPopupLabel : popupLabel;

        if (!settings.GetLabels().Contains(labelToApply))
        {
            settings.AddLabel(labelToApply);
        }

        foreach (string guid in popupPrefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = Path.GetFileNameWithoutExtension(path);

            if (!entry.labels.Contains(labelToApply))
                entry.SetLabel(labelToApply, true);
        }

        AssetDatabase.SaveAssets();
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
        Debug.Log($"✅ 팝업 프리팹 Addressable 등록 완료: {popupPrefabGuids.Count}개 (레이블: {labelToApply})");
    }

    // ========== 헬퍼 메서드들 ==========
    private string GetFinalLabel()
    {
        return label == "<새 레이블 입력>" ? newLabel : label;
    }

    private List<string> GetAllAddressableLabels()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var labels = new List<string>();

        if (settings != null)
        {
            foreach (var lbl in settings.GetLabels())
            {
                labels.Add(lbl);
            }
        }

        labels.Add("<새 레이블 입력>");
        return labels;
    }

    public List<GameObject> GetPrefabs(string prefabName)
    {
        List<GameObject> prefabs = new List<GameObject>();

        DirectoryInfo di = new DirectoryInfo("Assets/Prefab/");
        var prefabDirectories = di.GetDirectories();

        foreach (var dir in prefabDirectories)
        {
            var fileInfos = dir.GetFiles("*.prefab");

            foreach (FileInfo file in fileInfos)
            {
                if (file.Name.Contains("meta")) continue;
                string filePath = $"{di}{dir.Name}/{file.Name}";
                HLLogger.Log($"path ; {filePath}");

                if (file.Name.Contains($"{prefabName}.prefab"))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
                    prefabs.Add(prefab);
                }
            }
        }

        return prefabs;
    }

    [MenuItem("Tools/Util Window/어드레서블 에셋 등록 윈도우")]
    private static void OpenWindow()
    {
        GetWindow<AddressableAssetAdderWindow>("어드레서블 자동 등록");
    }
}