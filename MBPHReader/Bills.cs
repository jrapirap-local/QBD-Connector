using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MBPH.Extension.Extensions;
using static MBPHReader.SQL.Meta;
using Newtonsoft.Json;
using MBPH.Encryption;

namespace MBPHReader
{
    public static class Bills
    {
        public static async Task Log(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorLogBillsAndPayments", JsonConvert.SerializeObject(data));
        }
        public static async Task SyncDeleted(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorSyncDeletedBills", JsonConvert.SerializeObject(data));
        }
        public static async Task Sync(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorSyncBills", JsonConvert.SerializeObject(data));
        }
        public static async Task Download(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorDownloadBills", JsonConvert.SerializeObject(data));
        }

        public static async Task<DataTable> Upload<T>(T data)//dynamic object nalang ng madali. any na. basta may company id goods na
        {
            var mbph = await GetDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorUploadBills?data={data.ToBtoa()}");
            if (!string.IsNullOrEmpty(mbph))
            {
                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<DataTable>(mbph));
            }
            else
            {
                return new DataTable();
            }
        }

        public static async Task<DataTable> UploadDetails<T>(T data)//dynamic object nalang ng madali. any na. basta may company id goods na
        {
            var mbph = await GetDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorUploadBillDetails?data={data.ToBtoa()}");
            if (!string.IsNullOrEmpty(mbph))
            {
                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<DataTable>(mbph));
            }
            else
            {
                return new DataTable();
            }
        }

        public static async Task<DataTable> UploadDeletedBills<T>(T data)//dynamic object nalang ng madali. any na. basta may company id goods na
        {
            var mbph = await GetDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorUploadDeletedBills?data={data.ToBtoa()}");
            if (!string.IsNullOrEmpty(mbph))
            {
                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<DataTable>(mbph));
            }
            else
            {
                return new DataTable();
            }
        }
    }
}
