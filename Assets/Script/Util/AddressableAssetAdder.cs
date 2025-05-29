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
    [FolderPath]
    [LabelText("에셋 폴더 경로")]
    public string assetFolderPath = "Assets/MyAssets";

    [LabelText("어드레서블 그룹 이름")]
    public string groupName = "MyGroup";

    [LabelText("어드레서블 레이블")]
    public string label = "MyLabel";

    [Button("어드레서블 등록")]
    public void RegisterAssetsToAddressables()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings가 설정되지 않았습니다.");
            return;
        }

        string[] assetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });

        // 그룹이 존재하는지 확인하고 없으면 생성
        AddressableAssetGroup group = settings.FindGroup(groupName);
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
        }

        foreach (string guid in assetGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (AssetDatabase.IsValidFolder(path)) continue; // 폴더는 제외

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = System.IO.Path.GetFileNameWithoutExtension(path);

            if (!entry.labels.Contains(label))
                entry.SetLabel(label, true);
        }

        AssetDatabase.SaveAssets();
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
        Debug.Log($"어드레서블 등록 완료: {assetGuids.Length}개 에셋");
    }

    [MenuItem("Tools/Addressables/자동 등록 윈도우")]
    private static void OpenWindow()
    {
        GetWindow<AddressableAssetAdder>("어드레서블 자동 등록");
    }
}

