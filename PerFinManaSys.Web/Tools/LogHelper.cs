using System;
using log4net;

namespace PerFinManaSys.Web.Tools
{
    public class LogHelper
    {
        private static readonly ILog Logerror = LogManager.GetLogger("logerror");
        private static readonly ILog Logdebug = LogManager.GetLogger("logdebug");
        private static readonly ILog Loginfo = LogManager.GetLogger("loginfo");

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public static void LogConfig(string fileName)
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(fileName));
        }

        public static void Error(string errorMessage, Exception ex)
        {
            Logerror.Error(errorMessage, ex);
        }

        public static void Error(string errorMessage)
        {
            Logerror.Error(errorMessage);
        }

        public static void Error(Exception ex)
        {
            Logerror.Error(ex);
        }

        public static void Info(string info)
        {
            Loginfo.Info(info);
        }

        public static void Debug(string debug)
        {
            Logdebug.Debug(debug);
        }
    }
}
