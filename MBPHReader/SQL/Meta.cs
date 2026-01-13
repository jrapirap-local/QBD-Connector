using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static MBPH.Extension.Extensions;
namespace MBPHReader.SQL
{
    public static class Meta
    {
        private static readonly HttpClient _httpClient = new HttpClient() {//API Request to MBPH
            BaseAddress = new Uri(GetConfig("BaseUrl"))
        };

        public static async Task<string> GetDataFromApiAsync(string apiUrl)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        public static async Task PostDataFromApiAsync(string apiUrl, string jsonData)
        {
            HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            
             HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

             if (response.IsSuccessStatusCode)
             {
                 string data = await response.Content.ReadAsStringAsync();
             }

           
        }



    }
}
