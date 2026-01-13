using MBPH.QBDLinq.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickBooksReader.Repositories;
using QuickBooksReader.IRepositories;

namespace QuickBooksReader.Library
{
    public static class QBD
    {
        public static List<BillRet> Bills = new List<BillRet>();
        public static List<VendorRet> Vendors = new List<VendorRet>(); //QBD VendorLists
        public static List<AccountRet> Accounts = new List<AccountRet>(); //QBD AccountsLIsts
        public static List<ClassRet> Classes = new List<ClassRet>(); //QBD Class
        public static List<CustomerRet> Projects = new List<CustomerRet>(); //QBD Project ADDED By JRAPI
        public static List<ListDeletedRet> DeletedVendors = new List<ListDeletedRet>(); //QBD Vendors by RRIEL
        public static List<ListDeletedRet> DeletedClasses = new List<ListDeletedRet>(); //QBD Class
        public static List<ListDeletedRet> DeletedProjects = new List<ListDeletedRet>(); //QBD Project ADDED By JRAPI
        public static List<BillPaymentCheckRet> BillsPaymentChecks = new List<BillPaymentCheckRet>();
        public static List<ListDeletedRet> DeletedAccounts = new List<ListDeletedRet>();

        public static IQuickBooksDesktop _irepo = new QuickBooksDesktop();

        public static async Task<bool> ReadData(string BindedCompany="") {

            try
            {
                Bills = _irepo.EnumerateBills(BindedCompany);
                Vendors = _irepo.EnumerateVendors(BindedCompany);
                Accounts = _irepo.EnumerateAccounts(BindedCompany);
                Classes = _irepo.EnumerateClass(BindedCompany);
                Projects = _irepo.EnumerateProjects(BindedCompany);
                DeletedVendors = _irepo.EnumerateDeletedVendors(BindedCompany);
                DeletedClasses = _irepo.EnumerateDeletedClass(BindedCompany);
                DeletedProjects = _irepo.EnumerateDeletedProjects(BindedCompany);
                BillsPaymentChecks = _irepo.EnumerateBillsPaymentCheck(BindedCompany);
                DeletedAccounts = _irepo.EnumerateDeletedAccounts(BindedCompany);
                return await Task.Factory.StartNew(()=> true);
                
            }
            catch(Exception ex) {
                return await Task.Factory.StartNew(() => false);
            }
        }
        public static void ClearData() {
            Bills = new List<BillRet>();
            Vendors = new List<VendorRet>(); //QBD VendorLists
            Accounts = new List<AccountRet>(); //QBD AccountsLIsts
            Classes = new List<ClassRet>(); //QBD Class
            Projects = new List<CustomerRet>(); //QBD Projects
            DeletedClasses = new List<ListDeletedRet>(); //QBD Deleted Class
            DeletedProjects = new List<ListDeletedRet>(); //QBD Deleted Projects
            BillsPaymentChecks = new List<BillPaymentCheckRet>();
    }

    }
}
