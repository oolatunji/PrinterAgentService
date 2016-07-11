using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using ClientRuntime;

namespace PASLibrary
{
    public class PrinterHandler
    {
        public static void SetParameters()
        {
            var ip = System.Configuration.ConfigurationManager.AppSettings.Get("ip");
            var port = System.Configuration.ConfigurationManager.AppSettings.Get("port");
            var commtype = System.Configuration.ConfigurationManager.AppSettings.Get("commtype");
            var servername = System.Configuration.ConfigurationManager.AppSettings.Get("piperoot");
            var pipename = System.Configuration.ConfigurationManager.AppSettings.Get("pipename");
            var model = System.Configuration.ConfigurationManager.AppSettings.Get("model");
            BusinessHelper.Model = model;
            BusinessHelper.IP = commtype == "PIPE" ? servername : ip;
            BusinessHelper.Port = commtype == "PIPE" ? pipename : port;
            BusinessHelper.CommType = commtype;
        }

        public static List<string> Drivers()
        {
            var drivers = new List<string>();
            drivers.Add("Evolis Primacy");
            drivers.Add("Evolis Elypso");
            drivers.Add("Evolis Zenius");
            drivers.Add("Evolis KC200");

            return drivers;
        }

        public static List<string> GetTattoo2Printers()
        {
            List<string> result = new List<string>();

            ManagementScope scope = new ManagementScope(@"\root\cimv2");

            ErrorHandler.WriteError(new Exception("Access to printer scope started"));

            scope.Connect();

            ErrorHandler.WriteError(new Exception("Connected to scope"));

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");

            foreach (ManagementObject o in searcher.Get())
            {
                if (o["Name"] != null && o["Name"].ToString().ToLower().Contains("tattoo"))
                {
                    var msg = string.Format("Printer {0} Found", o["Name"].ToString());
                    ErrorHandler.WriteError(new Exception(msg));
                    result.Add(o["Name"].ToString());
                }                
            }

            if (!result.Any())
            {
                ErrorHandler.WriteError(new Exception("No Tattoo2 printer found"));
            }

            return result;
        }
    }
}
