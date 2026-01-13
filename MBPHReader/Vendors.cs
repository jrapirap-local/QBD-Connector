using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MBPHReader.SQL.Meta;
using Newtonsoft.Json;
using static MBPH.Extension.Extensions;
using System.Data;
using MBPH.Encryption;

namespace MBPHReader
{
    public static class Vendors
    {
        //Reading of Vendors
        //Get Method x["ColumnName"]
        public static async Task<DataSet> Upload<T>(T data)//dynamic object nalang ng madali. any na. basta may company id goods na
        {
            var mbph = await GetDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorUploadVendors?data={data.ToBtoa()}");
            if (!string.IsNullOrEmpty(mbph))
            {
                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<DataSet>(mbph));
            }
            else
            {
                return new DataSet();
            }
        }
        public static async Task Download(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorDownloadVendors", JsonConvert.SerializeObject(data));
        }
        public static async Task DownloadAddr(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorDownloadVendorsAddr", JsonConvert.SerializeObject(data));
        }
        public static async Task Sync(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorSyncVendors", JsonConvert.SerializeObject(data));
        }
       

        // ADDED BY ADOME02082024
        public static async Task<DataSet> ValidateToSyncVendors<T>(T data)
        {
            var mbph = await GetDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorValidateToSyncVendors?data={data.ToBtoa()}");
            if (!string.IsNullOrEmpty(mbph))
            {
                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<DataSet>(mbph));
            }
            else
            {
                return new DataSet();
            }
        }
        public static async Task DownloadDeleted(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorDownloadDeletedVendors", JsonConvert.SerializeObject(data));
        }

        //public static async Task InsertSyncedVendorsLogs(DataTable data)
        //{
        //    await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorInsertSyncedVendorsLogs", JsonConvert.SerializeObject(data));
        //}
    }
}
