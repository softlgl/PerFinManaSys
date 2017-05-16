using Newtonsoft.Json;
using PerFinManaSys.Web.Auth;
using PerFinManaSys.Web.Filter;
using PerFinManaSys.Web.Models;
using PerFinManaSys.Web.Models.VM;
using PerFinManaSys.Web.Tools;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace PerFinManaSys.Web.Controllers
{
    [RequiresAuthentication]
    public class IncomeController : BaseController
    {
        /// <summary>
        /// 收入管理首页
        /// </summary>
        /// <returns></returns>
        [Compress]
        public ActionResult Index()
        {
            return View();
        }
        [OutputCache(Duration=0)]
        public ActionResult GetIncomeData(int rows = 10, int page = 1, string begindate = null, string enddate = null, string export = null)
        {
            if (string.IsNullOrEmpty(user.U_Name)) 
                return Json(new { flag = "False", Message = "登录用户信息过期！" });
            var q = from t in Db.Income orderby t.I_Date descending where t.I_UID == user.U_ID select t;
            if (!string.IsNullOrEmpty(begindate))
            {
                DateTime begin = DateTime.Parse(begindate);
                q = q.Where(t => t.I_Date >= begin);
            }
            if (!string.IsNullOrEmpty(enddate))
            {
                DateTime end = DateTime.Parse(enddate + " 23:59:59");
                q = q.Where(t => t.I_Date <= end);
            }
             //导出excel
            if (!string.IsNullOrEmpty(export))
            {
                if (q.Any())
                {
                    var list = q.ToList().Select(t => new IncomeVM
                    {
                       I_Amount=t.I_Amount,
                       I_Date=t.I_Date,
                       I_Remark=t.I_Remark
                    }).ToList();
                    var dt = ExportExcel.ToDataTable(list, new[] { "金额(元)", "进账日期", "备注" });
                    Stream s = ExportExcel.RenderToExcel(dt);
                    return File(s, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                        , DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
                }
            }
            var count = q.Count();
            q = q.Skip((page - 1) * rows).Take(rows);
            return MyJson(new
            {
                total = count,
                rows = q.ToList().Select(t => new
                {
                    t.I_ID, t.I_Amount, t.I_Date,
                    I_Remark = !string.IsNullOrEmpty(t.I_Remark) 
                    ? (t.I_Remark.Length <= 10 ? t.I_Remark.Substring(0, t.I_Remark.Length) 
                    : t.I_Remark.Substring(0, 9) + ".....") : t.I_Remark
                })
            }, "yyyy-MM-dd");
        }

        /// <summary>
        /// 添加收入
        /// </summary>
        /// <param name="ic"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddIncome(Income ic)
        {
            if (string.IsNullOrEmpty(user.U_Name)) 
                return Json(new { flag = "False", Message = "登录用户信息过期！" });
            ic.I_ID = Guid.NewGuid();
            ic.I_UID = user.U_ID;
            ic.I_Date = DateTime.Now;
            Db.Entry(ic).State = EntityState.Added;
            Db.SaveChanges();
            return Json(new { flag = "Success", Message = "添加成功！" });
        }

        /// <summary>
        /// 修改收入信息
        /// </summary>
        /// <param name="ic"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditIncome(Income ic)
        {
            try
            {
                if (string.IsNullOrEmpty(user.U_Name)) 
                    return Json(new { flag = "False", Message = "登录用户信息过期！" });
                ic.I_UID = user.U_ID;
                Db.Entry(ic).State = EntityState.Modified;
                Db.SaveChanges();
                return Json(new { flag = "Success", Message = "修改成功！" });
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return Json(new { flag = "False", Message = "修改失败！" });
            }
        }
        /// <summary>
        /// 删除收入
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteIncome(string id) 
        {
            if (string.IsNullOrEmpty(id)) 
                return Json(new { flag = "False", Message = "请选择删除的记录！" });

            id.Split(',').ToList().ForEach(e =>
            {
                var userid = Guid.Parse(e);
                var c = new Income { I_ID=userid };
                Db.Entry(c).State = EntityState.Deleted;
            });
            Db.SaveChanges();
            return Json(new { flag = "Success", Message = "删除成功！" });
        }
        /// <summary>
        /// 获取收入总数
        /// </summary>
        /// <param name="begindate"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        public string GetIncomeSum(string begindate = null, string enddate = null)
        {
            if (string.IsNullOrEmpty(user.U_Name)) 
                return JsonConvert.SerializeObject(new { flag = "False", Message = "登录用户信息过期！" });

            var q = from t in Db.Income where t.I_UID == user.U_ID select t;
            if (!string.IsNullOrEmpty(begindate))
            {
                DateTime begin = DateTime.Parse(begindate);
                q = q.Where(t => t.I_Date >= begin);
            }
            if (!string.IsNullOrEmpty(enddate))
            {
                DateTime end = DateTime.Parse(enddate + " 23:59:59");
                q = q.Where(t => t.I_Date <= end);
            }
            if (!q.Any()) return JsonConvert.SerializeObject(new {flag="Success",sum="0.00" });
            var sum = q.Sum(t=>t.I_Amount);
            return JsonConvert.SerializeObject(new {flag="Success",sum=sum+"元" });
        }
        /// <summary>
        /// 获取收入详细
        /// </summary>
        /// <returns></returns>
        public string GetIncomeDetail(string id)
        {
            if (string.IsNullOrEmpty(id)) 
                return JsonConvert.SerializeObject(new {flag = "False", Message = "请选择查看的记录！" });
            Guid gid=Guid.Parse(id);
            var q = Db.Income.FirstOrDefault(t => t.I_ID == gid);
            return JsonConvert.SerializeObject(q);        }
    }
}
