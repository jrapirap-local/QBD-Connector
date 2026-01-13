using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MBPH_Connector.Repositories;
using MBPH_Connector.IRepositories;
using System.Windows.Forms;
using MBPHReader;
using System.Data;
using MBPH.Extension;
namespace MBPH_Connector.Abstraction
{
    public class Process
    {


        private readonly ICompanyBinding _com; //<scope,scoped>
        private readonly ISyncLists _lists;
        private readonly ISyncBillsAndPayment _billAndPayment;
        
        public Process() {
            _com = new CompanyBinding();
            _lists = new SyncLists();
            _billAndPayment = new SyncBillsAndPayment();
        }

        
        public async Task BindCompany() {
            var isValid =await _com.IsValidCompany();
            if(isValid)
                await _com.BindCompany();
        }

        public async Task DownloadQBDData()
        {
            try
            {
                var isValid = await _com.IsValidCompany();
                if (isValid)
                {
                    await _lists.Download();
                }
            }
            catch
            {

            }
        }
        public async Task DoneSyncing(int id)
        {
            var data = new DataTable();
            data.Columns.Add("CompanyID");
            data.Columns.Add("ItemID");
            var row = data.NewRow();
            await Task.Factory.StartNew(() =>
            {

                row["CompanyID"] = Session.UserSession.CompanyID;
                row["ItemID"] = id; 
            });
            await MyBillsPH.DoneSyncing(row.ModelToDataTable());
            
            
            
        }

        public async Task SyncVendors() { //3
            try
            {
                await _lists.SyncVendors();

                await DoneSyncing(1);
            }
            finally { }
        }
        public async Task SyncClasses()//4
        {
            try
            {
                await _lists.SyncClasses();
                await DoneSyncing(2);
            }
            finally { }
        }
        public async Task SyncProjects()//5
        {
            try
            {
                await _lists.SyncProjects();
                await DoneSyncing(3);
            }
            finally { }
        }
        public async Task SyncAccounts()//6
        {
            try
            {
                await _lists.SyncAccounts();
                await DoneSyncing(4);
            }
            finally { }
        }

        public async Task SyncBillsAndPayments()//7
        {
            //try
            //{
            //    await _billAndPayment.SyncBills();
            //    await _billAndPayment.SyncDeletedBills();
            //    await DoneSyncing(5);
            //}
            //finally { }
            try
            {
                await _billAndPayment.SyncBillsPayments(); //Sync payment update, bill update, new bill and new payment
                await _billAndPayment.SyncDeletedBills();
                //await DoneSyncing(5); //For Bills
                await DoneSyncing(6); //For Payments
            }
            finally { }

        }
        
        //public async Task SyncPayments()//9
        //{
        //    try
        //    {
        //        //await _billAndPayment.SyncPayments();
        //        await DoneSyncing(6);
        //    }
        //    finally { }
        //}



    }
}
