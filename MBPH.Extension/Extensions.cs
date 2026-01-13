using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Reflection;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace MBPH.Extension
{
    /*
     * 
     marami parameter
     Sql.ExecuteNonQuery("SP_Name",Model.ModelToSqlParamater());
        Enumerable.Action(x=>{//lambda expression. Read only. no memory. parallel. 10x faster// no garbage
         
        });
        .Action(x=>{ // void
            //ubusin nya. no return
        });


        Array.All(x=>{
            if(val>0)
                return true; // proceed to the next item
            else
                return false; // break the loop.
        });
         */
    public static class Extensions
    {
        public static StringBuilder logText = new StringBuilder();
        public static string GenerateNumber()
        {
            Random random = new Random();
            HashSet<string> generatedNumbers = new HashSet<string>();

            while (true)
            {
                // Generate a random 2-digit number
                int twoDigitNumber = random.Next(10, 100);

                // Generate a random 10-digit number
                long tenDigitNumber = random.Next(1_000_000_000, 2_000_000_000);

                // Format the numbers as ##-##########
                string formattedNumber = $"{twoDigitNumber:00}-{tenDigitNumber:0000000000}";

                // Check if the number is unique
                if (generatedNumbers.Add(formattedNumber))
                {
                    return formattedNumber;
                }
            }
        }
        public static void Action<T>(this IEnumerable<T> Lists, Action<T> actions) {
            if(Lists !=null)
            foreach (var l in Lists) {
                actions(l);
            }
        }

        public static void Action<T>(this IQueryable<T> Lists, Action<T> actions)
        {
            foreach (var l in Lists)
            {
                actions(l);
            }
        }

        public static void Action(this DataTable Lists, Action<DataRow> actions)
        {
            foreach (DataRow l in Lists.Rows)
            {
                actions(l);
            }
        }

        public static string GetConfig(string config)
        {
            return ConfigurationManager.AppSettings[config];
        }
        public static string GetConnectionString(string config)
        {
            return ConfigurationManager.ConnectionStrings[config].ConnectionString;
        }

        public static string apiToJSON(this string s) {
            if (!string.IsNullOrEmpty(s))
            {
                s = s.Replace("\"{", "{");
                s = s.Replace("}\"", "}");
                s = s.Replace("\"[", "[");
                s = s.Replace("]\"", "]");
                s = s.Replace("\\\"", "\"");

            }
            return s;
        }
        public static DataTable ModelToDataTable<T>(this T model) {
            DataTable dataTable = new DataTable();
            if (model != null) {
                Type type = typeof(T);
                PropertyInfo[] properties = type.GetProperties();
                DataRow newRow = dataTable.NewRow();
                foreach (PropertyInfo property in properties)
                {
                    dataTable.Columns.Add(property.Name);
                    newRow[$"{property.Name}"] = property.GetValue(model);
                }
                dataTable.Rows.Add(newRow);
            }
            return dataTable;
        }
        public static DataTable ModelToDataTable(this DataRow model)
        {
            DataTable dataTable = new DataTable();
            if (model != null)
            {
                foreach (DataColumn column in model.Table.Columns)
                {
                    dataTable.Columns.Add(column.ColumnName, column.DataType);
                }

                // Add the DataRow to the DataTable
                DataRow newRow = dataTable.NewRow();
                newRow.ItemArray = model.ItemArray; // Copy data from original DataRow
                dataTable.Rows.Add(newRow);
            }
            return dataTable;
        }
        public static string ReplaceQoutedMessage(this string s)
        {
            string pattern = "\"(.*?)\"";
            string replacedMessage = Regex.Replace(s, pattern, string.Empty);
            return replacedMessage;
        }
        public static SqlParameter[] ModelToSQLParameter<T>(this T model)
        {
            if (model!=null) {
                List<SqlParameter> param = new List<SqlParameter>();
                Type type = typeof(T);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    var propValue = property.GetValue(model);
                    if (propValue != null)
                    {
                        var newParam = new SqlParameter($"@{property.Name}", propValue);
                        param.Add(newParam);
                    }
                }
                return param.ToArray();
            }
            return null;
        }


        public static string GetFileID(string path)
        {
            string filePath = @"C:\Users\Public\Documents\Intuit\QuickBooks\Company Files\TechLabs.qbw";
            

            // Specify the fsutil command to query file ID
            string command = $"fsutil file queryfileid \"{filePath}\"";

            // Create a process to run the command
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {command}";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            // Event handler to capture output asynchronously
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);

            // Start the process
            process.Start();

            // Begin asynchronously reading the output
            process.BeginOutputReadLine();

            // Wait for the process to exit
            process.WaitForExit();

            // Close the process
            process.Close();
            return process.StandardOutput.ToString();
        }
    }
}
