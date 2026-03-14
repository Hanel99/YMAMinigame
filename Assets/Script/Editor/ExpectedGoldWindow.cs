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

            // 헤더에 ExpectedGold 및 ExpectedGold_NoDown 추가
            var outputLines = new List<string>();
            outputLines.Add(string.Join(",", headers) + ",ExpectedGold,ExpectedGold_NoDown");

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length <= costIndex) continue;

                if (!float.TryParse(parts[upIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out float up)) up = 0;
                if (!float.TryParse(parts[stayIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out float stay)) stay = 0;
                if (!float.TryParse(parts[downIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out float down)) down = 0;
                if (!float.TryParse(parts[costIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out float cost)) cost = 0;

                double expected = ComputeExpectedGold(up, stay, down, cost);
                double expectedNoDown = ComputeExpectedGold_NoDown(up, stay, down, cost);
                
                parts = Append(parts, expected.ToString("F2", CultureInfo.InvariantCulture));
                parts = Append(parts, expectedNoDown.ToString("F2", CultureInfo.InvariantCulture));
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

    /// <summary>
    /// 등급 상승 코인 기대값 계산
    /// 규칙:
    /// 1. 상승(A), 유지(B), 하락(C) 가중치 합을 전체 범위로 함.
    /// 2. 하락 결과 시 등급이 한 단계 하락 (L -> L-1).
    /// 3. 하락된 상태(L-1)에서는 하락 결과가 나와도 유지 처리 (추가 하락 없음).
    /// 4. 목표는 현재 등급(L)에서 상위 등급(L+1)으로 도달하는 것.
    /// </summary>
    private static double ComputeExpectedGold(double A, double B, double C, double cost)
    {
        double totalWeight = A + B + C;

        // 상승 가중치가 0이거나 전체 가중치가 0이면 도달 불가능
        if (A <= 0 || totalWeight <= 0)
        {
            return 0;
        }

        // [기대값 공식 유도]
        // E1: 현재 등급(L)에서 상위(L+1)로 가기 위한 기대 비용
        // E0: 하락한 등급(L-1)에서 상위(L+1)로 가기 위한 기대 비용
        // X : 하락한 등급(L-1)에서 다시 현재 등급(L)으로 복구하기 위한 기대 비용
        // pA = A/D, pB = B/D, pC = C/D (D = totalWeight)

        // 1) E1 = cost + pB*E1 + pC*E0  (상승 시 종료이므로 pA*0 생략)
        // 2) E0 = X + E1                (먼저 복구한 뒤 다시 E1 만큼의 비용 필요)
        // 3) X = cost + (pB+pC)*X       (L-1에서는 하락이 유지이므로 pB+pC 확률로 잔류)
        //    X * (1 - (pB+pC)) = cost
        //    X * (pA) = cost  => X = cost / pA = (D * cost) / A

        // 1번에 2번 대입:
        // E1 = cost + pB*E1 + pC*(X + E1)
        // E1 * (1 - pB - pC) = cost + pC*X
        // E1 * pA = cost + pC * (cost / pA)
        // E1 = (cost / pA) + (pC * cost) / (pA^2)
        // E1 = (pA * cost + pC * cost) / (pA^2)
        // E1 = (cost * (pA + pC)) / (pA^2)
        // 가중치(A, C)와 전체(D)로 치환:
        // E1 = (cost * (A/D + C/D)) / (A/D)^2 = (cost * (A+C)/D) / (A^2 / D^2)
        // E1 = (D * cost * (A + C)) / (A^2)

        double expected = (totalWeight * cost * (A + C)) / (A * A);
        return expected;
    }

    /// <summary>
    /// 하락 결과 시에도 등급 하락 없이 현재 등급이 유지된다고 가정했을 때의 기대값 계산
    /// 공식: E = (TotalWeight / A) * cost
    /// </summary>
    private static double ComputeExpectedGold_NoDown(double A, double B, double C, double cost)
    {
        double totalWeight = A + B + C;

        if (A <= 0 || totalWeight <= 0)
        {
            return 0;
        }

        // 단순 기하분포의 기대값: 1 / (상승확률)
        // (D/A) * cost
        return (totalWeight / A) * cost;
    }

    [MenuItem("Tools/Util Window/꿀밤 골드 계산기")]
    public static void OpenWindow()
    {
        GetWindow<ExpectedGoldWindow>("꿀밤 골드 계산기");
    }
}
#endif
