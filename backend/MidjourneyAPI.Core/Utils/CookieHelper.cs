namespace MidjourneyAPI.Core.Utils;

public static class CookieHelper
{
    /// <summary>
    /// 解析 Cookie 字符串为键值对。
    /// </summary>
    /// <param name="cookies">包含多个 Cookie 的字符串，每个 Cookie 以分号分隔。</param>
    /// <returns>包含 Cookie 名称和值的字典。</returns>
    public static Dictionary<string, string> ParseCookies(string cookies)
    {
        return cookies.Split(';')
            .Select(c => c.Trim())
            .Where(c => !string.IsNullOrEmpty(c))
            .Select(c =>
            {
                var index = c.IndexOf('=');
                if (index > 0)
                {
                    var key = c.Substring(0, index).Trim();
                    var value = c.Substring(index + 1).Trim();
                    return new { key, value };
                }

                return null;
            })
            .Where(c => c != null && !string.IsNullOrEmpty(c.key) && !string.IsNullOrEmpty(c.value))
            .ToDictionary(c => c!.key, c => c!.value);
    }

    /// <summary>
    /// 将包含 Cookie 名称和值的字典转换为请求头中使用的 Cookie 字符串。
    /// </summary>
    /// <param name="cookies">包含 Cookie 名称和值的字典。</param>
    /// <returns>格式化后的 Cookie 字符串，每个 Cookie 以分号分隔。</returns>
    public static string FormatCookies(Dictionary<string, string> cookies)
    {
        ArgumentNullException.ThrowIfNull(cookies);

        return string.Join("; ", cookies.Select(kvp => $"{kvp.Key}={kvp.Value}"));
    }
}