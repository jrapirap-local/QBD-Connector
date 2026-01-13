using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MBPH.QBDLinq.Model; //all models are in Linq
namespace MBPH_Connector.IRepositories
{
    interface ISyncLists //This is one time only. kaya puro New lang
    {
        Task Download();
        Task SyncClasses();
        Task SyncProjects();
        Task SyncAccounts();
        Task SyncVendors();
        Task EndSync();
        Task EndSyncLists();
    }
}
