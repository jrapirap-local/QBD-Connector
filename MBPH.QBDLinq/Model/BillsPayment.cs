using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBPH.QBDLinq.Model
{
    public class BillsPayment
    {
        public int PaymentID { get; set; }
        public string TransactionID { get; set; }
        public string BillLinkID { get; set; }
        public int CompanyID { get; set; }
        public string SyncedBy { get; set; }
        
        public bool QBDPaid { get; set; }
        public string Payment { get; set; }
        public decimal PaidAmount { get; set; }
    }
}
