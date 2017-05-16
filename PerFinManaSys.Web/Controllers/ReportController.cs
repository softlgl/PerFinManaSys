using PerFinManaSys.Web.Auth;
using PerFinManaSys.Web.Filter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            if (string.IsNullOrEmpty(user.U_Name)) 
                return Json(new { flag = "False", Message = "登录用户信息过期！" });

            var query = Db.Expenses.Where(t=>t.E_UID==user.U_ID);
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
            var groupQuery=query.Join(Db.Category, e => e.E_Type, c => c.C_ID, 
                (e, c) => new{Amount=e.E_Amount,Type=c.C_Name}).GroupBy(t=>t.Type);
            var names = groupQuery.Select(t=>t.Key);
            Hashtable hashdata = new Hashtable
            {
                {"xdata", names.Any() ? names.ToArray() : new string[] {}},
                {"series", groupQuery.Select(t => new {value = t.Sum(a => a.Amount), name = t.Key})}
            };
            //添加消费类型
            //添加数据
            return Json(hashdata);
        }

        /// <summary>
        /// 近一年消费
        /// </summary>
        /// <returns></returns>
        public ActionResult HalfYear()
        {
            return View();
        }

        /// <summary>
        /// 近一年消费数据
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration=0)]
        public ActionResult HalfYearData()
        {
            //获取用户信息
            if (string.IsNullOrEmpty(user.U_Name)) 
                return Json(new { flag = "False", Message = "登录用户信息过期！" });

            int mounth = 12;
            DateTime before = DateTime.Parse(DateTime.Now.AddMonths(mounth * -1).ToString("yyyy-MM") + "-01 00:00:00");
            //分组获取
            var q = Db.Expenses.Where(t => t.E_UID == user.U_ID&&t.E_Date>=before)
                .GroupBy(t => new { t.E_Date.Year, t.E_Date.Month }).ToList();
            var query = q.Select(t => new
            {
                Date = t.Key.Year + "年" + (t.Key.Month.ToString().Length >= 2 ? t.Key.Month.ToString() : "0" + t.Key.Month.ToString()) + "月",
                Total = t.Sum(e => e.E_Amount),
                Avg = t.Average(e => e.E_Amount)
            });
            List<string> halfYear = new List<string>();
            for(mounth=12; mounth >= 1; mounth--)
            {
                halfYear.Add(DateTime.Now.AddMonths(mounth*-1).ToString("yyyy年MM月"));
            }
            Hashtable hashdata = new Hashtable {{"xdata", halfYear}};
            List<decimal> mounthExprenses = new List<decimal>();
            halfYear.ForEach(h =>
            {
                var item = query.FirstOrDefault(t => t.Date == h);
                mounthExprenses.Add(item != null ? item.Total : 0);
            });
            hashdata.Add("series", mounthExprenses);
            return Json(hashdata);
        }
    }
}
