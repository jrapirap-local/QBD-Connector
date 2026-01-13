using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MBPHReader.SQL.Meta;
using Newtonsoft.Json;
using static MBPH.Extension.Extensions;
using MBPHReader.Model;
using System.Data;
using MBPH.Encryption;
namespace MBPHReader
{
    public static class Payments
    {
        public static async Task Log(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorLogBillsAndPayments", JsonConvert.SerializeObject(data));
        }
        public static async Task VoidPayment(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorSyncVoidPayment", JsonConvert.SerializeObject(data));
        }
        public static async Task Sync(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorSyncPayments", JsonConvert.SerializeObject(data));
        }

        public static async Task Download(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorDownloadPayments", JsonConvert.SerializeObject(data));
        }
        public static async Task DownloadVoidedPayment(DataTable data)
        {
            await PostDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorDownloadVoidedPayments", JsonConvert.SerializeObject(data));
        }

        public static async Task<DataTable> GetPayments<T>(T data)//dynamic object nalang ng madali. any na. basta may company id goods na
        {
            var mbph = await GetDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorGetPayments?data={data.ToBtoa()}");
            if (!string.IsNullOrEmpty(mbph))
            {
                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<DataTable>(mbph));
            }
            else
            {
                return new DataTable();
            }
        }

        public static async Task<DataTable> GetPaymentDetails<T>(T data)
        {

            var mbph = await GetDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorGetPaymentDetails?data={data.ToBtoa()}");
            if (!string.IsNullOrEmpty(mbph))
            {
                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<DataTable>(mbph));

            }
            else
            {
                return new DataTable();
            }
        }

        //public static async Task<DataTable> UploadDetails<T>(T data)//dynamic object nalang ng madali. any na. basta may company id goods na
        //{
        //    var mbph = await GetDataFromApiAsync($"{GetConfig("BaseUrl")}/ConnectorUploadBillDetails?data={data.ToBtoa()}");
        //    if (!string.IsNullOrEmpty(mbph))
        //    {
        //        return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<DataTable>(mbph));
        //    }
        //    else
        //    {
        //        return new DataTable();
        //    }
        //}


    }
}
