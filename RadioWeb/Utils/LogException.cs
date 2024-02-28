using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Utils
{
    public class LogException
    {
        public static string rutaLog = System.Configuration.ConfigurationManager.AppSettings["RutaAPILog"];

        public static void LogMessageToFile(string msg)
        {
            System.IO.StreamWriter sw = System.IO.File.AppendText(
               rutaLog + @"\excepciones\" + DateTime.Now.ToString("yyyyMMdd") + ".log");
            try
            {
                string logLine = System.String.Format(
                    "{0:G} -- {1}.", System.DateTime.Now, msg);
                sw.WriteLine(logLine);
            }
            finally
            {
                sw.Close();
            }
        }

        public static void LogMessageToFile(string filename, string msg)
        {
            System.IO.StreamWriter sw = System.IO.File.AppendText(
               rutaLog + @"\" + filename + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log");
            try
            {
                string logLine = System.String.Format(
                    "{0:G} -- {1}.", System.DateTime.Now, msg);
                sw.WriteLine(logLine);
            }
            finally
            {
                sw.Close();
            }
        }
    }
}