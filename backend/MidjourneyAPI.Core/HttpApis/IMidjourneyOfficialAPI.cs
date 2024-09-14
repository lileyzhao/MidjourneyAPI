using Furion.RemoteRequest;

namespace MidjourneyAPI.Core.HttpApis;

/// <summary>
/// Midjourney 官方 API
/// </summary>
public interface IMidjourneyOfficialAPI: IHttpDispatchProxy
{
    /// <summary>
    /// 提交任务
    /// </summary>
    /// <returns></returns>
    [Post("https://www.midjourney.com/api/app/submit-jobs")]
    Task<object> SubmitJobs();
    
    /// <summary>
    /// 获取任务状态
    /// </summary>
    /// <returns></returns>
    [Post("https://www.midjourney.com/api/app/job-status")]
    Task<object> GetJobStatus();
}