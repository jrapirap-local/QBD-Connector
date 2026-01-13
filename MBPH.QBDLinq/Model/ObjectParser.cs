using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace MBPH.QBDLinq.Model
{
    public class QBObjects
    {
        [JsonProperty("VendorRet")]
        public VendorRet VendorRet { get; set; }

        [JsonProperty("AccountRet")]
        public AccountRet AccountRet { get; set; }

        [JsonProperty("BillRet")]
        public BillRet BillRet { get; set; }

        [JsonProperty("ClassRet")]
        public ClassRet ClassRet { get; set; }

        [JsonProperty("CustomerRet")]
        public CustomerRet CustomerRet { get; set; }


        //BillPaymentCheckRet
        [JsonProperty("BillPaymentCheckRet")]
        public BillPaymentCheckRet BillPaymentCheckRet { get; set; }

        [JsonProperty("ListDeletedRet")]
        public ListDeletedRet ListDeletedRet { get; set; }

    }
    public class BillPaymentCheckRet
    {
        public string Memo { get; set; }
        public string EditSequence { get; set; }
        public string TxnID { get; set; }
        public string TxnNumber { get; set; }
        public PayeeEntityRef PayeeEntityRef { get; set; } // Payee under BillPaymentCheckRet objectmodel
        public APAccountPayment APAccountRef { get; set; } // COA under BillPaymentCheckRet objectmodel
        public BankAccountRef BankAccountRef { get; set; } // PaymentSource under BillPaymentCheckRet objectmodel
        public string TxnDate { get; set; }
        public string Amount { get; set; }
    }
    public class PayeeEntityRef
    {
        public string ListID { get; set; }
        public string FullName { get; set; }
    }
    public class APAccountPayment
    {
        public string ListID { get; set; }
        public string FullName { get; set; }
    }
    public class BankAccountRef
    {
        public string ListID { get; set; }
        public string FullName { get; set; }
    }
    public class AccountRef
    {
        public string ListID { get; set; }
        public string FullName { get; set; }
    }
    public class ClassRef
    {
        public string ListID { get; set; }
        public string FullName { get; set; }
        
    }
    public class ExpenseLineRet
    {
        public string TxnLineID { get; set; }
        public AccountRef AccountRef { get; set; }
        public DateTime TxnDate { get; set; }
        public long Amount { get; set; }
        public string Memo { get; set; }
        public ClassRef ClassRef { get; set; }
    }
    public class ClassRet
    {
        //Object Name depends on actual Object name from QBD xml file, for manageability the same name was used.
        public string ListID { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string EditSequence { get; set; }
        public bool IsActive { get; set; }
    }
    public class ClassRetDel
    {
        //Object Name depends on actual Object name from QBD xml file, for manageability the same name was used.
        public string ListID { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string EditSequence { get; set; }
        public bool IsActive { get; set; }
    }
    public class CustomerRet //added by JRAPI
    {
        //Object Name depends on actual Object name from QBD xml file, for manageability the same name was used.
        public string ListID { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string SubLevel { get; set; }
        public string EditSequence { get; set; }
        public bool IsActive { get; set; }
        public ParentRef ParentRef { get; set; } // Customername under parentref objectmodel
    }
    public class CustomerRetDel //added by JRAPI
    {
        //Object Name depends on actual Object name from QBD xml file, for manageability the same name was used.
        public string ListID { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string SubLevel { get; set; }
        public string EditSequence { get; set; }
        public bool IsActive { get; set; }
        public ParentRef ParentRef { get; set; } // Customername under parentref objectmodel
    }

    public class ParentRef
    {
        public string FullName { get; set; }
    }

    public class BillRet
    {
        public string TxnID { get; set; }
        public string EditSequence { get; set; }
        public string TxnNumber { get; set; }
        public VendorRet VendorRef { get; set; }
        public AccountRet APAccountRef { get; set; }
        
        public DateTime TxnDate { get; set; }
        public DateTime DueDate { get; set; }
        public double AmountDue { get; set; }
        public bool IsPaid { get; set; }
        public string Memo { get; set; }
        public double OpenAmount { get; set; }
        public string RefNumber { get; set; } // RRIEL10282024 added ref number here
        //public List<ExpenseLineRet> ExpenseLineRet { get; set; }
    }
    public class ListDeletedRet
    {
        public string ListDelType { get; set; }
        public string ListID { get; set; }
        public DateTime TimeCreated { get; set;}
        public DateTime TimeDeleted { get; set;}
        
        public string FullName { get; set;}


    }

    public class VendorRet
    {
        public string ListID { get; set; }
        public string EditSequence { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public string CompanyName { get; set; }
        public VendorAddress VendorAddress { get; set; }
        public string VendorTaxIdent { get; set;}

    }
    public class VendorAddress
    {
        public string Addr1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
    public class AccountRet
    {
        public string ListID { get; set; }
        public string EditSequence { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public int Sublevel { get; set; }
        public string AccountType { get; set; }
        public string AccountNumber { get; set; }
        public string Desc { get; set; }
        public double Balance { get; set; }
        public double TotalBalance { get; set; }
        public string CashFlowClassification { get; set; }
        public bool IsActive { get; set; }
        public string CompanyName { get; set; }

    }
}
