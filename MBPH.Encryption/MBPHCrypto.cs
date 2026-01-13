using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MBPH.Extension;
using Newtonsoft.Json;

namespace MBPH.Encryption
{
    /*
     using MBPH.Encryption;

    anySyting.Encrypt();
    encryptedString.Decrpt();
         */
    public static class MBPHCrypto
    {
        public static string ToBtoa(this string str)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
        }
        public static string ToBtoa<T>(this T model) //ADDED BY RBILO062724  
        {
            var str = JsonConvert.SerializeObject(model);
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
        }
        public static string ToAtob(this string base64str)
        {
            byte[] bytes = Convert.FromBase64String(base64str);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        public static string Encrypt(this string argPassword)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Extensions.GetConfig("CryptoKey"));
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(argPassword);
                        }
                        array = memoryStream.ToArray();
                        //Get encrypted password and save to database
                        return Convert.ToBase64String(array);
                        
                    }
                }
            }
       
       
       
       
       
       
       
       
       
       
       
       
       
       
       
       
       
        buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Extensions.GetConfig("CryptoKey"));
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                            
                        }
                    }
                }
            }
        }
    }
}
