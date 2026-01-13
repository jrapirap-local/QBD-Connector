using MBPH.ECIRA;
using MBPH.ECIRA.Enum;
using MBPH.ECIRA.Model;
using MBPH_Connector.Abstraction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuickBooksReader.Library.QBD;
using static QuickBooksReader.QBD;
using static MBPHReader.MyBillsPH;
using MBPHReader.Model;
using MBPH.CMD;

namespace MBPH_Connector.Repositories
{
    public static class AsyncECIRA
    {
        public static string SyncID { get; set; }
        public static int LogID { get; set; }
        public static SystemLogs logs { get; set; } //ECIRA Connector
        public static List<SystemLogDetails> details { get; set; } //ECIRA Connector
        public static async Task<DataTable> LogAsync(SystemLogs log) // Async . top bottom.  
        {
            //await DefaultValue();
            if (log.ErrorID != null)
                log.Success = false;
            DataTable ret = await Task<DataTable>.Factory.StartNew(() =>
            {
                return log.eciraLog(); // log.eciraLog();
            });

            return ret;

        }
        public static async Task DefaultValue()
        {
            await AsyncOpenConnection();
            await Task.Factory.StartNew(() =>
            {
                logs = new SystemLogs
                {
                    CompanyID = Convert.ToInt32(Session.UserSession.CompanyID),
                    AddedBy = Session.UserSession.AddedBy,
                    MBPHModuleID = Convert.ToInt32(MBPHModule.QBDsync),//Session.UserSession.MBPHModuleID,//STATIC na
                    MethodName = $"Connector:{System.Reflection.MethodBase.GetCurrentMethod().Name}",//unbindApp.name <<
                    SessionID = Session.UserSession.SessionID
                };
                details = new List<SystemLogDetails>();
                details.Add(new SystemLogDetails { ActionDone = "Connector:Binding Company", Value = $"Quicbooks Company : {qbdCompanyPath}" });
                details.Add(new SystemLogDetails { ActionDone = "Connector:Binding Company", Value = $"Bios Computer Serial : {bios.GetSerialNumber()}" });
                details.Add(new SystemLogDetails { ActionDone = "Connector:Binding Company", Value = $"Server Name : {Environment.MachineName}" });
                logs.Details = details;
            });
        }

        public static async Task AsyncCloseConnection()
        {
            await Task.Factory.StartNew(() => CloseQBD());
        }
        public static async Task AsyncOpenConnection()
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    OpenQBD(Session.UserSession.QBDCompany); //QBD/QBFC
                }
                catch (Exception ex)
                {
                    try
                    {

                        Parameters com = new Parameters()
                        {
                            CompanyID = Convert.ToInt32(Session.UserSession.CompanyID),
                            ResultMessage = $"{ex.Message}"
                        };
                        PushMessage(com);
                    }
                    catch
                    {
                        //throw deeper;
                        //Do nothing
                    }

                }
            });
        }
    }
}
