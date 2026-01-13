using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MBPH_Connector.IRepositories;
using static QuickBooksReader.QBD;
using static MBPHReader.MyBillsPH;
using static MBPH.Extension.Extensions;
using MBPH_Connector.Abstraction;
using MBPH.ECIRA.Model;
using MBPH.CMD;
using MBPHReader.Model;
using System.Data;
using static MBPH_Connector.Repositories.AsyncECIRA;
namespace MBPH_Connector.Repositories
{
    public class CompanyBinding:ICompanyBinding
    {
        public async Task BindCompany() {
            await DefaultValue();
            logs.MethodName = $"Connector:{System.Reflection.MethodBase.GetCurrentMethod().Name}";
            try
            {
                await AsyncOpenConnection();
                Parameters com = new Parameters()
                {
                    CompanyID = Convert.ToInt32(Session.UserSession.CompanyID),
                    QBDCompany = qbdCompanyPath,
                    SyncedBy = Session.UserSession.AddedBy,
                    BiosComputerSerial = bios.GetSerialNumber(),
                    ServerName = System.Environment.MachineName,
                    SyncStartDate = Session.UserSession.SyncStartDate,
                    SyncGUID = SyncID,
                    ResultMessage = $"Congratulations!You have now successfully binded your MBPH Company { Session.UserSession.CompanyName } thru QuickBooks Desktop with { qbdCompanyName }"
                };
                //Hindi ubra dynamic message sa try catch final.  no choice. kasi ang binding is for success only. hindi pwede sa final. sa Try kasi yung Open. qbdCompanyPath is triggered when OpenConnection
                await BindQBDCompany(com);
            }
            catch(Exception ex)
            {
                //Actual ex.eciraLog();
                Parameters com = new Parameters()
                {
                    CompanyID = Convert.ToInt32(Session.UserSession.CompanyID),
                    QBDCompany = qbdCompanyPath,
                    SyncedBy = Session.UserSession.AddedBy,
                    BiosComputerSerial = bios.GetSerialNumber(),
                    ServerName = System.Environment.MachineName,
                    SyncStartDate = Session.UserSession.SyncStartDate,
                    SyncGUID = SyncID,
                    ResultMessage = $"{ex.Message}",
                    IsSuccess = false,
                    ExceptionMessage = ex.Message
                };

                details.Add(new SystemLogDetails { ActionDone = "Exception Message", Value = ex.Message });
                if (ex.InnerException != null)
                {
                    details.Add(new SystemLogDetails { ActionDone = "Inner Exception", Value = ex.InnerException.ToString() });
                    com.ResultMessage = ex.InnerException.ToString();
                }
                details.Add(new SystemLogDetails { ActionDone = "Stack Trace", Value = ex.StackTrace });
                logs.Success = false; // force false. cozz is double handling.
                logs.Details = details;
                await PushMessage(com);
            }
            finally
            {
                await LogAsync(logs);
            }
        }

        public async Task<bool> IsValidCompany()
        {
            try
            {
                await DefaultValue();
                logs.MethodName = $"Connector:{System.Reflection.MethodBase.GetCurrentMethod().Name}";
                await AsyncOpenConnection();//
                var param = new Parameters
                {//BeginSession. Session started. Company Not Yet Open. kasi wala pa syncing and wala process. Enter Admin Password: [                ]
                    QBDCompany = qbdCompanyPath,
                    CompanyID = Convert.ToInt32(Session.UserSession.CompanyID)
                };
                if (string.IsNullOrEmpty(qbdCompanyPath)) {
                    return false;
                }
                var currentCompanyInfo = await ValidateCompanyAccess(param.ModelToDataTable());
                if (currentCompanyInfo.Rows.Count == 0)
                {
                    return true;
                }
                else
                {
                    var x = currentCompanyInfo.Rows.Cast<DataRow>().FirstOrDefault();
                    if (x["CompanyID"].ToString() == Session.UserSession.CompanyID)
                    {
                        if (x["BiosComputerSerial"].ToString() != bios.GetSerialNumber() && !x.IsNull("BiosComputerSerial"))
                        {
                            //logs.ErrorID = 3;
                            ////logs.Details.Add(new SystemLogDetails {ActionDone= "Binding Company",Value=$"ErrorID: {logs.ErrorID}" });
                            //var dt = await LogAsync(logs);
                            //foreach (DataRow d in dt.Rows)
                            //{
                            //    param.ResultMessage = Convert.ToString(d["Message"])
                            //        .Replace("[QBCompanyName]", $"{qbdCompanyName}");
                            //    await PushMessage(param);
                            //    return false;
                            //}
                            param.ResultMessage = "QuickBooks Company \"[QBCompanyName]\" is not in the right PC, Only admin PC can sync QuickBooks Desktop ";
                            await PushMessage(param);
                            return false; //invalid unit PC

                        }
                        else if (x["CompanyFilePath"].ToString() != qbdCompanyPath)
                        {
                            //logs.ErrorID = 33;
                            //var dt = await LogAsync(logs);
                            //foreach (DataRow d in dt.Rows)
                            //{
                            //    param.ResultMessage = Convert.ToString(d["Message"]).Replace("[CompanyName]", $"{currentCompanyInfo.Rows.Cast<DataRow>().FirstOrDefault()["CompanyName"]}")
                            //        .Replace("[QBCompanyName]", $"{qbdCompanyName}")
                            //        ; //$"Company \"{currentCompanyInfo.Rows.Cast<DataRow>().FirstOrDefault()["CompanyName"]}\" is already binded to Quickbook company \"{qbdCompanyName}\"";
                            //    await PushMessage(param);
                            //    return false;
                            //}
                            param.ResultMessage = "This company is bound to [QBDCompany], but you are attempting to sync records to a different company. Please ensure that you select and load the correct company to proceed with the process. Syncing will not continue.";
                            await PushMessage(param);
                            return false;
                        }
                        else
                        {
                            return true;
                        }

                    }
                    else
                    {

                        if (Session.UserSession.Step == 1)
                        {
                            //logs.ErrorID = 2;
                            param.ResultMessage = "The company you are trying to associate with your MBPH company is already bound with another MBPH company.";
                        }
                        else {
                            param.ResultMessage = "This company is bound to [QBDCompany], but you are attempting to sync records to a different company. Please ensure that you select and load the correct company to proceed with the process. Syncing will not continue.";
                            //logs.ErrorID = 33;
                        }


                        //var dt = await LogAsync(logs);
                        //foreach (DataRow d in dt.Rows)
                        //{
                        //    param.ResultMessage = Convert.ToString(d["Message"]).Replace("[CompanyName]", $"{currentCompanyInfo.Rows.Cast<DataRow>().FirstOrDefault()["CompanyName"]}")
                        //        .Replace("[QBCompanyName]", $"{qbdCompanyName}")
                        //        ; //$"Company \"{currentCompanyInfo.Rows.Cast<DataRow>().FirstOrDefault()["CompanyName"]}\" is already binded to Quickbook company \"{qbdCompanyName}\"";
                        //    await PushMessage(param);
                        //    return false;
                        //}
                        await PushMessage(param);

                        return false;

                    }
                }
            }
            catch
            {
                return false;
            }
            

        }
    }
}
