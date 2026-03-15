

public static class KoreanHelper
{
    public static string En_Nun(this string word)
    {
        return word + (HasJongseong(word) ? "은" : "는");
    }

    public static string E_Ga(this string word)
    {
        return word + (HasJongseong(word) ? "이" : "가");
    }

    public static string Eul_Reul(this string word)
    {
        return word + (HasJongseong(word) ? "을" : "를");
    }

    public static string Gwa_Wa(this string word)
    {
        return word + (HasJongseong(word) ? "과" : "와");
    }

    // 숫자를 한글 단위(억, 만)로 변환
    public static string ToKoreanUnit(this int value)
    {
        if (value >= 100000000) // 1억 이상: 1억 0000만
        {
            int uk = value / 100000000;
            int man = (value % 100000000) / 10000;
            return $"{uk}억 {man:D4}만";
        }
        else if (value >= 10000) // 1만 이상 ~ 1억 미만: 0000만
        {
            return $"{value / 10000}만";
        }
        else // 1만 미만: 0000
        {
            return value.ToString();
        }
    }

    private static bool HasJongseong(string word)
    {
        if (string.IsNullOrEmpty(word)) return false;

        char last = word[word.Length - 1];

        // 한글인 경우
        if (last >= 0xAC00 && last <= 0xD7A3)
            return (last - 0xAC00) % 28 != 0;

        // 영어 자음
        if ("bcdfghjklmnpqrstvwxzBCDFGHJKLMNPQRSTVWXZ".Contains(last))
            return true;

        // 숫자 (발음 기준)
        return "13678".Contains(last);
    }
}