#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using Sirenix.OdinInspector.Editor;


public class ExpectedGoldWindow : OdinEditorWindow
{
    [Title("CSV 파일 선택")]
    [Sirenix.OdinInspector.FilePath(Extensions = "csv", RequireExistingPath = true)]
    public string csvFilePath;

    [Button("계산 시작", ButtonSizes.Large)]
    public void CalculateExpectedGold()
    {
        if (string.IsNullOrEmpty(csvFilePath) || !File.Exists(csvFilePath))
        {
            EditorUtility.DisplayDialog("오류", "CSV 파일을 선택해주세요.", "확인");
            return;
        }

        try
        {
            var lines = File.ReadAllLines(csvFilePath);
            if (lines.Length == 0)
            {
                EditorUtility.DisplayDialog("오류", "CSV 파일이 비어 있습니다.", "확인");
                return;
            }

            // 첫 줄: 헤더 확인
            var headers = lines[0].Split(',');
            int upIndex = System.Array.IndexOf(headers, "up");
            int stayIndex = System.Array.IndexOf(headers, "stay");
            int downIndex = System.Array.IndexOf(headers, "down");
            int costIndex = System.Array.IndexOf(headers, "requireCoin");

            if (upIndex < 0 || stayIndex < 0 || downIndex < 0 || costIndex < 0)
            {
                EditorUtility.DisplayDialog("오류", "CSV에 up, stay, down, requireCoin 열이 없습니다.", "확인");
                return;
            }

            // 헤더에 ExpectedGold 추가
            var outputLines = new List<string>();
            outputLines.Add(string.Join(",", headers) + ",ExpectedGold");

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length <= costIndex) continue;

                if (!float.TryParse(parts[upIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out float up)) up = 0;
                if (!float.TryParse(parts[stayIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out float stay)) stay = 0;
                if (!float.TryParse(parts[downIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out float down)) down = 0;
                if (!float.TryParse(parts[costIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out float cost)) cost = 0;

                double expected = ComputeExpectedGold(up, stay, down, cost);
                parts = Append(parts, expected.ToString("F2", CultureInfo.InvariantCulture));
                outputLines.Add(string.Join(",", parts));
            }

            string savePath = Path.Combine(
                Path.GetDirectoryName(csvFilePath),
                Path.GetFileNameWithoutExtension(csvFilePath) + "_with_ExpectedGold.csv"
            );
            File.WriteAllLines(savePath, outputLines);

            EditorUtility.DisplayDialog("완료", $"계산이 완료되었습니다.\n저장 경로:\n{savePath}", "확인");
            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ExpectedGoldWindow] 계산 오류: {ex}");
            EditorUtility.DisplayDialog("오류", ex.Message, "확인");
        }
    }

    private static string[] Append(string[] arr, string value)
    {
        var list = new List<string>(arr);
        list.Add(value);
        return list.ToArray();
    }

    // 기대값 계산식 (Python 버전 포팅)
    private static double ComputeExpectedGold(double A, double B, double C, double cost, int Kmax = 300)
    {
        double D = A + B + C;
        if (D <= 0) return 0;

        double[] a = new double[Kmax + 2];
        double[] b = new double[Kmax + 2];
        double[] c = new double[Kmax + 2];
        double[] d = new double[Kmax + 2];

        b[Kmax + 1] = 1.0;
        d[Kmax + 1] = 1.0;
        a[Kmax + 1] = 0.0;
        c[Kmax + 1] = 1.0;

        for (int k = Kmax; k >= 0; k--)
        {
            double denom = D + 10 * k;
            double r = B / denom;
            double s = C / denom;
            double t = (B + C) / denom;
            double u = A / denom;

            b[k] = 1 + r * b[k + 1];
            a[k] = s + r * a[k + 1];
            d[k] = 1 + t * d[k + 1];
            c[k] = u + t * c[k + 1];
        }

        double denom2 = 1 - a[0] * c[0];
        if (denom2 <= 0) return 0;

        double E0 = (a[0] * d[0] + b[0]) / denom2;
        return E0 * cost;
    }

    [MenuItem("Tools/Util Window/꿀밤 골드 계산기")]
    public static void OpenWindow()
    {
        GetWindow<ExpectedGoldWindow>("꿀밤 골드 계산기");
    }
}
#endif
