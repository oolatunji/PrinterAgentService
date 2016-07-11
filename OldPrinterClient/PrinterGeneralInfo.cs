using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace OldPrinterClient
{
    public class PrinterGeneralInfo
    {
        public static List<string> GetPrinterList(string driverNameFilter, string onlineFilter)
        {
            List<string> result = new List<string>();

            ManagementScope scope = new ManagementScope(@"\root\cimv2");
            Console.WriteLine(scope);
            scope.Connect();

            Console.WriteLine("connected");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");

            foreach (ManagementObject o in searcher.Get())
            {
                if (o["Name"].ToString().ToLower().Contains("tat"))
                {
                    foreach (PropertyData prop in o.Properties)
                    {
                        Console.WriteLine("{0}: {1}", prop.Name, prop.Value);
                        var msg = string.Format("{0}: {1}", prop.Name, prop.Value);
                        ErrorHandler.WriteError(new Exception(msg));
                    }
                }
            }

            //foreach (ManagementObject o in searcher.Get())
            //{
            //    if (string.IsNullOrEmpty(onlineFilter) || (o["WorkOffline"].ToString().ToUpper() == onlineFilter))
            //    {
            //        if ((string.IsNullOrEmpty(driverNameFilter)) || (o["DriverName"].ToString().ToUpper().Contains(driverNameFilter.ToUpper())))
            //        {
            //            result.Add(o["Name"].ToString());
            //            Console.WriteLine("System Name: {0}", o["SystemName"].ToString());
            //        }
            //    }
            //}



            return result;
        }
    }
}
