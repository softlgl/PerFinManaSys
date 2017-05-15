using Newtonsoft.Json;
using PerFinManaSys.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace PerFinManaSys.Web.Auth
{
    public class FormsAuth
    {
        public static void SignIn(Users user)
        {
            var userJson = JsonConvert.SerializeObject(user);
            var ticket = new FormsAuthenticationTicket(1, user.U_Name, DateTime.Now, DateTime.Now.AddDays(1), true, userJson);
            string cookieString = FormsAuthentication.Encrypt(ticket);
            HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, cookieString);
            authCookie.Expires = ticket.Expiration;
            authCookie.Path = FormsAuthentication.FormsCookiePath;
            HttpContext.Current.Response.Cookies.Remove(authCookie.Name);
            HttpContext.Current.Response.Cookies.Add(authCookie);
        }

        public static void SingOut()
        {
            FormsAuthentication.SignOut();
        }

        public static Users GetUserData()
        {
            return GetUserData<Users>();
        }

        public static T GetUserData<T>() where T : class, new()
        {
            var UserData = new T();
            try
            {
                var context = HttpContext.Current;
                var cookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                UserData = JsonConvert.DeserializeObject<T>(ticket.UserData);
            }
            catch
            { }

            return UserData;
        }
    }
}