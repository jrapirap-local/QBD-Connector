using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MBPH.ECIRA.Repositories;
using MBPH.ECIRA.IRepositories;
using MBPH.ECIRA.Model;
using System.Data;

namespace MBPH.ECIRA
{
    public static class ecira
    {
        private static ILogging _ecira = new Logging();
        public static DataTable eciraLog(this SystemLogs log)
        {
            return _ecira.Log(log);
        }
        public static DataTable eciraLog(this Exception error,SystemLogs log)
        {
            return _ecira.Log(error,log);
        }
    }
}
