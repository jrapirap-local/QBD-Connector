using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MBPH.ECIRA.Model;
using MBPH.ECIRA.IRepositories;
using static MBPH.ECIRA.Repositories.ECIRAHttpClient;
using Newtonsoft.Json;
using System.Configuration;
using System.Data;

namespace MBPH.ECIRA.Repositories
{
    public class Logging : ILogging
    {
        public DataTable Log(SystemLogs log)
        {
            byte[] user_model = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(log));
            var data =  GetDataFromApiAsync($"{ConfigurationManager.AppSettings["BaseUrl"]}/ecira/LogAsync?data={Convert.ToBase64String(user_model)}").Result;
            //var parsedata = JsonConvert.DeserializeObject<DataTable>(data);
            if (data != null)
            {
                return JsonConvert.DeserializeObject<DataTable>(data); ;
            }
            else
            {
                return new DataTable();
            }
        }

        public DataTable Log(Exception error,SystemLogs log)
        {
            log.Details.Add(new SystemLogDetails{ActionDone=log.MethodName,Value= $"Exception Message:{error.Message}"});
            log.Details.Add(new SystemLogDetails { ActionDone = log.MethodName, Value = $"Inner Exception:{error.InnerException}" });
            log.Details.Add(new SystemLogDetails { ActionDone = log.MethodName, Value = $"Stack Trace:{error.StackTrace}" });

            byte[] user_model = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(log));
            var data = GetDataFromApiAsync($"{ConfigurationManager.AppSettings["BaseUrl"]}/ecira/LogAsync?data={Convert.ToBase64String(user_model)}").Result;
            //var parsedata = JsonConvert.DeserializeObject<DataTable>(data);
            if (data != null)
            {
                return JsonConvert.DeserializeObject<DataTable>(data); ;
            }
            else
            {
                return new DataTable();
            }
        }

    }
}
