using MBPH.Encryption;
using MBPH.Extension;
using MBPH_Connector.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBPH_Connector.Abstraction
{
   public static class Session
    {
        public static USER_INFO UserSession { get; set; }
        public static void SessionWatcher(string clip)
        {
            try
            {
                string decodedString = clip.Decrypt(); //eto
                UserSession = JsonConvert.DeserializeObject<USER_INFO>(decodedString.apiToJSON());
            }
            catch { }
            finally { }
            
        }

    }
}
