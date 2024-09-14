using Furion.RemoteRequest;

namespace MidjourneyAPI.Core.HttpApis;

/// <summary>
/// Midjourney 逆向 API
/// </summary>
[Client("Midjourney.Reversed")]
public interface IMidjourneyReversedAPI : IHttpDispatchProxy
{
    /// <summary>
    /// 获取 WebSocket Token
    /// </summary>
    /// <returns></returns>
    [Get("https://www.midjourney.com/imagine", HttpVersion = "1.1")]
    Task<HttpResponseMessage> GetWebSocketTokenAsync(
        [Interceptor(InterceptorTypes.Request)] Action<HttpClient, HttpRequestMessage>? action = default);

    /// <summary>
    /// 获取用户生成配置
    /// </summary>
    /// <returns></returns>
    [Get("https://www.midjourney.com/api/app/users/gen_settings", HttpVersion = "1.1")]
    Task<HttpResponseMessage> GetUserGenSettings();

    /// <summary>
    /// 提交任务
    /// </summary>
    /// <returns></returns>
    [Post("https://www.midjourney.com/api/app/submit-jobs", HttpVersion = "1.1")]
    Task<HttpResponseMessage> SubmitJobs();

    /// <summary>
    /// 获取任务状态
    /// </summary>
    /// <returns></returns>
    [Post("https://www.midjourney.com/api/app/job-status", HttpVersion = "1.1")]
    Task<HttpResponseMessage> GetJobStatus();
}