using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace PerFinManaSys.Web.Auth
{
    public class UserInfoPrincipal : IPrincipal
    {
        private UserInfoIdentity identity;
        public UserInfoPrincipal(string UserId,string UserName,int IsAdmin)
        {
            this.UserId = UserId;
            this.UserName = UserName;
            this.IsAdmin = IsAdmin;
            this.identity = new UserInfoIdentity(UserId, UserName, IsAdmin);
        }

        public string UserId { set; get; }
        public string UserName { set; get; }
        public int IsAdmin { set; get; }

        public IIdentity Identity
        {
            get
            {
                return identity;
            }
        }

        public bool IsInRole(string role)
        {
            if (this.IsAdmin == 1 && role.ToLower() == "admin")
            {
                return true;
            }
            if (this.IsAdmin == 0 && role.ToLower() == "user")
            {
                return true;
            }
            return false;
        }
    }
}