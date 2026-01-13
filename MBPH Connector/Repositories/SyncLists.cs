using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MBPH.QBDLinq.Model;
using MBPH_Connector.IRepositories;
using QuickBooksReader.Library;
using MBPH.Extension;
using MBPHReader;
using System.Data;
using MBPHReader.Model;
using static MBPH_Connector.Abstraction.Session;
using Newtonsoft.Json;
using System.Windows.Forms;
using MBPH_Connector;
using QBFC13Lib;
using MBPH_Connector.Model;
using static MBPH_Connector.Repositories.AsyncECIRA;
using static QuickBooksReader.QBD;
using MBPH_Connector.Abstraction;
using static MBPHReader.MyBillsPH;
using MBPH.CMD;
using MBPH.ECIRA.Model;
using static System.Collections.Specialized.BitVector32;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics.Eventing.Reader;

namespace MBPH_Connector.Repositories
{
    class SyncLists : ISyncLists
    {
        private static readonly ENAccountType[] accountEnum = (ENAccountType[])Enum.GetValues(typeof(ENAccountType));
        private static readonly List<List<string>> AccountTypeMapping = new List<List<string>>()
        {
            new List<string>{ "Accounts Payable", "0"},
            new List<string>{ "Bank", "2"},
            new List<string>{ "Cost of Goods Sold", "3"},
            new List<string>{ "Current Asset", "12"},
            new List<string>{ "Equity", "12"},
            new List<string>{ "Current Liability", "13"},
            new List<string>{ "Expenses", "6"},
            new List<string>{ "Expense", "6"},
            new List<string>{ "Fixed Asset", "7"},
            new List<string>{ "Inventory", "12"},
            new List<string>{ "Other Current Asset", "12"},
            new List<string>{ "Other Current Liability", "13"},
            new List<string>{ "Long Term Liability", "13"},
            new List<string>{ "Income", "8"},
            new List<string>{ "Other Expense", "14"},
            new List<string>{ "Other Income", "15"},

            //RRIEL 09112024 double validation for resyncing etc.
            new List<string>{ "AccountsPayable", "0"},
            new List<string>{ "CostOfGoodsSold", "3"},
            new List<string>{ "CurrentAsset", "12"},
            new List<string>{ "CurrentLiability", "13"},
            new List<string>{ "FixedAsset", "7"},
            new List<string>{ "OtherCurrentAsset", "12"},
            new List<string>{ "OtherCurrentLiability", "13"},
            new List<string>{ "LongTermLiability", "13"},
            new List<string>{ "OtherExpense", "14"},
            new List<string>{ "OtherIncome", "15"},

        };

        public async Task Download()
        {
            DataTable sync = new DataTable();
            await AsyncCloseConnection();
            await AsyncOpenConnection();
            await QBD.ReadData();
            await Task.Factory.StartNew(() =>
            {
                sync.Columns.Add("SyncID");
                sync.Columns.Add("CompanyID");
                var row = sync.NewRow();
                row["SyncID"] = SyncID;
                row["CompanyID"] = UserSession.CompanyID;
                sync.Rows.Add(row);
            });
            await MyBillsPH.UpdateSyncGuid(sync);
            await Classes.ResetClass(sync);
            //await Classes.ResetDeletedClass(sync);
            await Projects.ResetProject(sync);
            //await Projects.ResetDeletedProject(sync);
            foreach (var x in QBD.Accounts)
            {
                var data = new DataTable();

                await Task.Factory.StartNew(() =>
                {
                    data = x.ModelToDataTable();
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Columns.Add("CompanyID");
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Rows[0]["CompanyID"] = UserSession.CompanyID;
                });
                await Accounts.Download(data);
            }
            
            foreach (var x in QBD.Classes)
            {
                var data = new DataTable();

                await Task.Factory.StartNew(() =>
                {
                    data = x.ModelToDataTable();
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Columns.Add("CompanyID");
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Rows[0]["CompanyID"] = UserSession.CompanyID;
                });
                await Classes.Download(data);
            }

            foreach (var x in QBD.DeletedVendors)
            {
                var data = new DataTable();

                await Task.Factory.StartNew(() =>
                {
                    data = x.ModelToDataTable();
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Columns.Add("CompanyID");
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Rows[0]["CompanyID"] = UserSession.CompanyID;
                });
                await Vendors.DownloadDeleted(data);
            }

            foreach (var x in QBD.DeletedClasses)
            {
                var data = new DataTable();

                await Task.Factory.StartNew(() =>
                {
                    data = x.ModelToDataTable();
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Columns.Add("CompanyID");
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Rows[0]["CompanyID"] = UserSession.CompanyID;
                });
                await Classes.DownloadDeleted(data);
            }
            
            foreach (var x in QBD.Projects)
            {
                var data = new DataTable();

                await Task.Factory.StartNew(() =>
                {
                    data = x.ModelToDataTable();
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Columns.Add("CompanyID");
                    data.Columns.Add("CustomerName");
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Rows[0]["CompanyID"] = UserSession.CompanyID;
                    data.Rows[0]["CustomerName"] = x.ParentRef.FullName;
                });
                await Projects.Download(data);
            }
            
            foreach (var x in QBD.DeletedProjects)
            {
                var data = new DataTable();

                await Task.Factory.StartNew(() =>
                {
                    data = x.ModelToDataTable();
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Columns.Add("CompanyID");
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Rows[0]["CompanyID"] = UserSession.CompanyID;
                });
                await Projects.DownloadDeleted(data);
            }

            // ADDED BY ADOME01082024 'For validation of Vendor Legal Name for QBD vendor created
            //if (QBD.Vendors.Count > 0) //RRIEL Comment. this is unneccessary. foreach has it own validation if there are records. look for current struc
            //{

            //}
            foreach (var x in QBD.Vendors)
            {
                var data = new DataTable();
                var addr_details = new DataTable();  //RRIEL nasa head naman na yung TaxIden
                await Task.Factory.StartNew(() => {
                    addr_details = x.VendorAddress.ModelToDataTable(); //get this first
                });
                await Task.Factory.StartNew(() =>
                {
                    
                    data = x.ModelToDataTable();
                    data.Columns.Remove("VendorAddress");//then removed after useage
                });
                
                await Task.Factory.StartNew(() =>
                {
                    if (addr_details.Rows.Count>0) {
                        addr_details.Columns.Add("ListID");
                        addr_details.Columns.Add("CompanyID");
                    }
                    
                    data.Columns.Add("CompanyID");
                });
                await Task.Factory.StartNew(() =>
                {
                    if (addr_details.Rows.Count > 0)
                    {
                        addr_details.Rows[0]["ListID"] = x.ListID.ToString();
                        addr_details.Rows[0]["CompanyID"] = UserSession.CompanyID;
                    }
                    data.Rows[0]["CompanyID"] = UserSession.CompanyID;
                });
                
                await Vendors.Download(data);
                if (addr_details.Rows.Count > 0)
                {
                    await Vendors.DownloadAddr(addr_details);
                }
                    
            }
            foreach (var x in QBD.DeletedAccounts)
            {
                var data = new DataTable();

                await Task.Factory.StartNew(() =>
                {
                    data = x.ModelToDataTable();
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Columns.Add("CompanyID");
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Rows[0]["CompanyID"] = UserSession.CompanyID;
                });
                await Accounts.DownloadDeleted(data);
            }
            foreach(var x in QBD.Bills)
            {
                var data = new DataTable();

                await Task.Factory.StartNew(() =>
                {
                    data = x.ModelToDataTable();
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Columns.Add("CompanyID");
                    data.Columns.Add("VendorListID");
                    data.Columns.Add("VendorName");
                    data.Columns.Add("AccountListsID");
                    data.Columns.Add("AccountName");
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Rows[0]["CompanyID"] = UserSession.CompanyID;
                    data.Rows[0]["VendorListID"] = x.VendorRef.ListID;
                    data.Rows[0]["VendorName"] = x.VendorRef.FullName;
                    data.Rows[0]["AccountListsID"] = x.APAccountRef.ListID;
                    data.Rows[0]["VendorName"] = x.APAccountRef.FullName;
                });

                await Bills.Download(data);
            }

            
            foreach (var x in QBD.BillsPaymentChecks)
            {

                var data = new DataTable();

                await Task.Factory.StartNew(() =>
                {
                    data = x.ModelToDataTable();
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Columns.Add("CompanyID");
                    data.Columns.Add("VendorListID");
                    //data.Columns.Add("TxnID");
                    //data.Columns.Add("EditSequence");
                    //data.Columns.Add("TxnNumber");
                    data.Columns.Add("VendorName");
                    data.Columns.Add("AccountListsID");
                    data.Columns.Add("AccountName");
                    data.Columns.Add("PaymentSourceID");
                    data.Columns.Add("PaymentSourceName");
                });
                await Task.Factory.StartNew(() =>
                {
                    data.Rows[0]["CompanyID"] = UserSession.CompanyID;
                    data.Rows[0]["VendorListID"] = x.PayeeEntityRef.ListID;
                    data.Rows[0]["TxnID"] = x.TxnID;
                    data.Rows[0]["EditSequence"] = x.EditSequence;
                    data.Rows[0]["TxnNumber"] = x.TxnNumber;
                    data.Rows[0]["VendorName"] = x.PayeeEntityRef.FullName;
                    data.Rows[0]["AccountListsID"] = x.APAccountRef.ListID;
                    data.Rows[0]["AccountName"] = x.APAccountRef.FullName;
                    data.Rows[0]["PaymentSourceID"] = x.BankAccountRef.ListID;
                    data.Rows[0]["PaymentSourceName"] = x.BankAccountRef.FullName;
                });
                await Payments.Download(data);
            }
            await MyBillsPH.DoneDownload(UserSession);
        }

        public async Task SyncAccounts()
        {
            //throw new NotImplementedException();
            var data = await Accounts.Upload(UserSession);
            
            await Task.Factory.StartNew(() =>
            {
                data.Columns.Add("SyncedBy");
                data.Columns.Add("IsSuccess");
                data.Columns.Add("ItemID");
            });
            foreach (DataRow x in data.Rows)
            {
                try
                {
                int subIndex = 0;
                foreach (string sub in x["Name"].ToString().Split(':')) { //RRIEL simple code. just added this. then it will automatically add everything if not exists.
                    await QBD.ReadData(); //refresh data. to fetch updated data
                    await AsyncOpenConnection();
                    if (x["ListID"].Equals("0"))
                    {//new
                        var acc = request.AppendAccountAddRq();
                        acc.Name.SetValue(sub);
                        if (subIndex > 0) //RRIEL also handle all.
                        {
                            
                            string parentListsID = QBD.Accounts.FirstOrDefault(f => f.Name == x["Name"].ToString().Split(':')[subIndex - 1].ToString()).ListID;//getting parent lists id
                            acc.ParentRef.ListID.SetValue(parentListsID);
                            //acc.ParentRef.FullName.SetValue(QBD.Accounts.FirstOrDefault(f => f.Name == x["Name"].ToString().Split(':')[subIndex - 1].ToString()).FullName);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(x["AccountNumber"])))
                        {
                            acc.AccountNumber.SetValue(Convert.ToString(x["AccountNumber"]));
                        }
                        string accType = string.Empty;
                        try
                        {
                            accType = AccountTypeMapping.Find(f => f[0] == x["AccountType"].ToString())[1];
                        }
                        catch
                        {
                            accType = "14"; // default for now. for unregistered Type
                        }
                        var accountIndex = Convert.ToInt32(accType);
                        ENAccountType accountType = accountEnum[accountIndex];
                        acc.AccountType.SetValue(accountType);

                        //response
                        var responseLists = sessionManager.DoRequests(request).ResponseList;
                        var responseCount = responseLists.Count;
                        IResponse accountResponse = responseLists.GetAt(responseCount - 1);
                        IAccountRet accountRet = (IAccountRet)accountResponse.Detail;
                        if (accountRet != null)
                        {
                            x["SyncedBy"] = UserSession.AddedBy;
                            x["ListID"] = accountRet.ListID.GetValue();
                            x["EditSequence"] = accountRet.EditSequence.GetValue();
                            await Accounts.Sync(x.ModelToDataTable());
                            //increment was in SP.  to handle updates and insert at the same time.
                        }
                        else
                        {
                           
                        }
                        
                    }
                    else //update
                    {
                        //wala update. kasi qbd prio. nandun na downloaded na.
                    }
                        await AsyncCloseConnection();//RRIEL moving it here.
                        subIndex += 1;
                }
                
            }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            
        }

        public async Task SyncClasses()
        {
            var data = await Classes.Upload(UserSession);
            await Task.Factory.StartNew(() =>
            {
                data.Columns.Add("SyncedBy");
                data.Columns.Add("IsSuccess");
                data.Columns.Add("ItemID");
            });
            foreach (DataRow x in data.Rows)
            {
                await AsyncOpenConnection();
                if (x["ListID"].Equals("0"))
                {//new
                    var list = request.AppendClassAddRq();
                    list.Name.SetValue(x["Name"].ToString());
                    //response
                    var responseLists = sessionManager.DoRequests(request).ResponseList;
                    var responseCount = responseLists.Count;
                    IResponse classResponse = responseLists.GetAt(responseCount - 1);
                    IClassRet classRet = (IClassRet)classResponse.Detail;
                    if (classRet != null)
                    {
                        x["SyncedBy"] = UserSession.AddedBy;
                        x["ListID"] = classRet.ListID.GetValue();
                        x["EditSequence"] = classRet.EditSequence.GetValue();
                        //await Classes.Sync(x.ModelToDataTable()); RRIEL jeff did not use this. 09/09/2024
                        //RRIEL02042025. comment to Jeff. the difficulties arises when not following a standard structure. be sure to follow the structure next time. there's a reason or architecture why it is there.
                        x["ItemID"] = 2;
                        x["IsSuccess"] = true;
                        await Increment(x.ModelToDataTable());
                    }
                    else
                    {
                        x["ItemID"] = 2;
                        x["IsSuccess"] = false;
                        await Increment(x.ModelToDataTable());

                        //accountResponse.StatusMessage;   
                    }
                    await AsyncCloseConnection();
                }
                else //update
                {
                    //wala update. kasi qbd prio. nandun na downloaded na.
                }
            }
        }

        public async Task SyncProjects()
        {
            var data = await Projects.Upload(UserSession);
            await Task.Factory.StartNew(() => {
                data.Columns.Add("SyncedBy");
                data.Columns.Add("IsSuccess");
                data.Columns.Add("ItemID");
            });
            foreach (DataRow x in data.Rows)
            {
                await AsyncOpenConnection();
                if (x["ListID"].Equals("0"))
                {//new
                    var list = request.AppendCustomerAddRq();
                    list.Name.SetValue(x["Name"].ToString());
                    //response
                    var responseLists = sessionManager.DoRequests(request).ResponseList;
                    var responseCount = responseLists.Count;
                    IResponse customerResponse = responseLists.GetAt(responseCount - 1);
                    ICustomerRet customerRet = (ICustomerRet)customerResponse.Detail;
                    if (customerRet != null)
                    {
                        x["SyncedBy"] = UserSession.AddedBy;
                        x["ListID"] = customerRet.ListID.GetValue();
                        x["EditSequence"] = customerRet.EditSequence.GetValue();
                        await Classes.Sync(x.ModelToDataTable()); //RRIELComment you make the process very random. please follow the flow. this is for Classes. not for projects.
                        //I did look on it. it contains syncing of Classes not projects. 
                    }
                    else
                    {
                        x["ItemID"] = 3;
                        x["IsSuccess"] = false;
                        await Increment(x.ModelToDataTable());

                        //accountResponse.StatusMessage;   
                    }
                    await AsyncCloseConnection();
                }
                else //update
                {
                    //wala update. kasi qbd prio. nandun na downloaded na.
                }
            }
        }

        public async Task SyncVendors()
        {
            try
            {


                //RETORE CODES TO DEFAULT BY REM on 0430 20205. Original Syncing process.
                var dataSet = await Vendors.Upload(UserSession);
                var vendor = dataSet.Tables[0];
                vendor.Columns.Add("ItemID");
                vendor.Columns.Add("IsSuccess");
                vendor.Columns.Add("Key");
                var vendor_info = dataSet.Tables[1];
                var vendor_addr = dataSet.Tables[2];
                vendor.Columns.Add("SyncedBy");
                foreach (DataRow x in vendor.Rows)
                {

                    DataRow info = null;
                    DataRow addr = null;
                    if (vendor_info.Rows.Count > 0)
                        info = vendor_info.Select($"VendorID = {x["VendorID"]} AND CompanyID = {x["CompanyID"]}").FirstOrDefault();
                    if (vendor_addr.Rows.Count > 0)
                        addr = vendor_addr.Select($"VendorID = {x["VendorID"]} AND CompanyID = {x["CompanyID"]}").FirstOrDefault();
                    await AsyncOpenConnection();
                    if (x["ListID"].Equals("0"))
                    {
                        var ven = request.AppendVendorAddRq();
                        ven.Name.SetValue(x["Name"].ToString());
                        ven.IsActive.SetValue(Convert.ToBoolean(x["IsActive"]));

                        ven.CompanyName.SetValue($"{x["CompanyName"]}");
                        await Task.Factory.StartNew(() =>
                        {
                            if (info != null)
                            {
                                if (!info.IsNull("VendorTaxIdent"))
                                {
                                    var numericVal = Convert.ToDecimal(info["VendorTaxIdent"].ToString().Replace("-", "").Substring(0, Math.Min(9, info["VendorTaxIdent"].ToString().Replace("-", "").Length)));
                                    if (numericVal > 0 && numericVal > 0) // to handle 000 value
                                {
                                        var reformat = numericVal.ToString("###-######").PadLeft(10, '0');
                                        ven.VendorTaxIdent.SetValue(reformat);
                                    }

                                }
                                ven.FirstName.SetValue($"{info["FullName"]}");
                                ven.Email.SetValue($"{info["Email"]}");
                                ven.Phone.SetValue($"{info["Phone"]}");
                                ven.Mobile.SetValue($"{info["Mobile"]}");
                                ven.Cc.SetValue($"{info["Cc"]}");
                            //contact
                            ven.Contact.SetValue($"{info["Contact"]}");
                            }
                        });
                        await Task.Factory.StartNew(() =>
                        {
                            if (addr != null)
                            {
                            //Address info
                            IAddress address = ven.VendorAddress;
                                address.Addr1.SetValue($"{addr["Addr1"]}");
                                address.City.SetValue($"{addr["City"]}");
                                address.State.SetValue($"{addr["State"]}");
                                address.PostalCode.SetValue($"{addr["PostalCode"]}");
                                address.Country.SetValue($"{addr["Country"]}");
                            }
                        });

                        var responseLists = await Task.Factory.StartNew(() => sessionManager.DoRequests(request).ResponseList);
                        var responseCount = responseLists.Count;
                        IResponse vendorResponse = responseLists.GetAt(responseCount - 1);
                        IVendorRet vendorRet = (IVendorRet)vendorResponse.Detail;
                        if (vendorRet != null)
                        {
                            x["SyncedBy"] = UserSession.AddedBy;
                            x["ListID"] = vendorRet.ListID.GetValue();
                            x["EditSequence"] = vendorRet.EditSequence.GetValue();
                            x["ItemID"] = 1;
                            x["Key"] = x["VendorID"];
                            x["IsSuccess"] = true;
                            await Increment(x.ModelToDataTable());
                            await Vendors.Sync(x.ModelToDataTable());
                        }
                        else
                        {
                            x["ItemID"] = 1;
                            x["Key"] = x["VendorID"];
                            x["IsSuccess"] = false;
                            await Increment(x.ModelToDataTable());
                        }
                    }
                    else
                    {// meron update dito.
                        var ven = request.AppendVendorModRq();
                        ven.EditSequence.SetValue($"{x["EditSequence"]}");
                        ven.ListID.SetValue($"{x["ListID"]}");
                        ven.Name.SetValue(x["Name"].ToString());
                        ven.IsActive.SetValue(Convert.ToBoolean(x["IsActive"]));
                        if (!info.IsNull("VendorTaxIdent"))
                        {
                            var numericVal = Convert.ToDecimal(info["VendorTaxIdent"].ToString().Replace("-", "").Substring(0, Math.Min(9, info["VendorTaxIdent"].ToString().Replace("-", "").Length)));
                            if (numericVal > 0 && numericVal > 0) // to handle 000 value
                            {
                                var reformat = numericVal.ToString("###-######").PadLeft(10, '0');
                                ven.VendorTaxIdent.SetValue(reformat);
                            }

                        }
                        ven.CompanyName.SetValue($"{x["CompanyName"]}");
                        await Task.Factory.StartNew(() =>
                        {
                            if (info != null)
                            {
                                ven.FirstName.SetValue($"{info["FullName"]}");
                                ven.Email.SetValue($"{info["Email"]}");
                                ven.Phone.SetValue($"{info["Phone"]}");
                                ven.Mobile.SetValue($"{info["Mobile"]}");
                                ven.Cc.SetValue($"{info["Cc"]}");
                            //contact
                            ven.Contact.SetValue($"{info["Contact"]}");
                            }
                        });
                        await Task.Factory.StartNew(() =>
                        {
                            if (addr != null)
                            {
                            //Address info
                            IAddress address = ven.VendorAddress;
                                address.Addr1.SetValue($"{addr["Addr1"]}");
                                address.City.SetValue($"{addr["City"]}");
                                address.State.SetValue($"{addr["State"]}");
                                address.PostalCode.SetValue($"{addr["PostalCode"]}");
                                address.Country.SetValue($"{addr["Country"]}");
                            }
                        });

                        var responseLists = await Task.Factory.StartNew(() => sessionManager.DoRequests(request).ResponseList);
                        var responseCount = responseLists.Count;
                        IResponse vendorResponse = responseLists.GetAt(responseCount - 1);
                        string statusCode;
                        string statusMessage;
                        statusCode = vendorResponse.StatusCode.ToString();
                        statusMessage = vendorResponse.StatusMessage.ToString();
                        IVendorRet vendorRet = (IVendorRet)vendorResponse.Detail;
                        if (vendorRet != null)
                        {
                            x["SyncedBy"] = UserSession.AddedBy;
                            x["ListID"] = vendorRet.ListID.GetValue();
                            x["EditSequence"] = vendorRet.EditSequence.GetValue();
                            x["Key"] = x["VendorID"];
                            x["ItemID"] = 1;
                            x["IsSuccess"] = true;
                            await Increment(x.ModelToDataTable());
                            await Vendors.Sync(x.ModelToDataTable());
                        }
                        else
                        {
                            x["Key"] = x["VendorID"];
                            x["ItemID"] = 1;
                            x["IsSuccess"] = false;
                            await Increment(x.ModelToDataTable());
                        }

                    }
                }
            }
            catch (Exception ex) {
                
            }
        }

        //RRIEL DEPRECATED 04 30 2025. 
        //public async Task SyncedVendorToLogs(string methodname, string resultmessage, bool isSuccess)
        //{
        //    details.Add(new SystemLogDetails { ActionDone = methodname, Value = resultmessage });
        //    logs.ErrorID = null;
        //    logs.Success = isSuccess;
        //    logs.Details = details;
        //    await LogAsync(logs);
        //}

        public async Task EndSync()
        {
            await MyBillsPH.EndSync(UserSession);
        }
        public async Task EndSyncLists()
        {
            await MyBillsPH.EndSyncLists(UserSession);
        }
    }
}
