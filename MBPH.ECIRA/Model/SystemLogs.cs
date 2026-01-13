using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBPH.ECIRA.Model
{
    public class SystemLogs
    {
        public int CompanyID { get; set; }
        public string SessionID { get; set; }
        public int? ErrorID { get; set; }
        //public int? SuccessID { get; set; } // no need in param
        public int MBPHModuleID { get; set; }
        public string MBPHModule { get; set; }
        public string MethodName { get; set; }
        public bool Success { get; set; } = true; // IsSuccess 
        public string AddedBy { get; set; }
        public List<SystemLogDetails>  Details { get; set; }
    }

    public class SystemLogDetails
    {
        public int SystemLogID { get; set; }
        public string ActionDone { get; set; }
        public string Value { get; set; }
        public string Remarks { get; set; }
    }
}
