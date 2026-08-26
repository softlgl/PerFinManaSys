using Microsoft.Extensions.Logging;

namespace PerFinManaSys.Web.Net8.Tools;

/// <summary>
/// 日志辅助（封装 Microsoft.Extensions.Logging + log4net）。
/// </summary>
public class LogHelper
{
    private readonly ILogger<LogHelper> _logger;

    public LogHelper(ILogger<LogHelper> logger)
    {
        _logger = logger;
    }

    public void Error(Exception ex, string msg = "")
    {
        _logger.LogError(ex, "{Msg}", msg);
    }

    public void Info(string msg)
    {
        _logger.LogInformation("{Msg}", msg);
    }

    public void Warn(string msg)
    {
        _logger.LogWarning("{Msg}", msg);
    }
}
