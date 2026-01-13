using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBPH.QBDLinq.Model
{
    public class QBDBills
    {
        public int MWID { get; set; }
        public string BillLinkID { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string VendorListID { get; set; }
        public string AccountName { get; set; }
        public string Memo { get; set; }
        public string InvoiceNumber { get; set; }
        public string TxnLineID { get; set; }
    }
}
