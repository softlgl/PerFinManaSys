using EFTest.Tools;
using Newtonsoft.Json;
using PerFinManaSys.Web.Auth;
using PerFinManaSys.Web.Filter;
using PerFinManaSys.Web.Models;
using PerFinManaSys.Web.Models.VM;
using PerFinManaSys.Web.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
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
        [CompressAttribute]
        public ActionResult Index()
        {
            return View();
        }
        [OutputCache(Duration=0)]
        public ActionResult GetIncomeData(int rows = 10, int page = 1, string begindate = null, string enddate = null, string export = null)
        {
            Users user = FormsAuth.GetUserData();
            if (string.IsNullOrEmpty(user.U_Name)) return Json(new { flag = "False", Message = "登录用户信息过期！" });
            var q = from t in db.Income orderby t.I_Date descending where t.I_UID == user.U_ID select t;
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
                if (q.Count() > 0)
                {
                    List<IncomeVM> list = q.ToList().Select(t => new IncomeVM
                    {
                       I_Amount=t.I_Amount,
                       I_Date=t.I_Date,
                       I_Remark=t.I_Remark
                    }).ToList();
                    DataTable dt = ExportExcel.ToDataTable<IncomeVM>(list, new string[] { "金额(元)", "进账日期", "备注" });
                    Stream s = ExportExcel.RenderToExcel(dt);
                    return File(s, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
                }
            }
            int count = q.Count();
            q = q.Skip((page - 1) * rows).Take(rows);
            Hashtable hashdata = new Hashtable();
            hashdata.Add("total", count);
            hashdata.Add("rows", q.ToList().Select(t => new {
                I_ID = t.I_ID,
                I_Amount = t.I_Amount,
                I_Date = t.I_Date,
                I_Remark = !string.IsNullOrEmpty(t.I_Remark) ? (t.I_Remark.Length <= 10 ? t.I_Remark.Substring(0, t.I_Remark.Length) : t.I_Remark.Substring(0, 9) + ".....") : t.I_Remark
            }));
            return MyJson(hashdata, "yyyy-MM-dd");
        }

        /// <summary>
        /// 添加收入
        /// </summary>
        /// <param name="ic"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddIncome(Income ic)
        {
            try
            {
                Users user = FormsAuth.GetUserData();
                if (string.IsNullOrEmpty(user.U_Name)) return Json(new { flag = "False", Message = "登录用户信息过期！" });
                ic.I_ID = Guid.NewGuid();
                ic.I_UID = user.U_ID;
                ic.I_Date = DateTime.Now;
                db.Income.Add(ic);
                db.SaveChanges();
                return Json(new { flag = "Success", Message = "添加成功！" });
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return Json(new { flag = "False", Message = "添加失败！" });
            }
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
                Users user = FormsAuth.GetUserData();
                if (string.IsNullOrEmpty(user.U_Name)) return Json(new { flag = "False", Message = "登录用户信息过期！" });
                ic.I_UID = user.U_ID;
                db.Entry<Income>(ic).State = EntityState.Modified;
                db.SaveChanges();
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
            try
            {
                if (string.IsNullOrEmpty(id)) return Json(new { flag = "False", Message = "请选择删除的记录！" });
                id.Split(new char[] { ',' }).ToList().ForEach(e =>
                {
                    //Guid userid = Guid.Parse(e);
                    //Income c = db.Income.FirstOrDefault(t => t.I_ID == userid);
                    //db.Income.Remove(c);

                    Guid userid = Guid.Parse(e);
                    Income c = new Income() { I_ID=userid };
                    db.Set<Income>().Attach(c);
                    db.Entry<Income>(c).State = EntityState.Deleted;
                });
                db.SaveChanges();
                return Json(new { flag = "Success", Message = "删除成功！" });
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return Json(new { flag = "False", Message = "删除失败！" });
                throw;
            }
        }
        /// <summary>
        /// 获取收入总数
        /// </summary>
        /// <param name="begindate"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        public string GetIncomeSum(string begindate = null, string enddate = null)
        {
            try
            {
                Users user = FormsAuth.GetUserData();
                if (string.IsNullOrEmpty(user.U_Name)) return JsonConvert.SerializeObject(new { flag = "False", Message = "登录用户信息过期！" });
                var q = from t in db.Income where t.I_UID == user.U_ID select t;
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
                var sum = q.Sum(t=>t.I_Amount);
                return JsonConvert.SerializeObject(new {flag="Success",sum=sum+"元" });
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JsonConvert.SerializeObject(new { flag = "False", Message = "获取统计失败！" });
            }
        }
        /// <summary>
        /// 获取收入详细
        /// </summary>
        /// <returns></returns>
        public string GetIncomeDetail(string id)
        {
            if (string.IsNullOrEmpty(id)) return JsonConvert.SerializeObject(new {flag = "False", Message = "请选择查看的记录！" });
            Guid gid=Guid.Parse(id);
            try
            {
                var q = db.Income.FirstOrDefault(t => t.I_ID == gid);
                return JsonConvert.SerializeObject(q);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JsonConvert.SerializeObject(new {flag="False",Message="获取失败！" });
            }
        }
    }
}
