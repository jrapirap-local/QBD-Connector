using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MBPH.QBDLinq.Model;
//using QBFC14Lib;
using QBFC13Lib;

using System.IO;
using  static  MBPH.Extension.Extensions;
//using QBFC16Lib;
//using Interop.QBFC13;
namespace QuickBooksReader
{
    public static class QBD
    {
        public static IQBSessionManager sessionManager = new QBSessionManager();
        public static IMsgSetRequest request;
        public static string qbdCompanyName { get; set; }
        public static string qbdCompanyPath { get; set; }
        public static string qbdFileID{ get; set; }
        //public static IResponse response;
        public static bool isIntegratedConnection { get; set; } = false;
        public static int attemptTry { get; set; } = 0;

        public static bool OpenQBD(string BindedCompany="") {
            if (String.IsNullOrEmpty(BindedCompany))
            {
                BindedCompany = qbdCompanyPath;
            }
            try
            {
                if (sessionManager != null) // existing session . 
                {
                    sessionManager.EndSession(); // close muna natin
                    sessionManager.CloseConnection(); // maiwasan multiple connection request
                }                
                sessionManager.OpenConnection("", GetConfig("AppName"));
                sessionManager.BeginSession("", ENOpenMode.omSingleUser); // dont care for al l// not integrated, meaning open ang company
                request = sessionManager.CreateMsgSetRequest("US", 13,0); // "QuickBook Country Version", QBFC Version, QBFC Version 2ndary.  
                request.Attributes.OnError = ENRqOnError.roeContinue;//BackUp first before syncing. to be able to use roeRollBack.
                //
                qbdCompanyName =  Path.GetFileNameWithoutExtension(sessionManager.GetCurrentCompanyFileName());
                qbdCompanyPath = sessionManager.GetCurrentCompanyFileName();
                return true;
            }
            catch(Exception ex)
            {
                //if (sessionManager != null)
                //{
                //    sessionManager.EndSession();
                //    sessionManager.CloseConnection();
                //}
                //try
                //{
                //    sessionManager.OpenConnection("", "MBPH");
                //    sessionManager.BeginSession(BindedCompany, ENOpenMode.omSingleUser); // dont care for all//Integrated connection. naka close company.
                //    request = sessionManager.CreateMsgSetRequest("US", 13, 0); // QBFC Version. "US" << QBD Version , 
                //    request.Attributes.OnError = ENRqOnError.roeContinue;
                //    qbdCompanyName = Path.GetFileNameWithoutExtension(sessionManager.GetCurrentCompanyFileName());
                //    qbdCompanyPath = sessionManager.GetCurrentCompanyFileName();
                //    isIntegratedConnection = true;
                //    return true;
                //}
                //catch(Exception ex) {

                //    throw new Exception( ex.Message);

                //}
                throw ex;
               // return false;
            }
            //finally
            //{
            //    if (sessionManager != null)
            //    {
            //        sessionManager.EndSession();
            //        sessionManager.CloseConnection();
            //    }
            //}
        }

        public static void CloseQBD()
        {
            try
            {
                if (sessionManager != null)
                {
                    sessionManager.EndSession();
                    sessionManager.CloseConnection();
                }
            }
            finally
            {

                //do nothing
            }
            
        }
    }
}
