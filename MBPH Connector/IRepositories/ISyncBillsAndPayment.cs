using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBPH_Connector.IRepositories
{
    interface ISyncBillsAndPayment
    {
        Task Download();
        //Task SyncBills();
        Task SyncBillsPayments();
        Task SyncPaymentInserts(DataRow x, DataTable payments);
        Task SyncPaymentUpdates(DataRow x, DataTable payments);
        Task SyncBillInsert(DataRow x);
        Task SyncBillUpdates(DataRow x);
        //Task SyncPayments();
        Task VoidPayment(DataRow x, DataTable paymentDetails);
        Task SyncAsVoid(DataRow y, String t, String e, string PaymentVoucherNo);
        Task SyncDeletedBills();
    }
}
