using MBPH.QBDLinq.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickBooksReader.IRepositories
{
    public interface IQuickBooksDesktop
    {
        List<BillPaymentCheckRet> EnumerateBillsPaymentCheck(string BindedCompany = "");
        List<BillRet> EnumerateBills(string BindedCompany = "");
        List<VendorRet> EnumerateVendors(string BindedCompany = "");
        List<AccountRet> EnumerateAccounts(string BindedCompany = "");
        List<ClassRet> EnumerateClass(string BindedCompany = "");
        List<CustomerRet> EnumerateProjects(string BindedCompany = "");
        List<ListDeletedRet> EnumerateDeletedVendors(string BindedCompany = "");
        List<ListDeletedRet> EnumerateDeletedClass(string BindedCompany = "");
        List<ListDeletedRet> EnumerateDeletedProjects(string BindedCompany = "");
        List<ListDeletedRet> EnumerateDeletedAccounts(string BindedCompany = "");

    }
}
