using Newtonsoft.Json;
using PerFinManaSys.Web.Auth;
using PerFinManaSys.Web.Filter;
using PerFinManaSys.Web.Models;
using PerFinManaSys.Web.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PerFinManaSys.Web.Controllers
{
    public class LoginController : BaseController
    {
        [CompressAttribute]
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
        /// <param name="UserName"></param>
        /// <param name="PassWord"></param>
        /// <param name="txtCheckCode"></param>
        /// <returns></returns>
        public ActionResult doAction(string UserName, string PassWord, string txtCheckCode)
        {
            string checkcode = "";
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
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(PassWord))
            {
                return Json(new { flag = "False", Message = "请填写完整登录信息！" });
            }
            if (checkcode.ToLower() != txtCheckCode)
            {
                return Json(new { flag = "False", Message = "验证码输入不正确！" });
            }
            PassWord = DES.DesEncrypt(PassWord, DES.Key);
            Users user = db.Users.Where(t => t.U_Name == UserName && t.U_PassWord == PassWord).FirstOrDefault();
            if (user != null)
            {
                //写入cookie
                FormsAuth.SignIn(user);
                return Json(new { flag = "Success", Message = "登录成功！" });
            }
            else
            {
                return Json(new { flag = "False",Message="登录失败！" });
            }
           
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
            return RedirectToAction("../User/Login.html");
        }
    }
}
