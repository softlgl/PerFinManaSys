using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PerFinManaSys.Web.Models.VM
{
    public class ExpenseVM
    {
        public string E_Type { get; set; }
        public decimal E_Amount { get; set; }
        public string E_Date { get; set; }
        public string E_Remark { get; set; }
    }
}