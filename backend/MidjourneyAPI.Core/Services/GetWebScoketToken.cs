using System.Net;
using System.Text.RegularExpressions;
using Furion.DynamicApiController;
using MidjourneyAPI.Core.HttpApis;

namespace MidjourneyAPI.Core.Services;

public class GetWebScoketToken : IDynamicApiController
{
    private readonly IMidjourneyReversedAPI _mjReversedAPI;

    public GetWebScoketToken(IMidjourneyReversedAPI mjReversedAPI)
    {
        _mjReversedAPI = mjReversedAPI;
    }

    public async Task<object> GetData()
    {
        string cookies = """
                         AMP_MKTG_437c42b22c=JTdCJTdE; _ga=GA1.1.1721613393.1726284756; _gcl_au=1.1.984182170.1726284756; __stripe_mid=678a83ea-f04e-4935-8edd-5ef646850065b6c81b; __Host-Midjourney.AuthUserToken=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ0eXBlIjoiZmlyZWJhc2UiLCJpZFRva2VuIjoiZXlKaGJHY2lPaUpTVXpJMU5pSXNJbXRwWkNJNklqQXlNVEF3TnpFMlptUmtPVEEwWlRWaU5HUTBPVEV4Tm1abU5XUmlaR1pqT1RnNU9UazBNREVpTENKMGVYQWlPaUpLVjFRaWZRLmV5SnVZVzFsSWpvaWJHbHNaWGw2YUdGdklpd2liV2xrYW05MWNtNWxlVjlwWkNJNkltSTJPVGcyWldFNUxXUmxaRGN0TkRGa05pMDVNRE5tTFRKalltUmpNems0TXpVeVpDSXNJbWx6Y3lJNkltaDBkSEJ6T2k4dmMyVmpkWEpsZEc5clpXNHVaMjl2WjJ4bExtTnZiUzloZFhSb2FtOTFjbTVsZVNJc0ltRjFaQ0k2SW1GMWRHaHFiM1Z5Ym1WNUlpd2lZWFYwYUY5MGFXMWxJam94TnpJMk1qZzFNVEl6TENKMWMyVnlYMmxrSWpvaVlURm9XR2xPYzFkS2FtTXdSbTF1ZUhOWk5XMXpiWHBQTXpkdU1TSXNJbk4xWWlJNkltRXhhRmhwVG5OWFNtcGpNRVp0Ym5oeldUVnRjMjE2VHpNM2JqRWlMQ0pwWVhRaU9qRTNNalkyTWpZek1EZ3NJbVY0Y0NJNk1UY3lOall5T1Rrd09Dd2laVzFoYVd3aU9pSnNhV3hsZVhwb1lXOUFaMjFoYVd3dVkyOXRJaXdpWlcxaGFXeGZkbVZ5YVdacFpXUWlPblJ5ZFdVc0ltWnBjbVZpWVhObElqcDdJbWxrWlc1MGFYUnBaWE1pT25zaVoyOXZaMnhsTG1OdmJTSTZXeUl4TURrME56ZzRNamt5TkRZNU9ESTFPVEEyTkRJaVhTd2laR2x6WTI5eVpDNWpiMjBpT2xzaU9ESTJOekkwTlRFNE5ETXlPVGszTXprM0lsMHNJbVZ0WVdsc0lqcGJJbXhwYkdWNWVtaGhiMEJuYldGcGJDNWpiMjBpWFgwc0luTnBaMjVmYVc1ZmNISnZkbWxrWlhJaU9pSm5iMjluYkdVdVkyOXRJbjE5LmJLbFVNeGNmbDY0VGVPa3F3Y2UxOWFzbVVJWHZSQjMwcldlMVVJT19OUERNeGNGQ0RNQ0owWjJjSXhUNUpjM0NSQzdmR0hoU3hqejJIZ2oyMWNaNDhObGNYQ09tUmp6Y2xqcmlWX0xyRWFhZWRJSXdjbGhMeUtxYjB1MFVSX2sxbDR0dER6YXpJY21YTUZEY3AxczBydmhGME1Jckd1cU1IUXgwd1ZGRUd3V2hEMk45ZjQxV2ZGUnFFUkRtR29fTk9kaDlNbGNCQ2FOWTNWNTlEMW1vOTZ0WFFUR0NFVjJtaEdLQ3RGbDBQblBUUzlxVUFCZmI2RjdidGNfbmVGcXlheUR6Y29aMnZmcmR3aFQ5WjhjdUhIZG1FaEZsQ2pKcF9ldlVYN1lHNjFSTUlMS1NrWVEwOEYxUDhtRm5VeGxXaFNWNFhoWEh2dXFBTkc5MzBGTVhmQSIsInJlZnJlc2hUb2tlbiI6IkFNZi12QndpeHZBbENfVjl4dGVHWmtjWHhWWHNTS2N5Qy1qT0ZEVzFKM0p3bVFpNks3dExOcmtmYTR1ZnZUMXgxR0FmdkVZTHlKZm92SmYwQWFYeE1uWWlCYXVZeFk1YnRiRTFOUE5rN0xfLUlQaWIzaU1hbmRRcWFVYmZONVdEc2FrT0JTT2ZxcURPaThmWnpOWENTQ0dNNV90bUZzYjB3eFNncjlxbXdFU0NhdGVaWjNQQ3A1dTRYSDZkMmlLSVhVYUJOVGpHUVpiV1l1YXdEMEFGbVYtdnFSVkFxZ3lvc3BXZktSa2V3SE95YVVjcHgycVFJenl3OWlRenJ1VExETUJTQkZZTk1YdDhqMGpBRnoyVDNscUxUNEJnM3d1SEROLUJ6cldUcnBoZUREUThubm1BakdFOVVTTFgzRTBLRmwzNXBPbzluRTRQTXVpNnNPMUUxSzJLcnhjMHFOWTFQWnd3STJ0Qkl1ZkpUa0lJRlZkaXlZRjdTTFJzSjhVT1ZiX2FGanYxR3FuVGEzcGJvd3l5VHM5aW1rMTNvb0xwLVhXOHVPWFFPLU5tTWU3NmNUUUxRaFUiLCJpYXQiOjE3MjY2MjYzMDh9.L_biBLvZgmXoLHQE6RYxTJ9tAKVeEDadcZwrrKn506U; __cf_bm=QogdmhGEk997prJkf_To7vSJvGG.WCQ.F.Tn4UG1njc-1726626308-1.0.1.1-OvD0BUtThQz9PmvaGpTR9WDjSZbRxeTdzCcRacQcRGstBGCvhMsYXPW3W.eWxZJ0lXdqsitY9DoIT2VVwMN.UA; AMP_437c42b22c=JTdCJTIyZGV2aWNlSWQlMjIlM0ElMjIyZTdhODcwYi0zOTk5LTQ2ZDQtODAwOC0zZTdlOWFmNTMwOTklMjIlMkMlMjJ1c2VySWQlMjIlM0ElMjJiNjk4NmVhOS1kZWQ3LTQxZDYtOTAzZi0yY2JkYzM5ODM1MmQlMjIlMkMlMjJzZXNzaW9uSWQlMjIlM0ExNzI2NjI2MzEwNzUxJTJDJTIyb3B0T3V0JTIyJTNBZmFsc2UlMkMlMjJsYXN0RXZlbnRUaW1lJTIyJTNBMTcyNjYyNjMxMDgyMCUyQyUyMmxhc3RFdmVudElkJTIyJTNBMTgyJTdE; cf_clearance=RMfVN.Mk.SlAOqhyEVyHPXvUEvbQVOwNToHAMUoJQlE-1726626311-1.2.1.1-.D.weHgvw5OmUedutnjokr1WN56srMr5oeJnVgHVihfaHQSGhIKzd4sjt2qFpDCgTCIphDZwR_MYV1traivTAGOigwTHnCIHHtzY4sGBCKn3ej3d5FZ6nxYsv12HEf9ZkAY457gUk9.sq6r5U6EWgSwAWRkrZJfJgREKBzatT54ndK8QM5TbiKB4HxEpWb_Hjem8Lshn3IZKCfDXyTY244IhOdjdz34J_bw2t_3MwjcWhtU60D3RGVmVnz..nlMIW8FIBJ_QQUsTA5raVr4JoEtS.jAGUebJXumEiYkhnVsAZ595QPL7hQv9E5VIELKSDeQU01hciJi6ATBxN00HPPpgaYy.jlSUDC9_fmAZnrWq25nv_79Izu2zKf4bJwCrb7I2o0fdUtiiAq53kbtAemyedxbdMXSc_jV2VLwtjz8; _ga_Q0DQ5L7K0D=GS1.1.1726626311.5.0.1726626314.0.0.0; __stripe_sid=01241a73-99ed-4e96-b6da-d0c07f5fdb789346c7; _dd_s=logs=1&id=6752b3fb-6e73-4c79-ae30-50709beb7123&created=1726626310634&expire=1726627557415
                         """;

        var cookieDict = cookies.Split(";").Select(x => x.Split("=")).ToDictionary(x => x[0].Trim(), x => x[1].Trim());

        var imagineResponse = await _mjReversedAPI.GetWebSocketTokenAsync((client, request) =>
        {
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(request.RequestUri,
                new Cookie("__Host-Midjourney.AuthUserToken",
                    cookieDict["__Host-Midjourney.AuthUserToken"]));
            cookieContainer.Add(request.RequestUri,
                new Cookie("__cf_bm",
                    cookieDict["__cf_bm"]));
            request.Headers.Add("Cookie", cookieContainer.GetCookieHeader(request.RequestUri));
        });

        var imagineBody = await imagineResponse.Content.ReadAsStringAsync();

        // 一行获取 websocketToken 的值，如果没有匹配到则返回 null
        string websocketToken = Regex.Match(imagineBody, @"""websocketToken"":""([^""]+)""").Success
            ? Regex.Match(imagineBody, @"""websocketToken"":""([^""]+)""").Groups[1].Value
            : null;

        return websocketToken;
    }
}