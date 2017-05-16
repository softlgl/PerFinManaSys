using System.Collections.Generic;
using System.Net;
using System.Web;

namespace PerFinManaSys.Web.Tools
{
    public class HttpHelper
    {
        /// <summary>
        /// 得到用户IP地址
        /// </summary>
        /// <returns>返回用户IP地址,如果获取不到返回 0.0.0.0 </returns>
        /// 
        public static string ClientIp
        {
            get
            {
                var context = HttpContext.Current;
                string result = context.Request.UserHostAddress;
                if (string.IsNullOrEmpty(result))
                {
                    result = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];//获取包括使用了代理服务器的地址列表。
                }
                if (string.IsNullOrEmpty(result))
                {
                    result = context.Request.ServerVariables["REMOTE_ADDR"];//最后一个代理服务器地址。
                }
                if (string.IsNullOrEmpty(result))
                {
                    result = context.Request.UserHostAddress;
                }
                return result;
            }
        }
        public static bool IsLanIp(string ip)
        {
            var result = false;
            var ips = new List<string> { "10.", "192.", "172.", "127.", "::1", "localhost" };
            ips.ForEach(x =>
            {
                if (ip.StartsWith(x)) result = true;
            });
            return result;
        }
        public static string ClientHostName
        {
            get
            {
                var context = HttpContext.Current;
                string ip = context.Request.UserHostAddress;
                string name = context.Request.UserHostName;

                if (name == ip || name == "127.0.0.1" || name == "::1")
                {
                    name = context.Request.ServerVariables["REMOTE_HOST"];
                }
                if (name != ip && name != "127.0.0.1" && name != "::1") return name;
                try {
                    if (ip != null) name = Dns.GetHostEntry(ip).HostName;
                }
                catch
                {
                    // ignored
                }

                return name;
            }
        }
    }
}