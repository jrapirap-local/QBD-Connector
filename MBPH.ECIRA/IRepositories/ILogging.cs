using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MBPH.ECIRA.Model;
namespace MBPH.ECIRA.IRepositories
{
    interface ILogging
    {
        DataTable Log(SystemLogs log);
        DataTable Log(Exception error,SystemLogs log);
    }
}
