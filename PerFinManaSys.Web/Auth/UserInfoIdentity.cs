﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace PerFinManaSys.Web.Auth
{
    public class UserInfoIdentity : IIdentity
    {
        private FormsAuthenticationTicket ticket;
        private HttpContext context = HttpContext.Current;

        public UserInfoIdentity(FormsAuthenticationTicket ticket)
        {
            this.ticket = ticket;
        }

        private string userId;
        private string userName;
        private int isAdmin;

        public UserInfoIdentity(string userid, string userName, int isAdmin)
        {
            this.userId = userid;
            this.userName = userName;
            this.isAdmin = isAdmin;
        }


        public string UserID
        {
            get
            {
                return this.userId;
            }
        }
        public string UserName
        {
            get
            {
                return this.userName;
            }
        }
        public int IsAdmin
        {
            get
            {
                return this.isAdmin;
            }
        }

        public string AuthenticationType
        {
            get
            {
                return "Forms";
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return true;
            }
        }

        public string Name
        {
            get
            {
                return this.userName;
            }
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", UserID, UserName, IsAdmin);
        }
    }
}