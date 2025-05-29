using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.GUI;

public class AddressableAssetAdder : OdinEditorWindow
{
    [Title("📁 Addressable 에셋 자동 등록기")]

    [FolderPath(AbsolutePath = true)]
    [LabelText("에셋 폴더 경로 (Assets 하위)")]
    public string assetFolderPath = "Assets/MyAssets";

    [LabelText("어드레서블 그룹 이름")]
    public string groupName = "MyGroup";

    [ValueDropdown(nameof(GetAllAddressableLabels))]
    [LabelText("어드레서블 레이블")]
    public string label = "<새 레이블 입력>";

    [ShowIf("@label == \"<새 레이블 입력>\"")]
    [LabelText("새 레이블 이름")]
    public string newLabel = "NewLabel";

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

    [Button("어드레서블 등록", ButtonSizes.Large), GUIColor(0.2f, 0.7f, 1f)]
    public void RegisterAssetsToAddressables()
    {
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

    [MenuItem("Tools/Addressables/자동 등록 윈도우")]
    private static void OpenWindow()
    {
        GetWindow<AddressableAssetAdder>("어드레서블 자동 등록");
    }
}
