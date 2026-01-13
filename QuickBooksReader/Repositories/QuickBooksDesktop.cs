using MBPH.QBDLinq.Model;
using QuickBooksReader.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuickBooksReader.QBD;
using MBPH.QBDLinq;
//using QBFC14Lib;
using QBFC13Lib;
namespace QuickBooksReader.Repositories
{
    public class QuickBooksDesktop : IQuickBooksDesktop
    {
        //public QuickBooksDesktop() {
        //    if (!OpenQBD())
        //    {
        //        throw new Exception("Please Open QuickBooks Desktop"); 
        //    }
        //}
        //~QuickBooksDesktop()
        //{
        //    CloseQBD();
        //}
        
        public List<BillPaymentCheckRet> EnumerateBillsPaymentCheck(string BindedCompany="")
        {
            OpenQBD(BindedCompany);
            request.AppendBillPaymentCheckQueryRq();
            var ret = sessionManager.DoRequests(request).ToXMLString().QueryBillPaymentCheckList();
            CloseQBD();
            return ret;
        }

        public List<AccountRet> EnumerateAccounts(string BindedCompany = "")
        {
            OpenQBD(BindedCompany);
            var acc = request.AppendAccountQueryRq();
            acc.ORAccountListQuery.AccountListFilter.ActiveStatus.SetValue(ENActiveStatus.asAll);
            var ret = sessionManager.DoRequests(request).ToXMLString().QueryAccountList();
            CloseQBD();
            return ret;
        }
        public List<ListDeletedRet> EnumerateDeletedAccounts(string BindedCompany = "")
        {
            OpenQBD(BindedCompany);
            var acc = request.AppendListDeletedQueryRq();
            acc.ListDelTypeList.Add(ENListDelType.ldtAccount);
            var ret = sessionManager.DoRequests(request).ToXMLString().QueryDeletedList();
            CloseQBD();
            return ret;
        }

        public List<BillRet> EnumerateBills(string BindedCompany = "")
        {
            OpenQBD(BindedCompany);
            var req = request.AppendBillQueryRq();
            req.IncludeLineItems.SetValue(true);
            var ret = sessionManager.DoRequests(request).ToXMLString().QueryBillList();
            CloseQBD();
            return ret;
        }

        public List<ClassRet> EnumerateClass(string BindedCompany = "")
        {
            OpenQBD(BindedCompany);
            var cls = request.AppendClassQueryRq();
            cls.ORListQuery.ListFilter.ActiveStatus.SetValue(ENActiveStatus.asAll); // Get all item from active list
            var ret = sessionManager.DoRequests(request).ToXMLString().QueryClassList();
            CloseQBD();
            return ret;
        }

        public List<ListDeletedRet> EnumerateDeletedClass(string BindedCompany = "")
        {
            OpenQBD(BindedCompany);
            var cl = request.AppendListDeletedQueryRq();
            cl.ListDelTypeList.Add(ENListDelType.ldtClass);
            var ret = sessionManager.DoRequests(request).ToXMLString().QueryDeletedList();
            CloseQBD();
            return ret;
        }
        public List<ListDeletedRet> EnumerateDeletedVendors(string BindedCompany = "")
        {//RRIEL Added deleted tables for vendors
            OpenQBD(BindedCompany);
            var cl = request.AppendListDeletedQueryRq();
            cl.ListDelTypeList.Add(ENListDelType.ldtVendor);
            var ret = sessionManager.DoRequests(request).ToXMLString().QueryDeletedList();
            CloseQBD();
            return ret;
        }

        public List<CustomerRet> EnumerateProjects(string BindedCompany = "")
        {
            OpenQBD(BindedCompany);
            var cls = request.AppendCustomerQueryRq();
            cls.ORCustomerListQuery.CustomerListFilter.ActiveStatus.SetValue(ENActiveStatus.asAll); // Get all item from active list
            var ret = sessionManager.DoRequests(request).ToXMLString().QueryProjectsList();
            CloseQBD();
            return ret;
        }

        public List<ListDeletedRet> EnumerateDeletedProjects(string BindedCompany = "")
        {
            OpenQBD(BindedCompany);
            var acc = request.AppendListDeletedQueryRq();
            acc.ListDelTypeList.Add(ENListDelType.ldtCustomer);
            var ret = sessionManager.DoRequests(request).ToXMLString().QueryDeletedList();
            CloseQBD();
            return ret;
        }

        public List<VendorRet> EnumerateVendors(string BindedCompany = "")
        {
            OpenQBD(BindedCompany);
            var ven = request.AppendVendorQueryRq();
            ven.ORVendorListQuery.VendorListFilter.ActiveStatus.SetValue(ENActiveStatus.asAll); // include inactive
            var ret = sessionManager.DoRequests(request).ToXMLString().QueryVendorList();
            CloseQBD();
            return ret;
        }
    }
}
