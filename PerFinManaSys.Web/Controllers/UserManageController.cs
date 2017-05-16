using PerFinManaSys.Web.Enum;
using PerFinManaSys.Web.Models;
using PerFinManaSys.Web.Tools;
using System;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Data;
using Newtonsoft.Json.Converters;
using System.Text;
using PerFinManaSys.Web.Filter;
using PerFinManaSys.Web.Auth;

namespace PerFinManaSys.Web.Controllers
{
    [RequiresAuthentication(Roles = UserAuth.Admin)]
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
            IQueryable<Users> query = Db.Users.OrderByDescending(t => t.U_RegistDate);
            int count = query.Count();
            query = query.Skip((page - 1) * rows).Take(rows);
            //json时间转换
            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter {DateTimeFormat = "yyyy-MM-dd HH:mm:ss"};
            return JsonConvert.SerializeObject(new
            {
                total = count,
                rows = query.ToList().Select(t => new
                {
                    t.U_ID,
                    U_PassWord = Des.DesDecrypt(t.U_PassWord), 
                    t.U_Name, 
                    t.U_Tel,
                    t.U_LastLoginTime,
                    t.U_Role, 
                    t.U_RegistDate
                })
            }, timeConverter);
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
        public string AddUser(Users _user)
        {
            _user.U_Role = 1;
            return InsertUser(_user);
        }
        public string ManageAddUser(Users _user)
        {
            _user.U_RegistDate = DateTime.Now;
            return InsertUser(_user);
        }
        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string InsertUser(Users _user)
        {
            //验证登录信息是否完整
            if (string.IsNullOrEmpty(_user.U_Name) || string.IsNullOrEmpty(_user.U_PassWord) || string.IsNullOrEmpty(_user.U_Tel))
            {
                return JsonConvert.SerializeObject(new { flag = "False", Message = "请填写完整登录信息！" });
            }

            //验证邮箱格式
            Regex r = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            if (!r.IsMatch(_user.U_Tel)) 
                return JsonConvert.SerializeObject(new { flag = "False", Message = "邮箱格式不正确！" });

            //判断用户是否被注册
            Users result = Db.Users.FirstOrDefault(t => t.U_Name == _user.U_Name);
            if (result != null) 
                return JsonConvert.SerializeObject(new { flag = "Exits", Message = "该用户名已被注册！" });

            _user.U_ID = Guid.NewGuid();
            _user.U_PassWord = Des.DesEncrypt(_user.U_PassWord);
            _user.U_RegistDate = DateTime.Now;
            Db.Entry(_user).State=EntityState.Added;
            Db.SaveChanges();
            return JsonConvert.SerializeObject(new { flag = "Success", Message = "注册成功！" });
        }
        /// <summary>
        /// 根据用户名邮箱获取用户信息
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult FindUserInfo(Users _user)
        {
            _user = Db.Users.FirstOrDefault(t => t.U_Name == _user.U_Name && t.U_Tel == _user.U_Tel);
            if (_user == null) 
                return Json(new {flag = "False", Message = "验证信息不正确！"});

            Session["User"] = _user;
            return Json(new { flag = "Success", Message = "验证成功！" });
        }

        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpUserPass(string passWord)
        {
            if (Session["User"] == null)
            {
                return Json(new { flag = "False", Message = "验证信息已过期，请重新获取！" });
            }

            Users _user = (Users)Session["User"];
            _user.U_PassWord = Des.DesEncrypt(passWord);
            Db.Entry(_user).State = EntityState.Modified;
            Db.SaveChanges();
            return Json(new { flag = "Success", Message = "修改成功！" });
        }
        /// <summary>
        /// 登录后修改密码
        /// </summary>
        /// <param name="uPassWord"></param>
        /// <param name="uPassWordConfirm"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdatePass(string uPassWord,string uPassWordConfirm)
        {
            if (string.IsNullOrEmpty(user.U_Name)) 
                return Json(new {flag = "False", Message = "获取用户信息失败！"});

            uPassWord = Des.DesEncrypt(uPassWord);
            Users newuser = Db.Users.FirstOrDefault(t => t.U_Name == user.U_Name && t.U_PassWord == uPassWord);
            if (newuser == null) 
                return Json(new {flag = "False", Message = "原始密码不正确！"});

            newuser.U_PassWord = Des.DesEncrypt(uPassWordConfirm);
            Db.Entry(newuser).State = EntityState.Modified;
            Db.SaveChanges();
            return Json(new { flag = "Success", Message = "修改成功！" });
        }

        /// <summary>
        /// 修改用户
        /// </summary>
        /// <returns></returns>
        public string UpdateUser(Users _user)
        { //验证登录信息是否完整
            if (string.IsNullOrEmpty(_user.U_Name) || string.IsNullOrEmpty(_user.U_PassWord) 
                || string.IsNullOrEmpty(_user.U_Tel))
            {
                return JsonConvert.SerializeObject(new { flag = "False", Message = "请填写完整登录信息！" });
            }

            //验证邮箱格式
            Regex r = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            if (!r.IsMatch(_user.U_Tel)) 
                return JsonConvert.SerializeObject(new { flag = "False", Message = "邮箱格式不正确！" });

            _user.U_PassWord = Des.DesEncrypt(_user.U_PassWord);
            Db.Entry(_user).State = EntityState.Modified;
            Db.SaveChanges();
            return JsonConvert.SerializeObject(new { flag = "Success", Message = "修改成功！" });
        }

        public ActionResult IsExit(Users _user)
        {
            //判断用户是否被注册
            Users result = Db.Users.FirstOrDefault(t => t.U_Name == _user.U_Name);
            return Json(result != null ? new { flag = "False", Message = "该用户名已被注册！" }
                : new { flag = "Success", Message = "用户名不存在！" });
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <returns></returns>
        public string DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id)) 
                return JsonConvert.SerializeObject(new { flag = "False", Message = "用户不能为空！" });

            id.Split(',').ToList().ForEach(e => {
                var userid = Guid.Parse(e);
                Users _user = new Users { U_ID = userid };
                Db.Entry(_user).State = EntityState.Deleted;
                var category = Db.Category.Where(t => t.C_UsersID == userid);
                StringBuilder sb=new StringBuilder();
                foreach (var item in category)
                {
                    sb.Append(item.C_ID+",");
                }
                CategoryController c = new CategoryController();
                c.DeleteCate(sb.ToString());
            });
            Db.SaveChanges();
            return JsonConvert.SerializeObject(new { flag = "Success", Message = "删除成功！" });
        }
    }
}
