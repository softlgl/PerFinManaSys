using PerFinManaSys.Web.Auth;
using PerFinManaSys.Web.Filter;
using PerFinManaSys.Web.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PerFinManaSys.Web.Controllers
{
    [RequiresAuthentication]
    public class ReportController : BaseController
    {
        
        /// <summary>
        /// 消费比例报表
        /// </summary>
        /// <returns></returns>
        [CompressAttribute]
        public ActionResult ConsumptionScale()
        {
            return View();
        }
        /// <summary>
        /// 消费比例数据
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [OutputCache(Duration=0)]
        public ActionResult ConsumptionData(string beginDate, string endDate)
        {
            //获取用户信息
            Users user = FormsAuth.GetUserData();
            if (string.IsNullOrEmpty(user.U_Name)) return Json(new { flag = "False", Message = "登录用户信息过期！" });
            var query = db.Expenses.Where(t=>t.E_UID==user.U_ID);
            //开始日期
            if (!string.IsNullOrEmpty(beginDate))
            {
                DateTime begin = Convert.ToDateTime(beginDate);
                query = query.Where(t => t.E_Date >= begin);
            }
            //结束日期
            if (!string.IsNullOrEmpty(endDate))
            {
                DateTime end = Convert.ToDateTime(endDate + " 23:59:59");
                query = query.Where(t => t.E_Date <= end);
            }
            //获取消费分组
            var GroupQuery=query.Join(db.Category, e => e.E_Type, c => c.C_ID, (e, c) => new{Amount=e.E_Amount,Type=c.C_Name})
                .GroupBy(t=>t.Type);
            var names = GroupQuery.Select(t=>t.Key);
            Hashtable hashdata = new Hashtable();
            //添加消费类型
            if(names.Count() > 0)
            {
                hashdata.Add("xdata", names.ToArray());
            }
            else
            {
                hashdata.Add("xdata", new string[] { });
            }
            //添加数据
            hashdata.Add("series", GroupQuery.Select(t => new { value = t.Sum(a => a.Amount), name =t.Key}));
            return Json(hashdata);
        }
    }
}
