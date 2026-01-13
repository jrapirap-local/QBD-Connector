using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MBPH_Connector
{
    public static class Watcher{
        public static string Clipboard { get; set; }
    };
    static class Program
    {
        
        private static Mutex mutex = new Mutex(true, "{D56A0BDB-D76F-458A-BFF3-70BB86A550F2}");//ako nag generate neto.using yung app ko din na ginawa. para unique forever
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {


            /*
             DEVELOPMENT ONLY. REMOVED THIS ON SSL CERTIFICATE--- IMPORTANT  ----
             */
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //RRIEL12232024 added TLS enforce
            //ServicePointManager.ServerCertificateValidationCallback =
            //new RemoteCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true);
            //ServicePointManager.CheckCertificateRevocationList = false;


            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("MBPH Connector is already Running", "MBPHy Connector");
            }
        }
    }
}
