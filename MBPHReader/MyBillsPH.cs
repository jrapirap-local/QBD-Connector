using MBPHReader.Model;
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
    public static class MyBillsPH
    {
        //RRIEL01302025
        public static async Task LogError(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorLogErrorBillsAndPayments", JsonConvert.SerializeObject(data));
        }
        public static async Task LogCustomError(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorLogErrorCustom", JsonConvert.SerializeObject(data));
        }
        public static async Task Increment(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorIncrement", JsonConvert.SerializeObject(data));
        }
        public static async Task DoneSyncing(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorDoneSyncing", JsonConvert.SerializeObject(data));
        }
        public static async Task DoneDownload<T>(T data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorDoneDownload", JsonConvert.SerializeObject(data));
        }
        public static async Task EndSync<T>(T data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorEndSync", JsonConvert.SerializeObject(data));
        }
        public static async Task EndSyncLists<T>(T data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorEndSyncLists", JsonConvert.SerializeObject(data));
        }
        public static async Task EndSyncBillsAndPayments<T>(T data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorEndSyncBillsAndPayments", JsonConvert.SerializeObject(data));
        }
        public static async Task PushMessage(Parameters data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorPushMessage", JsonConvert.SerializeObject(data));
        }
        public static async Task UpdateSyncGuid(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorUpdateSyncGuid", JsonConvert.SerializeObject(data));
        }
       
        public static async Task SyncHistory(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorSyncHistory", JsonConvert.SerializeObject(data));
        }

        public static async Task BindQBDCompany(Parameters data)
        {
            
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorBindQBDCompany", JsonConvert.SerializeObject(data));
            
        }
        public static async Task<DataTable> ValidateCompanyAccess(DataTable data)
        {

            byte[] user_model = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            var mbph = await GetDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorValidateCompanyAccess?data={Convert.ToBase64String(user_model)}");
            if (!string.IsNullOrEmpty(mbph))
            {
                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<DataTable>(mbph));

            }
            else
            {
                return new DataTable();
            }

        }

        public static async Task<DataTable> GetEcira<T>(T data)//dynamic object nalang ng madali. any na. basta may company id goods na
        {
            var mbph = await GetDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorGetEcira?data={data.ToBtoa()}");
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
