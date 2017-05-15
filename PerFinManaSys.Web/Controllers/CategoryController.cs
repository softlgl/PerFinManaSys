using Newtonsoft.Json;
using PerFinManaSys.Web.Auth;
using PerFinManaSys.Web.Filter;
using PerFinManaSys.Web.Models;
using PerFinManaSys.Web.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PerFinManaSys.Web.Controllers
{
    [RequiresAuthentication]
    public class CategoryController : BaseController
    {
        /// <summary>
        /// 消费模块页面
        /// </summary>
        /// <returns></returns>
        [CompressAttribute]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [OutputCache(Duration=0)]
        public string GetCategoryPaging(int rows = 10, int page = 1)
        {
            Users user = FormsAuth.GetUserData();
            if (string.IsNullOrEmpty(user.U_Name)) return JsonConvert.SerializeObject(new { flag = "False", Message = "登录用户信息过期！" });
            IQueryable<Category> query = db.Category.Where(t=>t.C_UsersID==user.U_ID).OrderBy(t=>t.C_Name);
            int count = query.Count();
            query = query.Skip((page - 1) * rows).Take(rows);
            Hashtable hashdata = new Hashtable();
            hashdata.Add("total", count);
            hashdata.Add("rows", query);
            return JsonConvert.SerializeObject(hashdata);
        }

        /// <summary>
        /// 模块添加
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        [OutputCache(Duration=0),HttpPost]
        public ActionResult AddCategory(Category c)
        {
            try
            {
                Users user = FormsAuth.GetUserData();
                if (string.IsNullOrEmpty(user.U_Name)) return Json(new { flag = "False", Message = "登录用户信息过期！" });
                Category result = db.Category.Where(t => t.C_Name ==c.C_Name&&t.C_UsersID==user.U_ID).FirstOrDefault();
                if (result != null) return Json(new { flag = "False", Message = "该名称已存在！" });
                
                if (!string.IsNullOrEmpty(c.C_Name)) 
                { 
                    c.C_ID = Guid.NewGuid();
                    c.C_UsersID = user.U_ID;
                    db.Category.Add(c);
                    db.SaveChanges();
                    return Json(new { flag = "Success", Message = "添加成功！" });
                }
                return Json(new { flag = "False", Message = "请将信息填写完完整！" });
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return Json(new { flag = "False", Message = "添加失败！" });
            }
        }
        /// <summary>
        /// 修改模块名称
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        [OutputCache(Duration = 0)]
        [HttpPost]
        public ActionResult EditCategory(Category c)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry<Category>(c).State = EntityState.Modified;
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
        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OutputCache(Duration = 0)]
        public ActionResult DeleteCategory(string id)
        {
            if (string.IsNullOrEmpty(id)) return Json(new { flag = "False", Message = "请选择删除的记录！" });
            if(DeleteCate(id))
            { 
               return Json(new { flag = "Success", Message = "删除成功！" });
            }
            return Json(new { flag = "False", Message = "删除失败！" });
        }
        /// <summary>
        /// 删除Category执行
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteCate(string id)
        {
            try
            {
                id.Split(new char[] { ',' },StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(e =>
                {
                    //Guid userid = Guid.Parse(e);
                    //Category c = db.Category.FirstOrDefault(t => t.C_ID == userid);
                    //db.Category.Remove(c);

                    Guid gid = Guid.Parse(e);
                    Category c = new Category() { C_ID = gid };
                    db.Set<Category>().Attach(c);
                    db.Entry<Category>(c).State = EntityState.Deleted;

                    using (MoneyWatcherDBEntities mdb = new MoneyWatcherDBEntities())
                    {
                        IQueryable<Expenses> exp = db.Expenses.Where(t => t.E_Type == gid);
                        foreach (Expenses item in exp)
                        {
                            Expenses ex = new Expenses() { E_ID = item.E_ID };
                            mdb.Set<Expenses>().Attach(ex);
                            mdb.Entry<Expenses>(ex).State = EntityState.Deleted;
                        }
                        mdb.SaveChanges();
                    }
                });
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return false;
            }
        }
        /// <summary>
        /// 判断是否有相同记录
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [OutputCache(Duration = 0)]
        public ActionResult IsExit(Category c)
        {
            Users user = FormsAuth.GetUserData();
            //判断用户是否被注册
            if (string.IsNullOrEmpty(user.U_Name)) return Json(new { flag = "False", Message = "登录用户信息过期！" });
            Category result = db.Category.Where(t => t.C_Name == c.C_Name && t.C_UsersID == user.U_ID).FirstOrDefault();
            if (result != null) return Json(new { flag = "False", Message = "该名称已存在！" });
            return Json(new { flag = "Success", Message = "记录不存在！" });
        }

        /// <summary>
        /// 获取下拉列表数据
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration = 0)]
        public ActionResult GetCategroyDrop()
        {
            try
            {
                Users user = FormsAuth.GetUserData();
                //判断用户是否被注册
                if (string.IsNullOrEmpty(user.U_Name)) return Json(new { flag = "False", Message = "登录用户信息过期！" });
                var data = from t in db.Category
                           where t.C_UsersID == user.U_ID
                           select new { C_ID = t.C_ID, C_Name = t.C_Name };
                return Json(data,JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return Json(new { flag = "False", Message = "获取信息失败！" });
            }
           
        }
    }
}
