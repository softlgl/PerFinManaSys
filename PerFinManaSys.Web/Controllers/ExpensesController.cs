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
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PerFinManaSys.Web.Controllers
{
    [RequiresAuthentication]
    public class ExpensesController : BaseController
    {
       /// <summary>
       /// 账务明细
       /// </summary>
       /// <returns></returns>
       [CompressAttribute]
        public ActionResult Index()
        {
            return View();
        }
        [OutputCache(Duration=0)]
        public ActionResult GetExpenseData(int rows = 10, int page = 1, string Categroy = null, string begindate=null,string enddate=null,string export=null)
        {
            Users user = FormsAuth.GetUserData();
            if (string.IsNullOrEmpty(user.U_Name)) return Json(new { flag = "False", Message = "登录用户信息过期！" });
            var query = from ep in db.Expenses
                        join c in db.Category on ep.E_Type equals c.C_ID
                        orderby ep.E_Date descending where ep.E_UID==user.U_ID
                        select new { E_ID = ep.E_ID, E_UID = ep.E_UID, E_Type = c.C_Name, E_TypeID = ep.E_Type, E_Amount = ep.E_Amount, E_Date = ep.E_Date, E_Remark = ep.E_Remark };
            //条件判断
            if (!string.IsNullOrEmpty(Categroy))
            { 
                Guid id=Guid.Parse(Categroy);
                query = query.Where(t => t.E_TypeID ==id );
            }
            if (!string.IsNullOrEmpty(begindate))
            {
                DateTime dt = Convert.ToDateTime(begindate);
                query = query.Where(t=>t.E_Date>=dt);
            }
            if (!string.IsNullOrEmpty(enddate))
            {
                DateTime dt = Convert.ToDateTime(enddate+" 23:59:59");
                query = query.Where(t => t.E_Date <= dt);
            }
            //导出excel
            if (!string.IsNullOrEmpty(export))
            {
                if (query.Count() > 0)
                {
                    List<ExpenseVM> list = query.ToList().Select(t => new ExpenseVM
                    {
                        E_Type = t.E_Type,
                        E_Amount = t.E_Amount,
                        E_Date = t.E_Date.ToShortDateString(),
                        E_Remark = t.E_Remark
                    }).ToList();
                    DataTable dt = ExportExcel.ToDataTable<ExpenseVM>(list, new string[] { "消费类型", "金额(元)", "消费日期", "备注" });
                    Stream s = ExportExcel.RenderToExcel(dt);
                    return File(s, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
                }
                else
                {
                    return new EmptyResult();
                }
            }
            int count = query.Count();
            query = query.Skip((page - 1) * rows).Take(rows);
            Hashtable hashdata = new Hashtable();
            hashdata.Add("total", count);
            hashdata.Add("rows", query.Select(t => new {
                E_ID = t.E_ID, 
                E_UID = t.E_UID, 
                E_Type = t.E_Type, 
                E_TypeID = t.E_Type,
                E_Amount = t.E_Amount, 
                E_Date = t.E_Date,
                E_Remark = !string.IsNullOrEmpty(t.E_Remark) ? (t.E_Remark.Length <= 10 ? t.E_Remark.Substring(0, t.E_Remark.Length) : t.E_Remark.Substring(0, 9) + ".....") : t.E_Remark
            }));
            return MyJson(hashdata,"yyyy-MM-dd");
        }
        /// <summary>
        /// 添加账单
        /// </summary>
        /// <param name="ep"></param>
        /// <returns></returns>
        [HttpPost]
        public string AddExpenses(Expenses ep)
        {
            try 
	        {
                Users user = FormsAuth.GetUserData();
                //判断用户是否被注册
                if (string.IsNullOrEmpty(user.U_Name)) 
                    return JsonConvert.SerializeObject(new { flag = "False", Message = "登录用户信息过期！" });
                ep.E_ID = Guid.NewGuid();
                ep.E_Date = DateTime.Now;
                ep.E_UID = user.U_ID;
                db.Expenses.Add(ep);
                db.SaveChanges();
                return JsonConvert.SerializeObject(new { flag = "Success", Message = "添加成功！" });
	        }
	        catch (Exception ex)
	        {
                LogHelper.Error(ex);
                return JsonConvert.SerializeObject(new { flag = "False", Message = "添加失败！" });
	        }
          
        }
        /// <summary>
        /// 获取平均消费和消费总额
        /// </summary>
        /// <param name="Categroy"></param>
        /// <param name="begindate"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        [OutputCache(Duration = 0)]
        public ActionResult GetSumAvg(string Categroy = null, string begindate = null, string enddate = null)
        {
            Users user = FormsAuth.GetUserData();
            if (string.IsNullOrEmpty(user.U_Name)) return Json(new { flag = "False", Message = "登录用户信息过期！" });
            //var sum=from t in db.Expenses where t.E_UID==user.U_ID select new{ SUM=t.E_Amount,Date=t.E_Date };
            //var total = sum.Sum(t => t.SUM);
            //var key = from t in sum
            //         group t by t.Date.ToString("yyyy-MM-dd") into g
            //         select g.Key;
            //string avg = string.Empty;
            //if (key.Count() != 0)
            //{
            //    avg = (total / key.Count()).ToString();
            //}
            //else
            //{
            //    avg = "0.00";
            //}
            var q = from t in db.Expenses where t.E_UID==user.U_ID select t;
            if (!string.IsNullOrEmpty(Categroy))
            {
                Guid id = Guid.Parse(Categroy);
                q = q.Where(t => t.E_Type == id);
            }
            if (!string.IsNullOrEmpty(begindate))
            {
                DateTime dt = Convert.ToDateTime(begindate);
                q = q.Where(t => t.E_Date >= dt);
            }
            if (!string.IsNullOrEmpty(enddate))
            {
                DateTime dt = Convert.ToDateTime(begindate + " 23:59:59");
                q = q.Where(t => t.E_Date <= dt);
            }
            var query = from t in q
                        group t by t.E_Date into g
                        select new { Date = g.Key, Total = g.Sum(t => t.E_Amount) };
            if (query.Count() > 0)
            {
                var sumquery = query.Sum(t => t.Total);
                var avgquery = query.Average(t => t.Total);
                return Json(new { Total = sumquery+"元", Avg = avgquery.ToString("f2")+"元" });
            }
            else
            {
                return Json(new { Total = "0.00", Avg ="0.00" });
            }
        }
        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OutputCache(Duration = 0)]
        public ActionResult DeleteExpenses(string id)
        {
            if (string.IsNullOrEmpty(id)) return Json(new { flag = "False", Message = "请选择删除的记录！" });
            id.Split(new char[] { ',' }).ToList().ForEach(e =>
            {
                //Guid userid = Guid.Parse(e);
                //Expenses c = db.Expenses.FirstOrDefault(t => t.E_ID == userid);
                //db.Expenses.Remove(c);

                Guid userid = Guid.Parse(e);
                Expenses c = new Expenses() { E_ID = userid };
                db.Set<Expenses>().Attach(c);
                db.Entry<Expenses>(c).State = EntityState.Deleted;
            });
            db.SaveChanges();
            return Json(new { flag = "Success", Message = "删除成功！" });
        }

        /// <summary>
        /// 获取账单详细信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string ExpenseDetail(string id)
        {
            if (string.IsNullOrEmpty(id)) return JsonConvert.SerializeObject(new {flag = "False", Message = "请选择查看的记录！" });
            Guid gid=Guid.Parse(id);
            var q = db.Expenses.FirstOrDefault(t=>t.E_ID==gid);
            return JsonConvert.SerializeObject(q);
        }
        /// <summary>
        /// 修改账单
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditExpenses(Expenses c)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry<Expenses>(c).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { flag = "Success", Message = "修改成功！" });
                }
                return Json(new { flag = "False", Message = "请将信息填写完完整！" });
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return Json(new { flag = "False", Message = "修改失败！" });
            }
        }
    }
}
