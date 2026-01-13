using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MBPH.Extension.Extensions;
using static MBPHReader.SQL.Meta;
using MBPH.Encryption;
using MBPHReader.Model;
namespace MBPHReader
{
    public static class Accounts
    {
        
        public static async Task<DataTable> Upload<T>(T data)//dynamic object nalang ng madali. any na. basta may company id goods na
        {
            var mbph = await GetDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorUploadAccounts?data={data.ToBtoa()}");
            if (!string.IsNullOrEmpty(mbph))
            {
                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<DataTable>(mbph));
            }
            else
            {
                return new DataTable();
            }
        }
        public static async Task Sync(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorSyncAccounts", JsonConvert.SerializeObject(data));
        }
        public static async Task Download(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorDownloadAccounts", JsonConvert.SerializeObject(data));
        }
        public static async Task DownloadDeleted(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorDownloadDeletedAccounts", JsonConvert.SerializeObject(data));
        }

    }
}
