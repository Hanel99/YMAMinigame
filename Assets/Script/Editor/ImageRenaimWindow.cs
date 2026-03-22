using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class ImageRenameWindow : OdinEditorWindow
{
    [MenuItem("Tools/Util Window/이미지 이름 변경")]
    private static void OpenWindow()
    {
        GetWindow<ImageRenameWindow>().Show();
    }

    [FolderPath]
    [LabelText("이미지 폴더")]
    public string folderPath;

    [LabelText("삭제 대신 휴지통 폴더로 이동")]
    public bool useTrash = true;

    [ShowIf(nameof(useTrash))]
    public string trashFolderName = "_Trash";

    private Dictionary<int, int> mapping = new Dictionary<int, int>()
    {
        // 바꿀 숫자 -> 바뀐 숫자
        {10,9},
        {11,10},
        {12,11},
        {13,12},
        {14,13},
        {15,14},
        {16,15},
        {17,16},
        {18,17},
        {19,18},
        {20,19},
        {21,20},
        {22,21},
        {23,22},
        {24,23},
        {25,24},
        {26,25},
        {27,26}
    };

    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
    private void 실행()
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            Debug.LogError("폴더 경로가 잘못되었습니다.");
            return;
        }

        string trashPath = Path.Combine(folderPath, trashFolderName);
        if (useTrash && !Directory.Exists(trashPath))
            Directory.CreateDirectory(trashPath);

        // 1단계: .tmp로 변경하여 이름 프리징 (이미지 + 메타파일 세트 이동)
        string[] files = Directory.GetFiles(folderPath);
        List<string> tmpFileList = new List<string>();

        foreach (var file in files)
        {
            string ext = Path.GetExtension(file).ToLower();
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg") continue;

            string fileNameOnly = Path.GetFileNameWithoutExtension(file);
            if (fileNameOnly.Length < 4 || !int.TryParse(fileNameOnly, out _)) continue;

            int major = int.Parse(fileNameOnly.Substring(0, 2));
            if (!mapping.ContainsKey(major))
            {
                HandleDelete(file, trashPath);
                continue;
            }

            // 이미지 이동
            string tmpPath = file + ".tmp";
            if (SafeMove(file, tmpPath))
            {
                tmpFileList.Add(tmpPath);
                // 메타파일도 함께 .tmp.meta로 이동하여 유니티 간섭 방지
                string srcMeta = file + ".meta";
                if (File.Exists(srcMeta))
                {
                    SafeMove(srcMeta, tmpPath + ".meta");
                }
            }
        }

        // 2단계: .tmp -> 최종 이름으로 변경 (이미지 + 메타파일 세트 이동)
        foreach (var tmpFile in tmpFileList)
        {
            string dir = Path.GetDirectoryName(tmpFile);
            string tmpFileName = Path.GetFileName(tmpFile); // 0401.png.tmp


            string originalFileNameWithExt = tmpFileName.Substring(0, tmpFileName.Length - 4); // 0401.png
            string nameOnly = Path.GetFileNameWithoutExtension(originalFileNameWithExt); // 0401
            string extOnly = Path.GetExtension(originalFileNameWithExt); // .png

            int major = int.Parse(nameOnly.Substring(0, 2));
            string minor = nameOnly.Substring(2, 2);
            int newMajor = mapping[major];

            string finalName = $"{newMajor:00}{minor}{extOnly}";
            string finalPath = Path.Combine(dir, finalName);

            // 이미지 최종 이동
            if (SafeMove(tmpFile, finalPath))
            {
                // 메타파일 최종 이동
                string tmpMeta = tmpFile + ".meta";
                if (File.Exists(tmpMeta))
                {
                    SafeMove(tmpMeta, finalPath + ".meta");
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("이름 변경 처리가 완료되었습니다.");
    }

    private bool SafeMove(string src, string dst)
    {
        try
        {
            if (!File.Exists(src)) return false;
            if (File.Exists(dst)) File.Delete(dst);
            File.Move(src, dst);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"파일 이동 실패: {src} -> {dst}\n{e.Message}");
            return false;
        }
    }

    private void HandleDelete(string file, string trashPath)
    {
        string metaFile = file + ".meta";
        string fileName = Path.GetFileName(file);

        if (useTrash)
        {
            string dest = Path.Combine(trashPath, fileName);
            SafeMove(file, dest);
            if (File.Exists(metaFile)) SafeMove(metaFile, dest + ".meta");
            Debug.Log($"휴지통 이동: {fileName}");
        }
        else
        {
            if (File.Exists(file)) File.Delete(file);
            if (File.Exists(metaFile)) File.Delete(metaFile);
            Debug.Log($"삭제: {fileName}");
        }
    }
}