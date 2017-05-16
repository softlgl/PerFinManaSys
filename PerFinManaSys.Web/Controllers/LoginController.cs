using PerFinManaSys.Web.Auth;
using PerFinManaSys.Web.Filter;
using PerFinManaSys.Web.Tools;
using System.Linq;
using System.Web.Mvc;
using System;
using System.Data;
using System.Data.Entity.Migrations;
using PerFinManaSys.Web.Models;

namespace PerFinManaSys.Web.Controllers
{
    public class LoginController : BaseController
    {
        [Compress]
        public ActionResult Index()
        {
            return Mms();
        }
        /// <summary>
        /// 判断cookie是否过期，选择页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Mms()
        {
            ViewBag.CnName = "个人财务管理系统";
            ViewBag.EnName = "Personal Financial Manage System";
            return View("Index");
        }

        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <param name="txtCheckCode"></param>
        /// <param name="City"></param>
        /// <returns></returns>
        public ActionResult DoAction(string userName, string passWord, string txtCheckCode,string City)
        {
            string checkcode;
            if (txtCheckCode != null)
            {
                if (Session["ValidateCode"] == null)
                {
                    return Json(new { flag = "False", Message = "验证码超时，请重新获取！" });
                }
                checkcode = Session["ValidateCode"].ToString();
            }
            else
            {
                return Json(new { flag = "False", Message = "验证码不能为空！" });
            }
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passWord))
            {
                return Json(new { flag = "False", Message = "请填写完整登录信息！" });
            }
            if (checkcode.ToLower() != txtCheckCode.ToLower())
            {
                return Json(new { flag = "False", Message = "验证码输入不正确！" });
            }
            passWord = Des.DesEncrypt(passWord);
            var users = Db.Users.FirstOrDefault(t => t.U_Name == userName && t.U_PassWord == passWord);
            if (users == null) return Json(new {flag = "False", Message = "登录失败！"});
            users.U_LastLoginTime = DateTime.Now;
            Db.Entry(users).State = EntityState.Modified;
            var lanIp = HttpHelper.ClientIp;
            var ip = string.Empty;
            Logins login=new Logins
            {
                L_ID = Guid.NewGuid(),
                L_City =City,
                L_HostName = HttpHelper.IsLanIp(lanIp) ? HttpHelper.ClientHostName : string.Empty,
                L_IP = string.Format("{0}/{1}", ip, lanIp).Trim('/').Replace("::1", "localhost"),
                L_UID=users.U_ID,
                L_Role = users.U_Role,
                L_LoginTime = users.U_LastLoginTime.Value
            };
            Db.Logins.AddOrUpdate(login);
            Db.SaveChanges();
            //写入cookie
            FormsAuth.SignIn(users);
            return Json(new { flag = "Success", Message = "登录成功！" });
        }

        /// <summary>
        /// 验证码
        /// </summary>
        /// <returns></returns>
        [OutputCache(NoStore = true, Duration = 0)]
        public FileContentResult CheckCode()
        {
            ValidateCode vCode = new ValidateCode();
            string code = vCode.CreateValidateCode(4);
            Session["ValidateCode"] = code;
            byte[] bytes = vCode.CreateValidateGraphic(code);
            return File(bytes, @"image/jpeg");
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public ActionResult Sigout()
        {
            Session.Clear();
            FormsAuth.SingOut();
            return RedirectToAction("../Login.html");
        }
    }
}
