using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBPH_Connector.Model
{
    public class USER_INFO
    {
        public string CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string AddedBy { get; set; }
        public string SessionID { get; set; }

        public int Step { get; set; } // 1 , 2, 3,4
        public string QBDCompany { get; set; } //
        public DateTime? SyncStartDate { get; set; } = null; //
        public DateTime? SyncEndDate { get; set; } = null;//



        public string SelectedPaymentType { get; set; }//
        
        public int MBPHModuleID { get; set; } // 
        public bool PaidBills { get; set; } // okey
        public int Module { get; set; } 
    }
}
