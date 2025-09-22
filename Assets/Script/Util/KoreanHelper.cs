

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