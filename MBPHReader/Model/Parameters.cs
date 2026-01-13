using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBPHReader.Model
{
    public class Parameters
    {
        public int CompanyID { get; set; }
        public string SyncedBy { get; set; }
        public string SyncGUID { get; set; }
        //Accounts
        public string COAListID { get; set; }
        public int COAID { get; set; }

        //Vendors
        public string VendorListID { get; set; }
        public int VendorID { get; set; }
        public string VendorName { get; set; }  // ADDED BY ADOME05082024

        //Class
        public string ClassificationListID { get; set; }
        public string Classification { get; set; }

        //Bills BillLinkID
        public int MWID { get; set; }
        public string TransactionID { get; set; }
        public string BillLinkID { get; set; }
        public int TotalNoOfBills { get; set; }

        //Payment
        public int PaymentID { get; set; }
        public string PaymentTxnID { get; set; }
        public string QBDCompany { get; set; }
        public string ProjectCodingDesc { get; set; }
        public string DepartmentDescription { get; set; }
        public int DepartmentID { get; set; }
        public string EditSequence { get; set; }
        public int ProjectCodingID { get; set; }
        public string BiosComputerSerial { get; set; }
        public string ServerName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? SyncStartDate { get; set; } = null;
        public int AppID { get; set; }
        public string AccountingAppID { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string ResultMessage { get; set; }
        public string Table { get; set; }
        public string ColumnID { get; set; }
        public string Action { get; set; }
        public string Value { get; set; }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string ExceptionMessage { get; set; }
        public string InnerException { get; set; }
    }
}
