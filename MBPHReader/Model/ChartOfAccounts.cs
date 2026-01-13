using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBPHReader.Model
{
    public class ChartOfAccounts
    {
        public int COAID { get; set; }
        public string CompanyID { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public string Classification { get; set; }
        public string Description { get; set; }
        public bool IsShownToBP { get; set; }
        public bool IsEditable { get; set; }
        //public bool IsAlwaysActive { get; set; }
        //public string CreatedBy { get; set; }
        public string AddedBy { get; set; }
        public string SessionID { get; set; }
        public string ListID { get; set; }
        public string SyncGUID { get; set; }
        public string EditSequence { get; set; }
        public string COAListID { get; set; }
        public string ClassificationListID { get; set; }
        public bool IsActive { get; set; }
    }
}
