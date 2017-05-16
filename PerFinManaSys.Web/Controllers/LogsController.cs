using PerFinManaSys.Web.Enum;
using PerFinManaSys.Web.Models;
using PerFinManaSys.Web.Models.VM;
using PerFinManaSys.Web.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace PerFinManaSys.Web.Controllers
{
    public class LogsController : BaseController
    {
        /// <summary>
        /// 登录日志列表
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = UserAuth.Admin)]
        public ViewResult Logins()
        {
            return View();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="rows">请求条数</param>
        /// <param name="page">请求页面</param>
        /// <param name="begindate">开始时间</param>
        /// <param name="enddate">结束时间</param>
        /// <param name="export">是否导出</param>
        /// <returns></returns>
        [OutputCache(Duration = 0)]
        [Authorize(Roles = UserAuth.Admin)]
        public ActionResult GetLoginsPaging(int rows = 10, int page = 1, string begindate = null, string enddate = null, string export = null)
        {
            if (string.IsNullOrEmpty(user.U_Name))
                return Json(new { flag = "False", Message = "登录用户信息过期！" });
            var query = Db.Logins.Join(Db.Users, l => l.L_UID, u => u.U_ID, (l, u) => new
            {
                l.L_ID,
                u.U_Name,
                u.U_Tel,
                l.L_HostName,
                l.L_IP,
                l.L_City,
                Role = l.L_Role == 0 ? "管理员" : "用户",
                l.L_LoginTime
            });
            if (!string.IsNullOrEmpty(begindate))
            {
                DateTime dt = Convert.ToDateTime(begindate);
                query = query.Where(t => t.L_LoginTime >= dt);
            }
            if (!string.IsNullOrEmpty(enddate))
            {
                DateTime dt = Convert.ToDateTime(enddate + " 23:59:59");
                query = query.Where(t => t.L_LoginTime <= dt);
            }

            //导出excel
            if (!string.IsNullOrEmpty(export))
            {
                if (!query.Any()) return new EmptyResult();

                List<LoginsVM> list = query.ToList().Select(t => new LoginsVM
                {
                    UserName = t.U_Name,
                    UserEmail = t.U_Tel,
                    RoleName = t.Role,
                    HostName = t.L_HostName,
                    IP = t.L_IP,
                    City = t.L_City,
                    LoginTime = t.L_LoginTime

                }).ToList();
                DataTable dt = ExportExcel.ToDataTable(list
                    , new[] { "用户名", "邮箱", "角色", "主机名称", "IP地址", "登录城市", "登录时间" });
                Stream s = ExportExcel.RenderToExcel(dt);
                return File(s, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
            }

            int count = query.Count();
            query = query.OrderByDescending(t => t.L_LoginTime).Skip((page - 1) * rows).Take(rows);
            return MyJson(new { total = count, rows = query }, "yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OutputCache(Duration = 0)]
        [Authorize(Roles = UserAuth.Admin)]
        public ActionResult DeleteLogins(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { flag = "False", Message = "请选择删除的记录！" });

            id.Split(',').ToList().ForEach(e =>
            {
                var logid = Guid.Parse(e);
                Logins login = new Logins { L_ID = logid };
                Db.Entry(login).State = EntityState.Deleted;
            });
            Db.SaveChanges();

            return Json(new { flag = "Success", Message = "删除成功！" });
        }
    }
}
