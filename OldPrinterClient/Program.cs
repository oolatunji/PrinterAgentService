using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Runtime.InteropServices;

namespace printer_sdk
{
    class Program
    {
        /// <summary>
        /// return code for a successful execution
        /// </summary>
        private const int ReturnOk = 0;

        /// <summary>
        /// return code when a communication error happens
        /// </summary>
        private const int ReturnCommunicationError = 1;

        /// <summary>
        /// return code when a error happens during a writing operation
        /// </summary>
        private const int ReturnWriteError = 2;

        /// <summary>
        /// return code when a error happens during a reading operation
        /// </summary>
        private const int ReturnReadError = 3;

        /// <summary>
        /// return code when a memory error happens
        /// </summary>
        private const int ReturnMemoryError = 4;
        static void Main(string[] args)
        {
            //var list = PrinterGeneralInfo.GetPrinterList("Evolis", null);
            //list.ForEach(printer =>
            //{
            //    Console.WriteLine(printer);
            //});
            int returnCode = ReturnOk;

            IntPtr printerHandle = iomemdll.OpenPebble("Evolis Tattoo2 (Copy 1)");
            if (printerHandle == IntPtr.Zero)
            {
                Console.WriteLine("Error: communication with printer broken");
                Console.WriteLine("Press a key to continue ...");
                Console.ReadKey();
                returnCode = ReturnCommunicationError;
            }
            else
            {
                string escapeCommand = "\x1bRco;i\x0d";

                iomemdll.SetTimeout(10000);
                bool result = iomemdll.WritePebble(printerHandle, escapeCommand, escapeCommand.Length);

                if (!result)
                {
                    Console.WriteLine("Error: fail to write data to printer");
                    returnCode = ReturnWriteError;
                }
                else
                {
                    IntPtr printerAnswer = Marshal.AllocHGlobal(512);
                    if (printerAnswer.Equals(IntPtr.Zero))
                    {
                        returnCode = ReturnMemoryError;
                    }
                    else
                    {
                        uint answerSize = 0;

                        // Warning: the buffer size must be greater or equal to 512
                        result = iomemdll.ReadPebble(printerHandle, printerAnswer, 512, ref answerSize);
                        if (!result)
                        {
                            Console.WriteLine("Error: fail to read data from printer");
                            returnCode = ReturnReadError;
                        }
                        else
                        {
                            string answer = Marshal.PtrToStringAnsi(printerAnswer);
                            Console.Write(escapeCommand + ": ");
                            Console.WriteLine(answer);
                        }

                        Marshal.FreeHGlobal(printerAnswer);
                    }
                }

                iomemdll.ClosePebble(printerHandle);
                Console.WriteLine("Press a key to continue ...");
                Console.ReadKey();
            }

            Console.WriteLine(returnCode);
            Console.Read();
        }        
    }
}
