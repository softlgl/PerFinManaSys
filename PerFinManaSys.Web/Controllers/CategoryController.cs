using Newtonsoft.Json;
using PerFinManaSys.Web.Auth;
using PerFinManaSys.Web.Filter;
using PerFinManaSys.Web.Models;
using System;
using System.Data;
using System.Linq;
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
            if (string.IsNullOrEmpty(user.U_Name)) 
                return JsonConvert.SerializeObject(new { flag = "False", Message = "登录用户信息过期！" });
            IQueryable<Category> query = Db.Category.Where(t=>t.C_UsersID==user.U_ID).OrderBy(t=>t.C_Name);
            int count = query.Count();
            query = query.Skip((page - 1) * rows).Take(rows);
            return JsonConvert.SerializeObject(new {total=count,rows=query });
        }

        /// <summary>
        /// 模块添加
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        [OutputCache(Duration=0),HttpPost]
        public ActionResult AddCategory(Category c)
        {
            if (string.IsNullOrEmpty(user.U_Name)) 
                return Json(new { flag = "False", Message = "登录用户信息过期！" });
            Category result = Db.Category.FirstOrDefault(t => t.C_Name ==c.C_Name&&t.C_UsersID==user.U_ID);
            if (result != null) return Json(new { flag = "False", Message = "该名称已存在！" });

            if (string.IsNullOrEmpty(c.C_Name))
                return Json(new {flag = "False", Message = "请将信息填写完完整！"});
            c.C_ID = Guid.NewGuid();
            c.C_UsersID = user.U_ID;
            Db.Entry(c).State=EntityState.Added;
            Db.SaveChanges();
            return Json(new { flag = "Success", Message = "添加成功！" });
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
            if (!ModelState.IsValid) return Json(new {flag = "False", Message = "请将信息填写完完整！"});
            Db.Entry(c).State = EntityState.Modified;
            Db.SaveChanges();
            return Json(new { flag = "Success", Message = "修改成功！" });
        }
        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OutputCache(Duration = 0)]
        public ActionResult DeleteCategory(string id)
        {
            return string.IsNullOrEmpty(id) ?
                Json(new { flag = "False", Message = "请选择删除的记录！" }) : 
                Json(DeleteCate(id) 
                ? new { flag = "Success", Message = "删除成功！" } 
                : new { flag = "False", Message = "删除失败！" });
        }

        /// <summary>
        /// 删除Category执行
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteCate(string id)
        {

            id.Split(new[] { ',' },StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(e =>
            {
                var gid = Guid.Parse(e);
                var c = new Category { C_ID = gid };
                Db.Entry(c).State = EntityState.Deleted;
                using (var mdb = new MoneyWatcherDBEntities())
                {
                    var exp = Db.Expenses.Where(t => t.E_Type == gid);
                    foreach (var item in exp)
                    {
                        var ex = new Expenses { E_ID = item.E_ID };
                        mdb.Entry(ex).State = EntityState.Deleted;
                    }
                    mdb.SaveChanges();
                }
             });
             Db.SaveChanges();
             return true;
        }

        /// <summary>
        /// 判断是否有相同记录
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        [OutputCache(Duration = 0)]
        public ActionResult IsExit(Category c)
        {
            //判断用户是否被注册
            if (string.IsNullOrEmpty(user.U_Name)) 
                return Json(new { flag = "False", Message = "登录用户信息过期！" });
            var result = Db.Category.FirstOrDefault(t => t.C_Name == c.C_Name && t.C_UsersID == user.U_ID);
            return Json(result != null ? new { flag = "False", Message = "该名称已存在！" } : new { flag = "Success", Message = "记录不存在！" });
        }

        /// <summary>
        /// 获取下拉列表数据
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration = 0)]
        public ActionResult GetCategroyDrop()
        {
            //判断用户是否被注册
            if (string.IsNullOrEmpty(user.U_Name)) 
                return Json(new { flag = "False", Message = "登录用户信息过期！" });
            var data = from t in Db.Category
                        where t.C_UsersID == user.U_ID
                        select new {t.C_ID, t.C_Name };
            return Json(data,JsonRequestBehavior.AllowGet);
        }
    }
}
