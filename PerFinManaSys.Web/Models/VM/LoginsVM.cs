using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PerFinManaSys.Web.Models.VM
{
    public class LoginsVM
    {
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string RoleName { get; set; }
        public string HostName { get; set; }
        public string IP { get; set; }
        public string City { get; set; }
        public DateTime LoginTime { get; set; }
    }
}