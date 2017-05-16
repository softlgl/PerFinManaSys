using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace PerFinManaSys.Web.MyActionResult
{
    /// <summary>
    /// 自定义Json视图
    /// </summary>
    public class CustomJsonResult : JsonResult
    {
        /// <summary>
        /// 格式化字符串
        /// </summary>
        public string FormateStr
        {
            get;
            set;
        }

        /// <summary>
        /// 重写执行视图
        /// </summary>
        /// <param name="context">上下文</param>
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpResponseBase response = context.HttpContext.Response;

            response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

            response.ContentEncoding = ContentEncoding ?? Encoding.UTF8;
            if (Data == null) return;
            JavaScriptSerializer jss = new JavaScriptSerializer();
            string jsonString = jss.Serialize(Data);
            string p = @"\\/Date\((\d+)\)\\/";
            MatchEvaluator matchEvaluator = ConvertJsonDateToDateString;
            Regex reg = new Regex(p);
            jsonString = reg.Replace(jsonString, matchEvaluator);
            response.Write(jsonString);
        }

        /// <summary>  
        /// 将Json序列化的时间由/Date(1294499956278)转为字符串 .
        /// </summary>  
        /// <param name="m">正则匹配</param>
        /// <returns>格式化后的字符串</returns>
        private string ConvertJsonDateToDateString(Match m)
        {
            DateTime dt = new DateTime(1970, 1, 1);
            dt = dt.AddMilliseconds(long.Parse(m.Groups[1].Value));
            dt = dt.ToLocalTime();
            var result = dt.ToString(FormateStr);
            return result;
        }
    }
}