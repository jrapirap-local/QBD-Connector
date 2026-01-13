using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data.Odbc;
using System.IO;

namespace MBPH.CMD
{
    /*
     using MBPH.CMD;
     bios.GetSerialNumber(); //

    cmd.Start("taskkill /im QBW.exe /f");
         */
    public static class cmd
    {
        public static void Start(string cmd)
        {
            Process process = new System.Diagnostics.Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = cmd;
            process.StartInfo = startInfo;
            process.Start();
        }
    }

    public static class odbc
    {
        public static string ReadOdbcServerName(string filePath)
        {
            // Replace 'your_config_file_path' with the actual path to your ODBC configuration file.
            string configFilePath = filePath;

            // Read the connection string from the configuration file
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                // Now you can read the file using the fileStream

                // Example: Read the contents of the file
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string fileContents = reader.ReadToEnd();
                    return fileContents.Split('\n')[5].Split('=')[1].Replace("\r", "");
                }
            }

        }
        private static string ReadOdbcConfigFile(string filePath)
        {
            try
            {
                // Read the contents of the configuration file
                string configText = System.IO.File.ReadAllText(filePath);

                // Assuming the configuration file contains only the connection string
                return configText.Trim();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading ODBC configuration file: {ex.Message}", ex);
            }
        }
    }
    public static class bios
    {
        public static string GetSerialNumber() {
            string serialNumber = GetBiosSerialNumber();

            if (!string.IsNullOrEmpty(serialNumber))
            {
                return serialNumber.Split(':').Last();
            }
            else
            {
             throw new Exception("Unable to retrieve BIOS serial number.");
            }
        }
        private static string GetBiosSerialNumber()
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "wmic",
                    Arguments = "bios get serialnumber",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // Extracting the serial number from the output
                    string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length >= 2)
                    {
                        // The serial number is usually on the second line
                        return lines[1].Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return null;
        }
    }
}
