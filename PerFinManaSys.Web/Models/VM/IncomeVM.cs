using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PerFinManaSys.Web.Models.VM
{
    public class IncomeVM
    {
        public decimal I_Amount { get; set; }
        public DateTime I_Date { get; set; }
        public string I_Remark { get; set; }
    }
}