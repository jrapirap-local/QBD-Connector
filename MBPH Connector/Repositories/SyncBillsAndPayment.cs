using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MBPH_Connector.IRepositories;
using static MBPHReader.SQL.Meta;
using Newtonsoft.Json;
using static MBPH.Extension.Extensions;
using MBPH.CMD;
using MBPH.ECIRA.Model;
using static MBPH_Connector.Repositories.AsyncECIRA;
using static QuickBooksReader.QBD;
using MBPH_Connector.Abstraction;
using QBFC13Lib;
using QuickBooksReader.Library;
using MBPH.Extension;
using MBPHReader;
using System.Data;
using MBPHReader.Model;
using static MBPH_Connector.Abstraction.Session;
using static MBPHReader.MyBillsPH;
using Newtonsoft.Json.Linq;
using System.Security.Policy;
using MBPH.QBDLinq.Model;
namespace MBPH_Connector.Repositories
{
    public class SyncBillsAndPayment : ISyncBillsAndPayment
    {
        public async Task Download()
        {
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
                await Bills.Download(data);
            }
            //throw new NotImplementedException();
        }

        //######################################## MODIFY BY JRAPI06092025
        public async Task SyncBillsPayments()
        {
            try
            {
                await QBD.ReadData();

                // Process Bills
                var bills = await Bills.Upload(UserSession);
                var (billsForUpdate, billsForCreation) = SplitDataTableByTxnID(bills);

                // Process Payments
                var dtPayments = await Payments.GetPayments(UserSession);
                var payments = SortAndPreparePayments(dtPayments);
                var (paymentsForUpdate, paymentsForCreation) = SplitDataTableByTxnID(payments);

                // Handle updates and creations
                await HandlePaymentsForUpdate(paymentsForUpdate);
                await HandleBillsForUpdate(billsForUpdate);
                await HandleBillsForCreation(billsForCreation);

                // Done Syncing Info
                var syncInfo = CreateSyncInfo(Session.UserSession.CompanyID, 5);
                await DoneSyncing(syncInfo);

                await QBD.ReadData();

                // Re-process payments (post-bill sync)
                dtPayments = await Payments.GetPayments(UserSession);
                payments = SortAndPreparePayments(dtPayments);
                (_, paymentsForCreation) = SplitDataTableByTxnID(payments);

                await HandlePaymentsForCreation(paymentsForCreation);
            }
            catch
            {
                // Swallowing exception as per original intent
            }
            finally
            {
                await AsyncCloseConnection();
            }
        }

        private DataTable SortAndPreparePayments(DataTable payments)
        {
            if (payments == null || payments.Rows.Count == 0)
                return new DataTable();

            var sorted = payments.AsEnumerable()
                .OrderBy(row => row["TxnID"] == DBNull.Value)
                .CopyToDataTable();

            if (!sorted.Columns.Contains("processed"))
                sorted.Columns.Add("processed");

            return sorted;
        }

        private DataTable CreateSyncInfo(string companyId, int itemId)
        {
            var data = new DataTable();
            data.Columns.Add("CompanyID");
            data.Columns.Add("ItemID");

            var row = data.NewRow();
            row["CompanyID"] = companyId;
            row["ItemID"] = itemId;

            data.Rows.Add(row);
            return data;
        }

        private (DataTable update, DataTable create) SplitDataTableByTxnID(DataTable data)
        {
            var update = data.Clone();
            var create = data.Clone();

            foreach (DataRow row in data.Rows)
            {
                if (!row.IsNull("TxnID"))
                    update.ImportRow(row);
                else
                    create.ImportRow(row);
            }

            return (update, create);
        }

        private async Task HandlePaymentsForUpdate(DataTable paymentsForUpdate)
        {
            try
            {
                foreach (DataRow row in paymentsForUpdate.Rows)
                {
                    var existing = QBD.BillsPaymentChecks.Find(f => f.TxnID == row["TxnID"].ToString());
                    if (existing != null)
                        await SyncPaymentUpdates(row, paymentsForUpdate);
                }
            }
            catch (Exception ex)
            {
                await LogSyncError("Payments", "No Payment Voucher ID", 6, ex.Message);
            }
        }

        private async Task HandleBillsForUpdate(DataTable billsForUpdate)
        {
            try
            {
                foreach (DataRow row in billsForUpdate.Rows)
                {
                    var param = new Parameters
                    {
                        CompanyID = Convert.ToInt32(row["CompanyID"]),
                        MWID = Convert.ToInt32(row["MWID"])
                    };

                    var details = await Bills.UploadDetails(param);
                    var hasAccount = details.AsEnumerable().All(d => QBD.Accounts.Any(a => a.ListID == d["COAListID"].ToString()));

                    if (hasAccount && details.Rows.Count > 0)
                        await SyncBillUpdates(row);
                }
            }
            catch (Exception ex)
            {
                await LogSyncError("Bills", "No Bill Transaction ID", 5, ex.Message);
            }
        }

        private async Task HandleBillsForCreation(DataTable billsForCreation)
        {
            try
            {
                foreach (DataRow row in billsForCreation.Rows)
                {
                    var param = new Parameters
                    {
                        CompanyID = Convert.ToInt32(row["CompanyID"]),
                        MWID = Convert.ToInt32(row["MWID"])
                    };

                    var details = await Bills.UploadDetails(param);
                    var hasAccount = details.AsEnumerable().All(d => QBD.Accounts.Any(a => a.ListID == d["COAListID"].ToString()));

                    if (hasAccount && details.Rows.Count > 0)
                        await SyncBillInsert(row);
                }
            }
            catch (Exception ex)
            {
                await LogSyncError("Bills", "No Bill Transaction ID", 5, ex.Message);
            }
        }

        private async Task HandlePaymentsForCreation(DataTable paymentsForCreation)
        {
            try
            {
                foreach (DataRow row in paymentsForCreation.Rows)
                {
                    //var existing = QBD.BillsPaymentChecks.Find(f => f.TxnID == row["TxnID"].ToString());
                    //if (existing != null)
                        await SyncPaymentInserts(row, paymentsForCreation);
                }
            }
            catch (Exception ex)
            {
                await LogSyncError("Payments", "No Payment Voucher ID", 6, ex.Message);
            }
        }

        private async Task LogSyncError(string listType, string transactionID, int itemID, string message)
        {
            var syncData = UserSession.ModelToDataTable();
            await Task.Factory.StartNew(() =>
            {
                syncData.Columns.Add("TransactionID");
                syncData.Columns.Add("ItemID");
                syncData.Columns.Add("IsSuccess");
                syncData.Columns.Add("ListType");
                syncData.Columns.Add("SyncStatus");
                syncData.Columns.Add("Remarks");
                syncData.Columns.Add("Key");

                var row = syncData.Rows[0];
                row["TransactionID"] = transactionID;
                row["ItemID"] = itemID;
                row["IsSuccess"] = false;
                row["ListType"] = listType;
                row["SyncStatus"] = "UNSUCCESSFUL";
                row["Remarks"] = message;
                row["Key"] = string.Empty;
            });
            await Increment(syncData);
            await LogError(syncData);
        }
        //########################################

        public async Task SyncPaymentInserts(DataRow x, DataTable payments)
        {
            try
            {
                if (Convert.ToString(x["processed"]) != "1")
                {
                    var accnt = QBD.Accounts.Find(f => f.ListID == x["BankAccountListID"].ToString());
                    if (accnt != null)
                    {
                        await AsyncOpenConnection();
                        var Payment = request.AppendBillPaymentCheckAddRq();
                        Payment.PayeeEntityRef.FullName.SetValue(x["QBDVendorLegalName"].ToString());
                        Payment.PayeeEntityRef.ListID.SetValue(x["VendorListID"].ToString());
                        Payment.TxnDate.SetValue(DateTime.Parse(x["PaymentDate"].ToString()));
                        Payment.BankAccountRef.FullName.SetValue(x["PaymentSource"].ToString());
                        Payment.BankAccountRef.ListID.SetValue(x["BankAccountListID"].ToString());


                        Payment.APAccountRef.FullName.SetValue(x["AccountName"].ToString()); //AccountName 
                        Payment.APAccountRef.ListID.SetValue(x["APAccountListID"].ToString()); //COAListID / Accounts Payable
                        if (x["PaymentRefNo"].Equals("Not Applicable"))
                        {
                            Payment.ORCheckPrint.RefNumber.SetValue("N/A");
                        }
                        else
                        {
                            if (x["PaymentRefNo"].ToString().Length > 11)
                            {
                                Payment.ORCheckPrint.RefNumber.SetValue(x["PaymentRefNo"].ToString().Substring(0, 11).ToUpper());//limit only to 11 characters
                            }
                            else
                            {
                                Payment.ORCheckPrint.RefNumber.SetValue(x["PaymentRefNo"].ToString().ToUpper());
                            }

                        }

                        //DataRow[] selectedRows = payments.Select($"PaymentID = {x["PaymentID"]}");
                        var id = x["PaymentID"];
                        var selectedRows = payments.AsEnumerable()
                                                   .Where(row => row["PaymentID"].Equals(id))
                                                   .ToArray();

                        List<string> listOfTransactions = new List<string>();
                        foreach (DataRow row in selectedRows)
                        {
                            //table.ImportRow(row);
                            await Task.Factory.StartNew(() =>
                            {
                                var PaymentLine = Payment.AppliedToTxnAddList.Append();
                                PaymentLine.PaymentAmount.SetValue(Convert.ToDouble(row["Amount2"].ToString()));
                                PaymentLine.TxnID.SetValue(row["BillLinkID"].ToString());
                                listOfTransactions.Add(Convert.ToString(row["TransactionID"]));
                                row["processed"] = 1; // tagging for per PV

                            });
                        }

                        if (!x.IsNull("VoidingReason"))
                        {
                            var voidingReason = string.Empty;
                            if (string.IsNullOrEmpty(x["VoidingReason"].ToString()))
                            {
                                x["VoidingReason"] = "N/A";
                            }
                            voidingReason = $" | Voiding Reason : {x["VoidingReason"]}";
                            //Payment.Memo.SetValue($"{selectedPaymentDetails[0]["VoidingReason"].ToString()}");
                            Payment.Memo.SetValue($"PV #: {x["PaymentVoucherNo"]} | Transaction(s): {string.Join(",", listOfTransactions)} {voidingReason}");
                        }
                        else
                        {
                            Payment.Memo.SetValue($"PV #: {x["PaymentVoucherNo"]} | Transaction(s): {string.Join(",", listOfTransactions)}");
                        }

                        var responseLists = await Task.Factory.StartNew(() => sessionManager.DoRequests(request).ResponseList);
                        var responseCount = responseLists.Count;

                        IResponse paymentResponse = responseLists.GetAt(responseCount - 1);
                        var test = paymentResponse.StatusMessage.ToString(); //for debugging only jrapi

                        IBillPaymentCheckRet paymentRet = (IBillPaymentCheckRet)paymentResponse.Detail;
                        if (paymentRet != null)
                        {
                            var syncData = UserSession.ModelToDataTable();
                            await Task.Factory.StartNew(() =>
                            {
                                syncData.Columns.Add("TransactionID");
                                syncData.Columns.Add("TxnID");
                                syncData.Columns.Add("PaymentID");
                                syncData.Columns.Add("BillLinkID");
                                syncData.Columns.Add("EditSequence");
                                syncData.Columns.Add("IsVoid");
                                syncData.Columns.Add("IsSuccess");
                            });
                            await Task.Factory.StartNew(() =>
                            {
                                syncData.Rows[0]["TransactionID"] = x["TransactionID"].ToString();
                                syncData.Rows[0]["TxnID"] = paymentRet.TxnID.GetValue();
                                syncData.Rows[0]["PaymentID"] = x["PaymentID"].ToString();
                                syncData.Rows[0]["BillLinkID"] = x["BillLinkID"];
                                syncData.Rows[0]["EditSequence"] = paymentRet.EditSequence.GetValue();
                                syncData.Rows[0]["IsVoid"] = false;
                            });

                            await Payments.Sync(syncData);
                            if (x["Voided"].Equals(true) && x["Active"].Equals(true))
                            {
                                await SyncAsVoid(x, paymentRet.TxnID.GetValue(), paymentRet.EditSequence.GetValue(), x["PaymentVoucherNo"].ToString()); //added params if payments is to be voided, for logging purposes only JRAPI05142025

                            }
                            await Task.Factory.StartNew(() => {
                                syncData.Columns.Add("ItemID");
                                syncData.Columns.Add("Key");
                                syncData.Rows[0]["Key"] = x["PaymentVoucherNo"].ToString(); //ADDED BY JRAPI040725
                                syncData.Rows[0]["ItemID"] = 6; //ITEM ID = 6 for payments
                                syncData.Rows[0]["IsSuccess"] = true;
                            });

                            await Increment(syncData);
                        }
                        else
                        {
                            var syncData = UserSession.ModelToDataTable();
                            await Task.Factory.StartNew(() =>
                            {
                                syncData.Columns.Add("TransactionID");
                                syncData.Columns.Add("ItemID");
                                syncData.Columns.Add("IsSuccess");
                                syncData.Columns.Add("ListType");
                                syncData.Columns.Add("SyncStatus");
                                syncData.Columns.Add("Remarks");
                                syncData.Columns.Add("Key");
                                syncData.Rows[0]["Key"] = x["PaymentVoucherNo"].ToString();
                                syncData.Rows[0]["TransactionID"] = x["PaymentVoucherNo"].ToString(); //ADDED BY JRAPI040725
                                syncData.Rows[0]["ListType"] = "Payments";
                                syncData.Rows[0]["SyncStatus"] = "UNSUCCESSFUL";
                                syncData.Rows[0]["Remarks"] = paymentResponse.StatusMessage;
                                syncData.Rows[0]["ItemID"] = 6;
                                syncData.Rows[0]["IsSuccess"] = false;

                            });
                            await Increment(syncData);
                            await LogError(syncData);
                        }
                    }
                    else
                    {
                        var syncData = UserSession.ModelToDataTable();
                        await Task.Factory.StartNew(() =>
                        {
                            syncData.Columns.Add("TransactionID");
                            syncData.Columns.Add("ItemID");
                            syncData.Columns.Add("IsSuccess");
                            syncData.Columns.Add("ListType");
                            syncData.Columns.Add("Remarks");
                            syncData.Columns.Add("PaymentErrType");
                            syncData.Columns.Add("Key");
                            syncData.Rows[0]["Key"] = x["PaymentVoucherNo"].ToString();
                            syncData.Rows[0]["PaymentErrType"] = 1;
                            syncData.Rows[0]["TransactionID"] = x["PaymentVoucherNo"].ToString();
                            syncData.Rows[0]["ListType"] = "Payments";
                            syncData.Rows[0]["ItemID"] = 6;
                            syncData.Rows[0]["IsSuccess"] = false;

                        });
                        await LogCustomError(syncData);

                    }
                }
                await AsyncCloseConnection(); //close connection
            }
            catch (Exception ex)
            {
                var syncData = UserSession.ModelToDataTable();

                await Task.Factory.StartNew(() =>
                {
                    syncData.Columns.Add("TransactionID");
                    syncData.Columns.Add("ItemID");
                    syncData.Columns.Add("IsSuccess");
                    syncData.Columns.Add("ListType");
                    syncData.Columns.Add("SyncStatus");
                    syncData.Columns.Add("Remarks");
                    syncData.Columns.Add("Key");

                    //y["TransactionID"].ToString()
                    syncData.Rows[0]["Key"] = string.Empty;
                    syncData.Rows[0]["TransactionID"] = "No Payment Voucher ID";
                    syncData.Rows[0]["ListType"] = "Payments";
                    syncData.Rows[0]["SyncStatus"] = "UNSUCCESSFUL";
                    syncData.Rows[0]["Remarks"] = ex.Message;

                    syncData.Rows[0]["ItemID"] = 6;
                    syncData.Rows[0]["IsSuccess"] = false;

                });
                await Increment(syncData);
                await LogError(syncData);
            }
            finally
            {
                await AsyncCloseConnection();
            }

        }

        public async Task SyncPaymentUpdates(DataRow x, DataTable payments)
        {
            try
            {
                //READ ALL DATA FROM QBD
                await QBD.ReadData();

                if (x["Voided"].Equals(true) && x["Active"].Equals(true))
                {
                    await VoidPayment(x, payments);
                }
                else //if not for voiding, initiate updating of values for payments JRAPI05142025
                {
                    await AsyncOpenConnection();
                    var Payment = request.AppendBillPaymentCheckModRq();
                    var qbPayments = QBD.BillsPaymentChecks.Find(f => f.TxnID == x["TxnID"].ToString()); //to refetch the current Editsequence it will cause bug if missed match
                    Payment.EditSequence.SetValue(qbPayments.EditSequence);
                    Payment.TxnID.SetValue(qbPayments.TxnID);
                    Payment.TxnDate.SetValue(DateTime.Parse(x["PaymentDate"].ToString()));
                    Payment.BankAccountRef.FullName.SetValue(x["PaymentSource"].ToString());
                    Payment.BankAccountRef.ListID.SetValue(x["BankAccountListID"].ToString());
                    
                    if (x["PaymentRefNo"].Equals("Not Applicable"))
                    {
                        Payment.ORCheckPrint.RefNumber.SetValue("N/A");
                    }
                    else
                    {
                        if (x["PaymentRefNo"].ToString().Length > 11)
                        {
                            Payment.ORCheckPrint.RefNumber.SetValue(x["PaymentRefNo"].ToString().Substring(0, 11).ToUpper());//limit only to 11 characters
                        }
                        else
                        {
                            Payment.ORCheckPrint.RefNumber.SetValue(x["PaymentRefNo"].ToString().ToUpper());
                        }

                    }

                    DataRow[] selectedRows = payments.Select($"PaymentID = {x["PaymentID"]}");
                    List<string> listOfTransactions = new List<string>();
                    foreach (DataRow row in selectedRows)
                    {
                        await Task.Factory.StartNew(() =>
                        {
                            var PaymentLine = Payment.AppliedToTxnModList.Append();
                            PaymentLine.PaymentAmount.SetValue(Convert.ToDouble(row["Amount2"].ToString()));
                            PaymentLine.TxnID.SetValue(row["BillLinkID"].ToString());
                            listOfTransactions.Add(Convert.ToString(row["TransactionID"]));
                            row["processed"] = 1;
                        });
                    }

                    if (!x.IsNull("VoidingReason"))
                    {
                        var voidingReason = string.Empty;
                        if (string.IsNullOrEmpty(x["VoidingReason"].ToString()))
                        {
                            x["VoidingReason"] = "N/A";
                        }
                        voidingReason = $" | Voiding Reason : {x["VoidingReason"]}";
                        Payment.Memo.SetValue($"PV #: {x["PaymentVoucherNo"]} | Transaction(s): {string.Join(",", listOfTransactions)} {voidingReason}");
                    }
                    else
                    {
                        Payment.Memo.SetValue($"PV #: {x["PaymentVoucherNo"]} | Transaction(s): {string.Join(",", listOfTransactions)}");
                    }

                    var responseLists = await Task.Factory.StartNew(() => sessionManager.DoRequests(request).ResponseList);
                    var responseCount = responseLists.Count;
                    IResponse paymentResponse = responseLists.GetAt(responseCount - 1);

                    IBillPaymentCheckRet paymentRet = (IBillPaymentCheckRet)paymentResponse.Detail;
                    if (paymentRet != null)
                    {
                        DataRow[] selectedRowSync = payments.Select("PaymentID = " + x["PaymentID"]);
                        foreach (DataRow y in selectedRowSync)
                        {

                            var syncData = UserSession.ModelToDataTable();
                            await Task.Factory.StartNew(() =>
                            {
                                syncData.Columns.Add("TransactionID");
                                syncData.Columns.Add("TxnID");
                                syncData.Columns.Add("PaymentID");
                                syncData.Columns.Add("BillLinkID");
                                syncData.Columns.Add("EditSequence");
                                syncData.Columns.Add("IsVoid");
                                syncData.Columns.Add("IsSuccess");
                            });
                            await Task.Factory.StartNew(() =>
                            {
                                syncData.Rows[0]["TransactionID"] = y["TransactionID"].ToString();
                                syncData.Rows[0]["TxnID"] = paymentRet.TxnID.GetValue();
                                syncData.Rows[0]["PaymentID"] = y["PaymentID"].ToString();
                                syncData.Rows[0]["BillLinkID"] = y["BillLinkID"];
                                syncData.Rows[0]["EditSequence"] = paymentRet.EditSequence.GetValue();
                                syncData.Rows[0]["IsVoid"] = false;

                                
                            });
                            await Payments.Sync(syncData);
                            if (x["Voided"].Equals(true) && x["Active"].Equals(true))
                            {
                                await SyncAsVoid(y, paymentRet.TxnID.GetValue(), paymentRet.EditSequence.GetValue(), x["PaymentVoucherNo"].ToString()); //added params if payments is to be voided, for logging purposes only JRAPI05142025

                            }

                            await Task.Factory.StartNew(() => {
                                syncData.Columns.Add("ItemID");
                                syncData.Columns.Add("Key");
                                syncData.Rows[0]["Key"] = x["PaymentVoucherNo"].ToString(); //ADDED BY JRAPI040725
                                syncData.Rows[0]["ItemID"] = 6; //ITEM ID = 5 for payments
                                syncData.Rows[0]["IsSuccess"] = true;
                            });

                            await Increment(syncData);
                        }
                    }
                    else
                    {
                        var syncData = UserSession.ModelToDataTable();
                        await Task.Factory.StartNew(() =>
                        {
                            syncData.Columns.Add("TransactionID");
                            syncData.Columns.Add("ItemID");
                            syncData.Columns.Add("IsSuccess");
                            syncData.Columns.Add("ListType");
                            syncData.Columns.Add("SyncStatus");
                            syncData.Columns.Add("Remarks");
                            syncData.Columns.Add("Key");
                            syncData.Rows[0]["Key"] = x["PaymentVoucherNo"].ToString();
                            syncData.Rows[0]["TransactionID"] = x["PaymentVoucherNo"].ToString(); //ADDED BY JRAPI040725
                            syncData.Rows[0]["ListType"] = "Payments";
                            syncData.Rows[0]["SyncStatus"] = "UNSUCCESSFUL";
                            syncData.Rows[0]["Remarks"] = paymentResponse.StatusMessage;
                            syncData.Rows[0]["ItemID"] = 6;
                            syncData.Rows[0]["IsSuccess"] = false;

                        });
                        await Increment(syncData);
                        await LogError(syncData);
                    }
                }
            }
            catch (Exception ex)
            {
                var syncData = UserSession.ModelToDataTable();

                await Task.Factory.StartNew(() =>
                {
                    syncData.Columns.Add("TransactionID");
                    syncData.Columns.Add("ItemID");
                    syncData.Columns.Add("IsSuccess");
                    syncData.Columns.Add("ListType");
                    syncData.Columns.Add("SyncStatus");
                    syncData.Columns.Add("Remarks");
                    syncData.Columns.Add("Key");

                    //y["TransactionID"].ToString()
                    syncData.Rows[0]["Key"] = string.Empty;
                    syncData.Rows[0]["TransactionID"] = "No Payment Voucher ID";
                    syncData.Rows[0]["ListType"] = "Payments";
                    syncData.Rows[0]["SyncStatus"] = "UNSUCCESSFUL";
                    syncData.Rows[0]["Remarks"] = ex.Message;

                    syncData.Rows[0]["ItemID"] = 6;
                    syncData.Rows[0]["IsSuccess"] = false;

                });
                await Increment(syncData);
                await LogError(syncData);
            }
            finally
            {
                await AsyncCloseConnection();
            }

        }

        public async Task SyncAsVoid(DataRow y, String t, String e, string PaymentVoucherNo)  //added params if payments is to be voided, for logging purposes only JRAPI05142025
        {
            await AsyncCloseConnection();
            await AsyncOpenConnection();
            var voidPayments = request.AppendTxnVoidRq();
            voidPayments.TxnVoidType.SetValue(ENTxnVoidType.tvtBillPaymentCheck);
            voidPayments.TxnID.SetValue(t);

            await Task.Factory.StartNew(() => sessionManager.DoRequests(request).ResponseList);
            var syncData = UserSession.ModelToDataTable();
            await Task.Factory.StartNew(() =>
            {
                syncData.Columns.Add("TransactionID");
                syncData.Columns.Add("TxnID");
                syncData.Columns.Add("PaymentID");
                syncData.Columns.Add("BillLinkID");
                syncData.Columns.Add("EditSequence");
                syncData.Columns.Add("IsVoid");
                syncData.Columns.Add("IsSuccess");
                syncData.Columns.Add("Key");
                syncData.Columns.Add("ItemID");
            });
            await Task.Factory.StartNew(() =>
            {
                syncData.Rows[0]["TransactionID"] = y["TransactionID"].ToString();
                syncData.Rows[0]["TxnID"] = t;
                syncData.Rows[0]["PaymentID"] = y["PaymentID"].ToString();
                syncData.Rows[0]["BillLinkID"] = y["BillLinkID"].ToString();
                syncData.Rows[0]["EditSequence"] = e;
                syncData.Rows[0]["IsVoid"] = true;
                syncData.Rows[0]["IsSuccess"] = true;
                syncData.Rows[0]["Key"] = PaymentVoucherNo;
                syncData.Rows[0]["ItemID"] = 6;
                syncData.Rows[0]["IsSuccess"] = false;
            });
            await Payments.Sync(syncData);
            await Increment(syncData); //added increment to detect value change when voiding JRAPI05152025
            await AsyncCloseConnection();
        }

        public async Task VoidPayment(DataRow x, DataTable paymentDetails)
        {
            await AsyncCloseConnection();
            await AsyncOpenConnection();
            var Payment = request.AppendBillPaymentCheckModRq();

            var qbPayments = QBD.BillsPaymentChecks.Find(f => f.TxnID == x["TxnID"].ToString());

            DataRow[] selectedRows = paymentDetails.Select($"PaymentID = {x["PaymentID"]}");
            List<string> listOfTransactions = new List<string>();
            foreach (DataRow row in selectedRows)
            {
                listOfTransactions.Add(Convert.ToString(row["TransactionID"]));
            }

            Payment.EditSequence.SetValue(qbPayments.EditSequence);
            Payment.TxnID.SetValue(qbPayments.TxnID);
            var voidingReason = string.Empty;
            if (!x.IsNull("VoidingReason"))
            {
                if (string.IsNullOrEmpty(x["VoidingReason"].ToString()))
                {
                    x["VoidingReason"] = "N/A";
                }
                voidingReason = $" | Voiding Reason : {x["VoidingReason"]}";
                //Payment.Memo.SetValue($"{qbPayments.Memo} {voidingReason}");
                Payment.Memo.SetValue($"PV #: {x["PaymentVoucherNo"]} | Transaction(s): {string.Join(",", listOfTransactions)} {voidingReason}");
            }

            var responseLists = await Task.Factory.StartNew(() => sessionManager.DoRequests(request).ResponseList);
            var responseCount = responseLists.Count;
            IResponse paymentResponse = responseLists.GetAt(responseCount - 1);

            IBillPaymentCheckRet paymentRet = (IBillPaymentCheckRet)paymentResponse.Detail;
            await AsyncCloseConnection();
            if (paymentRet != null)
            {
                var syncData = UserSession.ModelToDataTable();
                await AsyncOpenConnection();
                var voidPayments = request.AppendTxnVoidRq();
                voidPayments.TxnVoidType.SetValue(ENTxnVoidType.tvtBillPaymentCheck);
                voidPayments.TxnID.SetValue(Convert.ToString(qbPayments.TxnID));
                await Task.Factory.StartNew(() => sessionManager.DoRequests(request).ResponseList);
                await AsyncCloseConnection();

                await QBD.ReadData();

                var qbVoidedPayments = QBD.BillsPaymentChecks.Find(f => f.TxnID == x["TxnID"].ToString());
                await Task.Factory.StartNew(() =>
                {
                    syncData.Columns.Add("TransactionID");
                    syncData.Columns.Add("TxnID");
                    syncData.Columns.Add("PaymentID");
                    syncData.Columns.Add("BillLinkID");
                    syncData.Columns.Add("EditSequence");
                    syncData.Columns.Add("IsVoid");
                });
                await Task.Factory.StartNew(() =>
                {
                    syncData.Rows[0]["TransactionID"] = x["TransactionID"].ToString();
                    syncData.Rows[0]["TxnID"] = qbVoidedPayments.TxnID;
                    syncData.Rows[0]["PaymentID"] = x["PaymentID"].ToString();
                    syncData.Rows[0]["BillLinkID"] = x["BillLinkID"];
                    syncData.Rows[0]["EditSequence"] = qbVoidedPayments.EditSequence;
                    syncData.Rows[0]["IsVoid"] = true;
                });
                await Payments.Sync(syncData);
                await Task.Factory.StartNew(() =>
                {
                    syncData.Columns.Add("ItemID");
                    syncData.Columns.Add("IsSuccess");
                    syncData.Columns.Add("Key");
                    syncData.Rows[0]["Key"] = x["PaymentID"].ToString();
                    syncData.Rows[0]["ItemID"] = 6;
                    syncData.Rows[0]["IsSuccess"] = true;
                });
                await Increment(syncData);
            }
            else
            
            {
               
                var syncData = UserSession.ModelToDataTable();

                await Task.Factory.StartNew(() =>
                {
                    syncData.Columns.Add("TransactionID");
                    syncData.Columns.Add("ItemID");
                    syncData.Columns.Add("IsSuccess");
                    syncData.Columns.Add("ListType");
                    syncData.Columns.Add("SyncStatus");
                    syncData.Columns.Add("Remarks");
                    syncData.Columns.Add("Key");
                    syncData.Rows[0]["Key"] = x["TransactionID"].ToString();
                    //y["TransactionID"].ToString()
                    syncData.Rows[0]["TransactionID"] = x["TransactionID"].ToString();
                    syncData.Rows[0]["ListType"] = "Payments";
                    syncData.Rows[0]["SyncStatus"] = "UNSUCCESSFUL";
                    syncData.Rows[0]["Remarks"] = paymentResponse.StatusMessage;

                    syncData.Rows[0]["ItemID"] = 6;
                    syncData.Rows[0]["IsSuccess"] = false;

                });
                await Increment(syncData);
                await LogError(syncData);

            }

        }


        public async Task SyncDeletedBills() {
            var data = await Bills.UploadDeletedBills(UserSession);
            foreach(DataRow x in data.Rows)
            {
                try
                {
                    await AsyncOpenConnection();
                    ITxnDel txnDel = request.AppendTxnDelRq();
                    txnDel.TxnID.SetValue(x["TxnID"].ToString());
                    txnDel.TxnDelType.SetValue(ENTxnDelType.tdtBill);
                    var responseLists = sessionManager.DoRequests(request).ResponseList;
                    var responseCount = responseLists.Count;
                    var response = responseLists.GetAt(responseCount - 1);
                    if(response.Detail!= null)
                    {
                        var param = UserSession.ModelToDataTable();
                        await Task.Factory.StartNew(() =>
                        {
                            param.Columns.Add("BillLInkID");
                        });
                        await Task.Factory.StartNew(() =>
                        {
                            param.Rows[0]["BillLInkID"] = x["TxnID"].ToString();
                        });
                        await Bills.SyncDeleted(param);

                        await Task.Factory.StartNew(() =>
                        {
                            param.Columns.Add("ItemID");
                            param.Columns.Add("IsSuccess");
                            param.Columns.Add("Key");
                            param.Rows[0]["ItemID"] = 5;
                            param.Rows[0]["IsSuccess"] = true;
                            param.Rows[0]["Key"] = x["TxnID"].ToString();
                        });
                        await Increment(param);
                    }

                }
                catch 
                {
                    var syncData = UserSession.ModelToDataTable();
                    await Task.Factory.StartNew(() =>
                    {
                        syncData.Columns.Add("ItemID");
                        syncData.Columns.Add("IsSuccess");
                        syncData.Columns.Add("Key");
                        syncData.Rows[0]["Key"] = string.Empty;
                        syncData.Rows[0]["ItemID"] = 5;
                        syncData.Rows[0]["IsSuccess"] = false;
                    });
                    await Increment(syncData);
                }
                finally
                {

                }
                

            }

        }

        public async Task SyncBills()
        {
            await QBD.ReadData();
            DataTable accounts = new DataTable();
            accounts.Columns.Add("FullName");
            accounts.PrimaryKey = new DataColumn[] { accounts.Columns["FullName"] };
            foreach (var x in QBD.Accounts)// get account list to check if account used in bill exist in qbd else log in ecira JRAPI03252025
            {
                accounts.Rows.Add(x.FullName);
            }
            var data = await Bills.Upload(UserSession);

            foreach (DataRow x in data.Rows)
            {
                try
                {
                    Parameters tmpparam = new Parameters()
                    {
                        CompanyID = Convert.ToInt32(x["CompanyID"]),
                        MWID = Convert.ToInt32(x["MWID"])
                    };
                    var tmptxn = await Bills.UploadDetails(tmpparam);
                    var hasAccount = 1; //default value is 1, if all account was dound will stay 1 id 1 was not found will be changed to 0
                    foreach (DataRow tmpdetails in tmptxn.Rows)
                    {
                        var accnt = QBD.Accounts.Find(f => f.ListID == tmpdetails["COAListID"].ToString());
                        if (accnt == null)
                        {
                            hasAccount = 0; //set to 0 if an accnt was not found
                        }
                    }
                    
                    if (hasAccount == 1 && tmptxn.Rows.Count >0) //RRIEL tmptxn.Rows.Count >0 added this. not ideal. pero eto yung current na logic. hard code. for faster dev time
                    {
                        if (x.IsNull("TxnID"))
                        { //RRIEL condition if new
                            await AsyncOpenConnection();// prompt
                            var bill = request.AppendBillAddRq();
                            bill.VendorRef.ListID.SetValue(x["VendorListID"].ToString());
                            bill.VendorRef.FullName.SetValue(x["VendorName"].ToString());
                            bill.APAccountRef.ListID.SetValue(x["AccountListsID"].ToString());
                            bill.APAccountRef.FullName.SetValue(x["AccountName"].ToString());
                            if (!string.IsNullOrEmpty(x["RefNumber"].ToString()))
                                bill.RefNumber.SetValue(x["RefNumber"].ToString());
                            bill.TxnDate.SetValue(Convert.ToDateTime(x["TxnDate"]));
                            bill.Memo.SetValue(x["Memo"].ToString());
                            if (!x.IsNull("DueDate"))
                            {
                                bill.DueDate.SetValue(Convert.ToDateTime(x["DueDate"]));
                            }
                            else
                            {
                                bill.DueDate.SetValue(Convert.ToDateTime(x["TxnDate"])); // if wala due date. same sa Invoice date nalang.
                            }

                            //session.MWID = x.MWID;
                            Parameters param = new Parameters()
                            {
                                CompanyID = Convert.ToInt32(x["CompanyID"]),
                                MWID = Convert.ToInt32(x["MWID"])
                            };
                            var txn = await Bills.UploadDetails(param);
                            foreach (DataRow details in txn.Rows)
                            {
                                var ExpenseLineDetails = bill.ExpenseLineAddList.Append();
                                ExpenseLineDetails.AccountRef.ListID.SetValue(details["COAListID"].ToString());
                                ExpenseLineDetails.AccountRef.FullName.SetValue(details["AccountName"].ToString());
                                ExpenseLineDetails.Amount.SetValue(Convert.ToDouble(details["Amt"]));
                                if (!string.IsNullOrEmpty(details["Memo"].ToString()))
                                    ExpenseLineDetails.Memo.SetValue(details["Memo"].ToString());
                                if (!details.IsNull("ListID") && QBD.Classes.Count() > 0)
                                {
                                    IMsgSetRequest reqd = sessionManager.CreateMsgSetRequest("US", 13, 0);
                                    IClassQuery ClassQueryRq = reqd.AppendClassQueryRq();
                                    ClassQueryRq.ORListQuery.FullNameList.Add(details["Department"].ToString());
                                    var responseRequestMsgSet = sessionManager.DoRequests(reqd).ResponseList;
                                    var responseCountRequestMsgSet = responseRequestMsgSet.Count;
                                    IResponse classResponse = responseRequestMsgSet.GetAt(responseCountRequestMsgSet - 1);
                                    IClassRetList classRet = (IClassRetList)classResponse.Detail;
                                    if (classRet != null)
                                    {
                                        var cls = QBD.Classes.Find(f => f.ListID == details["ListID"].ToString()); //checks if current class exist in QBD JRAPI05152025
                                        if (cls != null)
                                        {
                                            ExpenseLineDetails.ClassRef.FullName.SetValue(details["Department"].ToString());
                                            ExpenseLineDetails.ClassRef.ListID.SetValue(details["ListID"].ToString());
                                        }
                                    }
                                }
                                if (!details.IsNull("ProjListID") && QBD.Projects.Count() > 0)
                                {
                                    IMsgSetRequest reqp = sessionManager.CreateMsgSetRequest("US", 13, 0);
                                    ICustomerQuery CustomerQueryRq = reqp.AppendCustomerQueryRq();
                                    CustomerQueryRq.ORCustomerListQuery.FullNameList.Add(details["ProjectDescription"].ToString());
                                    var responseRequestMsgSet = sessionManager.DoRequests(reqp).ResponseList;
                                    var responseCountRequestMsgSet = responseRequestMsgSet.Count;
                                    IResponse customerResponse = responseRequestMsgSet.GetAt(responseCountRequestMsgSet - 1);
                                    ICustomerRetList customerRet = (ICustomerRetList)customerResponse.Detail;
                                    if (customerRet != null)
                                    {
                                        var prj = QBD.Projects.Find(f => f.ListID == details["ProjListID"].ToString()); //checks if current project exist in QBD JRAPI05152025
                                        if (prj != null)
                                        {
                                            ExpenseLineDetails.CustomerRef.FullName.SetValue(details["ProjectDescription"].ToString());
                                            ExpenseLineDetails.CustomerRef.ListID.SetValue(details["ProjListID"].ToString());
                                        }
                                    }
                                }

                            }
                            var responseLists = await Task.Factory.StartNew(() => sessionManager.DoRequests(request).ResponseList);
                            var responseCount = responseLists.Count;
                            IResponse billResponse = responseLists.GetAt(responseCount - 1);
                            IBillRet billRet = (IBillRet)billResponse.Detail;
                            if (billRet != null)
                            {//RRIEL sync here
                             //BILL Link ID billRet.TxnID.GetValue();
                                var syncData = UserSession.ModelToDataTable();
                                await Task.Factory.StartNew(() =>
                                {
                                    syncData.Columns.Add("TransactionID");
                                    syncData.Columns.Add("BillLInkID");
                                    syncData.Columns.Add("EditSequence");
                                });
                                await Task.Factory.StartNew(() =>
                                {
                                    syncData.Rows[0]["TransactionID"] = x["TransactionID"].ToString();
                                    syncData.Rows[0]["BillLInkID"] = billRet.TxnID.GetValue();
                                    syncData.Rows[0]["EditSequence"] = billRet.EditSequence.GetValue();
                                });
                                await Bills.Sync(syncData);

                                await Task.Factory.StartNew(() =>
                                {
                                    syncData.Columns.Add("ItemID");
                                    syncData.Columns.Add("IsSuccess");
                                    syncData.Columns.Add("Key");
                                    syncData.Rows[0]["Key"] = string.Empty;
                                    syncData.Rows[0]["ItemID"] = 5;
                                    syncData.Rows[0]["IsSuccess"] = true;
                                    syncData.Rows[0]["Key"] = x["TransactionID"].ToString();
                                });
                                await Increment(syncData);

                            }
                            else
                            {
                                var syncData = UserSession.ModelToDataTable();

                                await Task.Factory.StartNew(() =>
                                {
                                    syncData.Columns.Add("TransactionID");
                                    syncData.Columns.Add("ItemID");
                                    syncData.Columns.Add("IsSuccess");
                                    syncData.Columns.Add("ListType");
                                    syncData.Columns.Add("SyncStatus");
                                    syncData.Columns.Add("Remarks");
                                    syncData.Columns.Add("Key");

                                    //y["TransactionID"].ToString()
                                    syncData.Rows[0]["TransactionID"] = x["TransactionID"].ToString();
                                    syncData.Rows[0]["ListType"] = "Bills";
                                    syncData.Rows[0]["SyncStatus"] = "UNSUCCESSFUL";
                                    syncData.Rows[0]["Remarks"] = billResponse.StatusMessage;

                                    syncData.Rows[0]["ItemID"] = 5;
                                    syncData.Rows[0]["Key"] = x["TransactionID"].ToString();
                                    syncData.Rows[0]["IsSuccess"] = false;

                                });
                                await Increment(syncData);
                                await LogError(syncData);

                            }
                        }
                        else//for update
                        {
                            await AsyncOpenConnection();
                            var bill = request.AppendBillModRq();
                            bill.IncludeRetElementList.Add("ExpenseLineRet");
                            var qbBill = QBD.Bills.Find(f => f.TxnID == x["TxnID"].ToString()); //to refetch the current Editsequence it will cause bug if missed match
                            bill.EditSequence.SetValue(qbBill.EditSequence);
                            bill.TxnID.SetValue(qbBill.TxnID);
                            bill.VendorRef.ListID.SetValue(x["VendorListID"].ToString());
                            bill.VendorRef.FullName.SetValue(x["VendorName"].ToString());
                            bill.APAccountRef.ListID.SetValue(x["AccountListsID"].ToString());
                            bill.APAccountRef.FullName.SetValue(x["AccountName"].ToString());
                            //if (!string.IsNullOrEmpty(x["RefNumber"].ToString()))
                            bill.RefNumber.SetValue(x["RefNumber"].ToString());
                            bill.TxnDate.SetValue(Convert.ToDateTime(x["TxnDate"]));
                            bill.Memo.SetValue(x["Memo"].ToString());
                            if (!x.IsNull("DueDate"))
                            {
                                bill.DueDate.SetValue(Convert.ToDateTime(x["DueDate"]));
                            }
                            else
                            {
                                bill.DueDate.SetValue(Convert.ToDateTime(x["TxnDate"])); // if wala due date. same sa Invoice date nalang.
                            }

                            Parameters paramDetail = new Parameters()
                            {
                                CompanyID = Convert.ToInt32(x["CompanyID"]),
                                MWID = Convert.ToInt32(x["MWID"])
                            };

                            var txn = await Bills.UploadDetails(paramDetail);
                            foreach (DataRow details in txn.Rows)
                            {
                                var ExpenseLineDetails = bill.ExpenseLineModList.Append();
                                ExpenseLineDetails.TxnLineID.SetValue("-1");
                                ExpenseLineDetails.AccountRef.ListID.SetValue(details["COAListID"].ToString());
                                ExpenseLineDetails.AccountRef.FullName.SetValue(details["AccountName"].ToString());
                                ExpenseLineDetails.Amount.SetValue(Convert.ToDouble(details["Amt"]));
                                if (!string.IsNullOrEmpty(details["Memo"].ToString()))
                                    ExpenseLineDetails.Memo.SetValue(details["Memo"].ToString());
                                if (!details.IsNull("ListID") && QBD.Classes.Count() > 0)
                                {
                                    if (!string.IsNullOrEmpty(details["ListID"].ToString()))
                                    {
                                        ExpenseLineDetails.ClassRef.FullName.SetValue(details["Department"].ToString());
                                        ExpenseLineDetails.ClassRef.ListID.SetValue(details["ListID"].ToString());
                                    }
                                }
                                if (!details.IsNull("ProjListID") && QBD.Projects.Count() > 0)
                                {
                                    if (!string.IsNullOrEmpty(details["ProjListID"].ToString()))
                                    {
                                        ExpenseLineDetails.CustomerRef.FullName.SetValue(details["ProjectDescription"].ToString());
                                        ExpenseLineDetails.CustomerRef.ListID.SetValue(details["ProjListID"].ToString());
                                    }
                                }
                            }

                            var responseLists = await Task.Factory.StartNew(() => sessionManager.DoRequests(request).ResponseList);
                            var responseCount = responseLists.Count;
                            IResponse billResponse = responseLists.GetAt(responseCount - 1);
                            if (billResponse.Detail != null)
                            {
                                //IBillMod billRet = (IBillMod)billResponse.Detail;
                                //if (billRet != null)
                                //{//RRIEL sync here
                                // //BILL Link ID billRet.TxnID.GetValue();

                                //}
                                //RRIEL rebirth to original logic. no need to verify the data. used the existing values
                                var syncData = UserSession.ModelToDataTable();
                                await Task.Factory.StartNew(() =>
                                {
                                    syncData.Columns.Add("TransactionID");
                                    syncData.Columns.Add("BillLInkID");
                                    syncData.Columns.Add("EditSequence");
                                });
                                await QBD.ReadData();
                                qbBill = QBD.Bills.Find(f => f.TxnID == x["TxnID"].ToString()); //to refetch the current Editsequence it will cause bug if missed match
                                await Task.Factory.StartNew(() =>
                                {

                                    syncData.Rows[0]["TransactionID"] = x["TransactionID"].ToString();
                                    syncData.Rows[0]["BillLInkID"] = qbBill.TxnID;
                                    syncData.Rows[0]["EditSequence"] = qbBill.EditSequence;
                                });
                                await Bills.Sync(syncData);
                                await Task.Factory.StartNew(() =>
                                {
                                    syncData.Columns.Add("ItemID");
                                    syncData.Columns.Add("IsSuccess");
                                    syncData.Columns.Add("Key");
                                    syncData.Rows[0]["Key"] = x["TransactionID"].ToString();
                                    syncData.Rows[0]["ItemID"] = 5;
                                
                                    syncData.Rows[0]["IsSuccess"] = true;
                                });
                                await Increment(syncData);
                            }
                            else
                            {
                                var syncData = UserSession.ModelToDataTable();

                                await Task.Factory.StartNew(() =>
                                {
                                    syncData.Columns.Add("TransactionID");
                                    syncData.Columns.Add("ItemID");
                                    syncData.Columns.Add("IsSuccess");
                                    syncData.Columns.Add("ListType");
                                    syncData.Columns.Add("SyncStatus");
                                    syncData.Columns.Add("Remarks");
                                    syncData.Columns.Add("Key");
                                    syncData.Rows[0]["Key"] = x["TransactionID"].ToString();
                                    //y["TransactionID"].ToString()
                                    syncData.Rows[0]["TransactionID"] = x["TransactionID"].ToString();
                                    syncData.Rows[0]["ListType"] = "Bills";
                                    syncData.Rows[0]["SyncStatus"] = "UNSUCCESSFUL";
                                    syncData.Rows[0]["Remarks"] = billResponse.StatusMessage;

                                    syncData.Rows[0]["ItemID"] = 5;
                                    syncData.Rows[0]["IsSuccess"] = false;

                                });
                                await Increment(syncData);
                                await LogError(syncData);
                            }

                        }
                    }
                    else
                    {
                        var syncData = UserSession.ModelToDataTable();
                        await Task.Factory.StartNew(() =>
                        {
                            syncData.Columns.Add("TransactionID");
                            syncData.Columns.Add("ItemID");
                            syncData.Columns.Add("IsSuccess");
                            syncData.Columns.Add("ListType");
                            syncData.Columns.Add("Remarks");
                            syncData.Columns.Add("PaymentErrType");
                            syncData.Columns.Add("Key");
                            syncData.Rows[0]["Key"] = x["TransactionID"].ToString();
                            syncData.Rows[0]["TransactionID"] = x["TransactionID"];
                            syncData.Rows[0]["ListType"] = "Bills";
                            syncData.Rows[0]["ItemID"] = 5;
                            syncData.Rows[0]["IsSuccess"] = false;

                        });
                        await LogCustomError(syncData);
                        await Increment(syncData);
                    }
                }
                catch (Exception ex)
                {
                    var syncData = UserSession.ModelToDataTable();
                    await Task.Factory.StartNew(() =>
                    {
                        syncData.Columns.Add("TransactionID");
                        syncData.Columns.Add("ItemID");
                        syncData.Columns.Add("IsSuccess");
                        syncData.Columns.Add("ListType");
                        syncData.Columns.Add("SyncStatus");
                        syncData.Columns.Add("Remarks");
                        syncData.Columns.Add("Key");
                        syncData.Rows[0]["Key"] = x["TransactionID"].ToString();
                        //y["TransactionID"].ToString()
                        syncData.Rows[0]["TransactionID"] = x["TransactionID"].ToString();
                        syncData.Rows[0]["ListType"] = "Bills";
                        syncData.Rows[0]["SyncStatus"] = "UNSUCCESSFUL";
                        syncData.Rows[0]["Remarks"] = ex.Message;

                        syncData.Rows[0]["ItemID"] = 5;
                        syncData.Rows[0]["IsSuccess"] = false;

                    });
                    await LogError(syncData);
                    await Increment(syncData);
                    
                }
                finally
                {
                    await AsyncCloseConnection();
                }

            }
        }

        public async Task SyncBillInsert(DataRow x)
        {
            await AsyncOpenConnection();// prompt
            var bill = request.AppendBillAddRq();
            bill.VendorRef.ListID.SetValue(x["VendorListID"].ToString());
            bill.VendorRef.FullName.SetValue(x["VendorName"].ToString());
            bill.APAccountRef.ListID.SetValue(x["AccountListsID"].ToString());
            bill.APAccountRef.FullName.SetValue(x["AccountName"].ToString());
            if (!string.IsNullOrEmpty(x["RefNumber"].ToString()))
                bill.RefNumber.SetValue(x["RefNumber"].ToString());
            bill.TxnDate.SetValue(Convert.ToDateTime(x["TxnDate"]));
            bill.Memo.SetValue(x["Memo"].ToString());
            if (!x.IsNull("DueDate"))
            {
                bill.DueDate.SetValue(Convert.ToDateTime(x["DueDate"]));
            }
            else
            {
                bill.DueDate.SetValue(Convert.ToDateTime(x["TxnDate"])); // if wala due date. same sa Invoice date nalang.
            }

            //session.MWID = x.MWID;
            Parameters param = new Parameters()
            {
                CompanyID = Convert.ToInt32(x["CompanyID"]),
                MWID = Convert.ToInt32(x["MWID"])
            };
            var txn = await Bills.UploadDetails(param);
            foreach (DataRow details in txn.Rows)
            {
                var ExpenseLineDetails = bill.ExpenseLineAddList.Append();
                ExpenseLineDetails.AccountRef.ListID.SetValue(details["COAListID"].ToString());
                ExpenseLineDetails.AccountRef.FullName.SetValue(details["AccountName"].ToString());
                ExpenseLineDetails.Amount.SetValue(Convert.ToDouble(details["Amt"]));
                if (!string.IsNullOrEmpty(details["Memo"].ToString()))
                    ExpenseLineDetails.Memo.SetValue(details["Memo"].ToString());
                if (!details.IsNull("ListID") && QBD.Classes.Count() > 0)
                {
                    IMsgSetRequest reqd = sessionManager.CreateMsgSetRequest("US", 13, 0);
                    IClassQuery ClassQueryRq = reqd.AppendClassQueryRq();
                    ClassQueryRq.ORListQuery.FullNameList.Add(details["Department"].ToString());
                    var responseRequestMsgSet = sessionManager.DoRequests(reqd).ResponseList;
                    var responseCountRequestMsgSet = responseRequestMsgSet.Count;
                    IResponse classResponse = responseRequestMsgSet.GetAt(responseCountRequestMsgSet - 1);
                    IClassRetList classRet = (IClassRetList)classResponse.Detail;
                    if (classRet != null)
                    {
                        var cls = QBD.Classes.Find(f => f.ListID == details["ListID"].ToString()); //checks if current class exist in QBD JRAPI05152025
                        if (cls != null)
                        {
                            ExpenseLineDetails.ClassRef.FullName.SetValue(details["Department"].ToString());
                            ExpenseLineDetails.ClassRef.ListID.SetValue(details["ListID"].ToString());
                        }
                    }
                }
                if (!details.IsNull("ProjListID") && QBD.Projects.Count() > 0)
                {
                    IMsgSetRequest reqp = sessionManager.CreateMsgSetRequest("US", 13, 0);
                    ICustomerQuery CustomerQueryRq = reqp.AppendCustomerQueryRq();
                    CustomerQueryRq.ORCustomerListQuery.FullNameList.Add(details["ProjectDescription"].ToString());
                    var responseRequestMsgSet = sessionManager.DoRequests(reqp).ResponseList;
                    var responseCountRequestMsgSet = responseRequestMsgSet.Count;
                    IResponse customerResponse = responseRequestMsgSet.GetAt(responseCountRequestMsgSet - 1);
                    ICustomerRetList customerRet = (ICustomerRetList)customerResponse.Detail;
                    if (customerRet != null)
                    {
                        var prj = QBD.Projects.Find(f => f.ListID == details["ProjListID"].ToString()); //checks if current project exist in QBD JRAPI05152025
                        if (prj != null)
                        {
                            ExpenseLineDetails.CustomerRef.FullName.SetValue(details["ProjectDescription"].ToString());
                            ExpenseLineDetails.CustomerRef.ListID.SetValue(details["ProjListID"].ToString());
                        }
                    }
                }

            }
            var responseLists = await Task.Factory.StartNew(() => sessionManager.DoRequests(request).ResponseList);
            var responseCount = responseLists.Count;
            IResponse billResponse = responseLists.GetAt(responseCount - 1);
            IBillRet billRet = (IBillRet)billResponse.Detail;
            if (billRet != null)
            {//RRIEL sync here
             //BILL Link ID billRet.TxnID.GetValue();
                var syncData = UserSession.ModelToDataTable();
                await Task.Factory.StartNew(() =>
                {
                    syncData.Columns.Add("TransactionID");
                    syncData.Columns.Add("BillLInkID");
                    syncData.Columns.Add("EditSequence");
                });
                await Task.Factory.StartNew(() =>
                {
                    syncData.Rows[0]["TransactionID"] = x["TransactionID"].ToString();
                    syncData.Rows[0]["BillLInkID"] = billRet.TxnID.GetValue();
                    syncData.Rows[0]["EditSequence"] = billRet.EditSequence.GetValue();
                });
                await Bills.Sync(syncData);

                await Task.Factory.StartNew(() =>
                {
                    syncData.Columns.Add("ItemID");
                    syncData.Columns.Add("IsSuccess");
                    syncData.Columns.Add("Key");
                    syncData.Rows[0]["Key"] = string.Empty;
                    syncData.Rows[0]["ItemID"] = 5;
                    syncData.Rows[0]["IsSuccess"] = true;
                    syncData.Rows[0]["Key"] = x["TransactionID"].ToString();
                });
                await Increment(syncData);

            }
            else
            {
                var syncData = UserSession.ModelToDataTable();

                await Task.Factory.StartNew(() =>
                {
                    syncData.Columns.Add("TransactionID");
                    syncData.Columns.Add("ItemID");
                    syncData.Columns.Add("IsSuccess");
                    syncData.Columns.Add("ListType");
                    syncData.Columns.Add("SyncStatus");
                    syncData.Columns.Add("Remarks");
                    syncData.Columns.Add("Key");

                    //y["TransactionID"].ToString()
                    syncData.Rows[0]["TransactionID"] = x["TransactionID"].ToString();
                    syncData.Rows[0]["ListType"] = "Bills";
                    syncData.Rows[0]["SyncStatus"] = "UNSUCCESSFUL";
                    syncData.Rows[0]["Remarks"] = billResponse.StatusMessage;

                    syncData.Rows[0]["ItemID"] = 5;
                    syncData.Rows[0]["Key"] = x["TransactionID"].ToString();
                    syncData.Rows[0]["IsSuccess"] = false;

                });
                await Increment(syncData);
                await LogError(syncData);

            }
            await AsyncCloseConnection();
        }

        public async Task SyncBillUpdates(DataRow x)
        {
            try
            {
                await AsyncOpenConnection();
                var bill = request.AppendBillModRq();
                bill.IncludeRetElementList.Add("ExpenseLineRet");
                var qbBill = QBD.Bills.Find(f => f.TxnID == x["TxnID"].ToString()); //to refetch the current Editsequence it will cause bug if missed match
                bill.EditSequence.SetValue(qbBill.EditSequence);
                bill.TxnID.SetValue(qbBill.TxnID);
                bill.VendorRef.ListID.SetValue(x["VendorListID"].ToString());
                bill.VendorRef.FullName.SetValue(x["VendorName"].ToString());
                bill.APAccountRef.ListID.SetValue(x["AccountListsID"].ToString());
                bill.APAccountRef.FullName.SetValue(x["AccountName"].ToString());
                //if (!string.IsNullOrEmpty(x["RefNumber"].ToString()))
                bill.RefNumber.SetValue(x["RefNumber"].ToString());
                bill.TxnDate.SetValue(Convert.ToDateTime(x["TxnDate"]));
                bill.Memo.SetValue(x["Memo"].ToString());
                if (!x.IsNull("DueDate"))
                {
                    bill.DueDate.SetValue(Convert.ToDateTime(x["DueDate"]));
                }
                else
                {
                    bill.DueDate.SetValue(Convert.ToDateTime(x["TxnDate"])); // if wala due date. same sa Invoice date nalang.
                }

                Parameters paramDetail = new Parameters()
                {
                    CompanyID = Convert.ToInt32(x["CompanyID"]),
                    MWID = Convert.ToInt32(x["MWID"])
                };

                var txn = await Bills.UploadDetails(paramDetail);
                foreach (DataRow details in txn.Rows)
                {
                    var ExpenseLineDetails = bill.ExpenseLineModList.Append();
                    ExpenseLineDetails.TxnLineID.SetValue("-1");
                    ExpenseLineDetails.AccountRef.ListID.SetValue(details["COAListID"].ToString());
                    ExpenseLineDetails.AccountRef.FullName.SetValue(details["AccountName"].ToString());
                    ExpenseLineDetails.Amount.SetValue(Convert.ToDouble(details["Amt"]));
                    if (!string.IsNullOrEmpty(details["Memo"].ToString()))
                        ExpenseLineDetails.Memo.SetValue(details["Memo"].ToString());
                    if (!details.IsNull("ListID") && QBD.Classes.Count() > 0)
                    {
                        if (!string.IsNullOrEmpty(details["ListID"].ToString()))
                        {
                            ExpenseLineDetails.ClassRef.FullName.SetValue(details["Department"].ToString());
                            ExpenseLineDetails.ClassRef.ListID.SetValue(details["ListID"].ToString());
                        }
                    }
                    if (!details.IsNull("ProjListID") && QBD.Projects.Count() > 0)
                    {
                        if (!string.IsNullOrEmpty(details["ProjListID"].ToString()))
                        {
                            ExpenseLineDetails.CustomerRef.FullName.SetValue(details["ProjectDescription"].ToString());
                            ExpenseLineDetails.CustomerRef.ListID.SetValue(details["ProjListID"].ToString());
                        }
                    }
                }

                var responseLists = await Task.Factory.StartNew(() => sessionManager.DoRequests(request).ResponseList);
                var responseCount = responseLists.Count;
                IResponse billResponse = responseLists.GetAt(responseCount - 1);
                if (billResponse.Detail != null)
                {
                    //IBillMod billRet = (IBillMod)billResponse.Detail;
                    //if (billRet != null)
                    //{//RRIEL sync here
                    // //BILL Link ID billRet.TxnID.GetValue();

                    //}
                    //RRIEL rebirth to original logic. no need to verify the data. used the existing values
                    var syncData = UserSession.ModelToDataTable();
                    await Task.Factory.StartNew(() =>
                    {
                        syncData.Columns.Add("TransactionID");
                        syncData.Columns.Add("BillLInkID");
                        syncData.Columns.Add("EditSequence");
                    });
                    await QBD.ReadData();
                    qbBill = QBD.Bills.Find(f => f.TxnID == x["TxnID"].ToString()); //to refetch the current Editsequence it will cause bug if missed match
                    await Task.Factory.StartNew(() =>
                    {

                        syncData.Rows[0]["TransactionID"] = x["TransactionID"].ToString();
                        syncData.Rows[0]["BillLInkID"] = qbBill.TxnID;
                        syncData.Rows[0]["EditSequence"] = qbBill.EditSequence;
                    });
                    await Bills.Sync(syncData);
                    await Task.Factory.StartNew(() =>
                    {
                        syncData.Columns.Add("ItemID");
                        syncData.Columns.Add("IsSuccess");
                        syncData.Columns.Add("Key");
                        syncData.Rows[0]["Key"] = x["TransactionID"].ToString();
                        syncData.Rows[0]["ItemID"] = 5;

                        syncData.Rows[0]["IsSuccess"] = true;
                    });
                    await Increment(syncData);
                }
                else
                {
                    var syncData = UserSession.ModelToDataTable();

                    await Task.Factory.StartNew(() =>
                    {
                        syncData.Columns.Add("TransactionID");
                        syncData.Columns.Add("ItemID");
                        syncData.Columns.Add("IsSuccess");
                        syncData.Columns.Add("ListType");
                        syncData.Columns.Add("SyncStatus");
                        syncData.Columns.Add("Remarks");
                        syncData.Columns.Add("Key");
                        syncData.Rows[0]["Key"] = x["TransactionID"].ToString();
                        //y["TransactionID"].ToString()
                        syncData.Rows[0]["TransactionID"] = x["TransactionID"].ToString();
                        syncData.Rows[0]["ListType"] = "Bills";
                        syncData.Rows[0]["SyncStatus"] = "UNSUCCESSFUL";
                        syncData.Rows[0]["Remarks"] = billResponse.StatusMessage;

                        syncData.Rows[0]["ItemID"] = 5;
                        syncData.Rows[0]["IsSuccess"] = false;

                    });
                    await Increment(syncData);
                    await LogError(syncData);
                }
                await AsyncCloseConnection();
            } catch (Exception ex)
            {
                var syncData = UserSession.ModelToDataTable();
                await Task.Factory.StartNew(() =>
                {
                    syncData.Columns.Add("TransactionID");
                    syncData.Columns.Add("ItemID");
                    syncData.Columns.Add("IsSuccess");
                    syncData.Columns.Add("ListType");
                    syncData.Columns.Add("SyncStatus");
                    syncData.Columns.Add("Remarks");
                    syncData.Columns.Add("Key");
                    syncData.Rows[0]["Key"] = x["TransactionID"].ToString();
                    //y["TransactionID"].ToString()
                    syncData.Rows[0]["TransactionID"] = x["TransactionID"].ToString();
                    syncData.Rows[0]["ListType"] = "Bills";
                    syncData.Rows[0]["SyncStatus"] = "UNSUCCESSFUL";
                    syncData.Rows[0]["Remarks"] = ex.Message;

                    syncData.Rows[0]["ItemID"] = 5;
                    syncData.Rows[0]["IsSuccess"] = false;

                });
                await LogError(syncData);
                await Increment(syncData);
            }
        }
    }
}
