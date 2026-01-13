using MBPH.Encryption;
using MBPH_Connector.Abstraction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MBPH_Connector.Repositories.AsyncECIRA;
namespace MBPH_Connector
{
    public partial class Form1 : Form
    {
        
        Process _pro = new Process();
        public Form1()
        {
            InitializeComponent();
            InitTimer();
        }

        private void InitTimer() {
            timer1.Interval = 2000; // set small delay;
            timer1.Start();
        }
        private void label4_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("If you Exist MBPH Connector. the communication between QuickBooks Desktop and MyBillsPH will loose.", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DialogResult.Yes == result)
                Application.Exit();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true; // Show in taskbar
            }
        }

        private async Task BeginWatcher() {
            var clip = Clipboard.GetText();
            if (!string.IsNullOrEmpty(Clipboard.GetText()))
            {
                //bool validEncryption = false;
                try
                {
                    clip.Decrypt(); // to validate if valid mbph encryption.
                    timer1.Stop(); // stop only when valid.
                    Session.SessionWatcher(clip);
                    Clipboard.Clear();
                    if (Session.UserSession.Step == 1)// Step 1 ,2 ,3 . 1 Binding 2, Syncing of Accounts and Projects(Department) , Classes.
                    {
                        SyncID = Guid.NewGuid().ToString();// unique identifyer per transaction // upon download
                        LogID = 1; //RESET TO 1

                        await _pro.BindCompany(); //force wait. coz non async.
                    }
                    else if (Session.UserSession.Step == 2) // ON DL download all QBD Info.
                    {
                        SyncID = Guid.NewGuid().ToString(); //rriel09042024 set unique identity for the process.
                        await _pro.DownloadQBDData();
                    }
                    else if (Session.UserSession.Step == 3)//Proceed
                    {
                        await _pro.SyncVendors();
                    }
                    else if (Session.UserSession.Step == 4)
                    {
                        await _pro.SyncClasses();
                    }
                    else if (Session.UserSession.Step == 5)
                    {
                        await _pro.SyncProjects();
                    }
                    else if (Session.UserSession.Step == 6)
                    {
                        await _pro.SyncAccounts();
                    }
                    else if (Session.UserSession.Step == 7)
                    {
                        //await _pro.SyncBills();
                        await _pro.SyncBillsAndPayments(); //modified by JRAPI06092025 ; New Process Flow for syncing ob Bills and Payments
                    }
                    //else if (Session.UserSession.Step == 8)
                    //{
                    //    await _pro.SyncPayments();
                    //}
                
                    // only in process so it wont affect the reality

                }
                catch
                {
                    //DO NOTHING. 
                }
                finally
                {
                    timer1.Start();//re start to watch again
                }
            }
        }
        private async void timer1_Tick(object sender, EventArgs e)
        {
            await BeginWatcher();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;  // Cancel the default close operation

                // Minimize to system tray
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false; // Hide from taskbar

            }

            base.OnFormClosing(e);
        }

        private void clipboardWatcher_Tick(object sender, EventArgs e)
        {
            Watcher.Clipboard = Clipboard.GetText();
        }

       
    }
}
