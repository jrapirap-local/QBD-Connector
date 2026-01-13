using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using static MBPHReader.SQL.Meta;
using Newtonsoft.Json;
using static MBPH.Extension.Extensions;
using MBPHReader.Model;
using MBPH.Encryption;

namespace MBPHReader
{
    public static class Classes
    {
        //Post sample
        //public static async Task InsertSyncedAccounts(DataTable data)
        //{
        //    await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorInsertSyncedAccounts", JsonConvert.SerializeObject(data));
        //}

        //GET Sample
        //public static async Task<DataTable> GetBills(DataTable data) {

        //    byte[] user_model = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data).Decrypt());
        //    var mbph = await GetDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorGetBills?data={Convert.ToBase64String(user_model)}");
        //    if (!string.IsNullOrEmpty(mbph))
        //    {
        //        return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<DataTable>(mbph));

        //    }
        //    else
        //    {
        //        return new DataTable();
        //    }
        //}
        //.Encrypt();
        public static async Task<DataTable> Upload<T>(T data)//dynamic object nalang ng madali. any na. basta may company id goods na
        {
            var mbph = await GetDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorUploadClasses?data={data.ToBtoa()}");
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
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorSyncClasses", JsonConvert.SerializeObject(data));
        }
        public static async Task ResetClass(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorResetClasses", JsonConvert.SerializeObject(data));
        }
        public static async Task Download(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorDownloadClasses", JsonConvert.SerializeObject(data));
        }
        public static async Task DownloadDeleted(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorDownloadDeletedClasses", JsonConvert.SerializeObject(data));
        }
    }
}
