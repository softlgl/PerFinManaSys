using PerFinManaSys.Web.Enum;
using PerFinManaSys.Web.Models;
using PerFinManaSys.Web.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Data;
using System.Collections;
using Newtonsoft.Json.Converters;
using PerFinManaSys.Web.Auth;
using System.Text;
using PerFinManaSys.Web.Filter;

namespace PerFinManaSys.Web.Controllers
{
    [Authorize(Roles = UserAuth.Admin)]
    public class UserManageController : BaseController
    {
        /// <summary>
        /// 用户列表界面
        /// </summary>
        /// <returns></returns>
        [CompressAttribute]
        public ActionResult UserList()
        {
            return View();
        }
        /// <summary>
        /// 用户列表数据
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration=0)]
        public string UserListData(int rows = 10, int page=1)
        {
            IQueryable<Users> query = db.Users.OrderByDescending(t => t.U_RegistDate);
            int count = query.Count();
            query = query.Skip((page - 1) * rows).Take(rows);
            Hashtable hashdata = new Hashtable();
            hashdata.Add("total", count);
            hashdata.Add("rows", query.ToList().Select(t => new {
                U_ID=t.U_ID,
                U_PassWord = DES.DesDecrypt(t.U_PassWord, DES.Key), 
                U_Name=t.U_Name,
                U_Tel=t.U_Tel,
                U_Role=t.U_Role,
                U_RegistDate = t.U_RegistDate

            }));
            //json时间转换
            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
            timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            return JsonConvert.SerializeObject(hashdata, timeConverter);
        }
        /// <summary>
        /// 获取用户详细信息
        /// </summary>
        /// <returns></returns>
        public string UserDetail()
        {
            return "";
        }
        /// <summary>
        /// 用户注册
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public string AddUser(Users user)
        {
            user.U_Role = 1;
            return InsertUser(user);
        }
        public string ManageAddUser(Users user)
        {
            user.U_RegistDate = DateTime.Now;
            return InsertUser(user);
        }
        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string InsertUser(Users user)
        {
            //验证登录信息是否完整
            if (string.IsNullOrEmpty(user.U_Name) || string.IsNullOrEmpty(user.U_PassWord) || string.IsNullOrEmpty(user.U_Tel))
            {
                return JsonConvert.SerializeObject(new { flag = "False", Message = "请填写完整登录信息！" });
            }
            //验证邮箱格式
            Regex r = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            if (!r.IsMatch(user.U_Tel)) return JsonConvert.SerializeObject(new { flag = "False", Message = "邮箱格式不正确！" });
            try
            {
                //判断用户是否被注册
                Users result = db.Users.Where(t => t.U_Name == user.U_Name).FirstOrDefault();
                if (result != null) return JsonConvert.SerializeObject(new { flag = "Exits", Message = "该用户名已被注册！" });
                user.U_ID = Guid.NewGuid();
                user.U_PassWord = DES.DesEncrypt(user.U_PassWord, DES.Key);
                user.U_RegistDate = DateTime.Now;
                db.Users.Add(user);
                db.SaveChanges();
                return JsonConvert.SerializeObject(new { flag = "Success", Message = "注册成功！" });
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JsonConvert.SerializeObject(new { flag = "False", Message = "系统错误！" });
            }
        }
        /// <summary>
        /// 根据用户名邮箱获取用户信息
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult FindUserInfo(Users user)
        {
            user = db.Users.FirstOrDefault(t => t.U_Name == user.U_Name && t.U_Tel == user.U_Tel);
            if (user != null)
            {
                Session["User"] = user;
                return Json(new { flag = "Success", Message = "验证成功！" });
            }
            else
            {
                return Json(new { flag = "False", Message = "验证信息不正确！" });
            }
        }

        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpUserPass(string PassWord)
        {
            if (Session["User"] == null)
            {
                return Json(new { flag = "False", Message = "验证信息已过期，请重新获取！" });
            }
            else
            {
                Users user = (Users)Session["User"];
                user.U_PassWord = DES.DesEncrypt(PassWord, DES.Key);
                db.Entry<Users>(user).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { flag = "Success", Message = "修改成功！" });
            }
        }
        /// <summary>
        /// 登录后修改密码
        /// </summary>
        /// <param name="U_PassWord"></param>
        /// <param name="U_PassWordConfirm"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdatePass(string U_PassWord,string U_PassWordConfirm)
        {
                Users user= FormsAuth.GetUserData();
                if (!string.IsNullOrEmpty(user.U_Name))
                {
                    U_PassWord = DES.DesEncrypt(U_PassWord, DES.Key);
                    Users newuser = db.Users.FirstOrDefault(t => t.U_Name == user.U_Name && t.U_PassWord == U_PassWord);
                    if (newuser != null)
                    {
                        newuser.U_PassWord = DES.DesEncrypt(U_PassWordConfirm, DES.Key);
                        db.Entry<Users>(newuser).State = EntityState.Modified;
                        db.SaveChanges();
                        return Json(new { flag = "Success", Message = "修改成功！" });
                    }
                    else
                    {
                        return Json(new { flag = "False", Message = "原始密码不正确！" });
                    }
                }
                else
                {
                    return Json(new { flag = "False", Message = "获取用户信息失败！" });
                }
        }
        /// <summary>
        /// 修改用户
        /// </summary>
        /// <returns></returns>
        public string UpdateUser(Users user)
        { //验证登录信息是否完整
            if (string.IsNullOrEmpty(user.U_Name) || string.IsNullOrEmpty(user.U_PassWord) 
                || string.IsNullOrEmpty(user.U_Tel))
            {
                return JsonConvert.SerializeObject(new { flag = "False", Message = "请填写完整登录信息！" });
            }
            //验证邮箱格式
            Regex r = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            if (!r.IsMatch(user.U_Tel)) return JsonConvert.SerializeObject(new { flag = "False", Message = "邮箱格式不正确！" });
            try
            {
                user.U_PassWord = DES.DesEncrypt(user.U_PassWord, DES.Key);
                db.Entry<Users>(user).State = EntityState.Modified;
                db.SaveChanges();
                return JsonConvert.SerializeObject(new { flag = "Success", Message = "修改成功！" });
            } 
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JsonConvert.SerializeObject(new { flag = "False", Message = "系统错误！" });
            }
        }

        public ActionResult IsExit(Users user)
        {
            //判断用户是否被注册
            Users result = db.Users.Where(t => t.U_Name == user.U_Name).FirstOrDefault();
            if (result != null) return Json(new { flag = "False", Message = "该用户名已被注册！" });
            return Json(new { flag = "Success", Message = "用户名不存在！" });
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <returns></returns>
        public string DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return JsonConvert.SerializeObject(new { flag = "False", Message = "用户不能为空！" });
            try
            {
                 id.Split(new char[]{','}).ToList().ForEach(e => {
                    //Guid userid = Guid.Parse(e);
                    //Users user = db.Users.FirstOrDefault(t => t.U_ID == userid);
                    //db.Users.Remove(user);

                    Guid userid = Guid.Parse(e);
                    Users user = new Users() { U_ID = userid };
                    db.Set<Users>().Attach(user);
                    db.Entry<Users>(user).State = EntityState.Deleted;
                    var category = db.Category.Where(t => t.C_UsersID == userid);
                    StringBuilder sb=new StringBuilder();
                    foreach (var item in category)
                    {
                        sb.Append(item.C_ID+",");
                    }
                    CategoryController c = new CategoryController();
                    c.DeleteCate(sb.ToString());
                });
                db.SaveChanges();
                return JsonConvert.SerializeObject(new { flag = "Success", Message = "删除成功！" });
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JsonConvert.SerializeObject(new { flag = "False", Message = "删除失败！" });
            }
            
        }
    }
}
