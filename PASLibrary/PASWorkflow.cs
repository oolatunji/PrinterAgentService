using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientRuntime;
using ClientRuntime.EspfRequest;
using System.Runtime.InteropServices;

namespace PASLibrary
{
    public static class PASWorkflow
    {                
        public static void SendEcho()
        {
            try
            {
                PrinterHandler.SetParameters();
                var echoRequest = new EspfEcho();
                echoRequest.id = "ECHO10";
                echoRequest.jsonrpc = "2.0";
                echoRequest.method = "ECHO.Echo";
                echoRequest.parameters = new EspfParamsEcho { data = "Olatunji Toni" };

                var request = SerializationTools.jsonSerialize(echoRequest, echoRequest.GetType());

                var response = EspfClientProcessor.SendRequest(BusinessHelper.CommProcType, BusinessHelper.IP, BusinessHelper.Port, request);
                var responsetype = new EspfResponse().GetType();
                var formattedresponse = (EspfResponse)SerializationTools.jsonDeserialize(response, responsetype);
                var responseid = formattedresponse.id;
                var responsejsonrpc = formattedresponse.jsonrpc;
                var responseresult = formattedresponse.result;
                var responseerror = formattedresponse.error != null ? formattedresponse.error.message : string.Empty;

                var message = string.Format("Echo sent with the following response: id: {0}, jsonrpc: {1}, result: {2}, error: {3}", responseid, responsejsonrpc, responseresult, responseerror);
                ErrorHandler.WriteError(new Exception(message));
            }
            catch (Exception ex)
            {
                ErrorHandler.WriteError(ex);
            }
        }

        public static void SendRequest()
        {
            try
            {                
                PrinterHandler.SetParameters();

                #region New drivers list
                var drivers = PrinterHandler.Drivers();
                #endregion

                #region get states of printer devices under the supervision of Evolis Services Provider Framework-ESPF                
                drivers.ForEach(driver =>
                    {
                        #region get device names
                        var deviceName = GetDeviceName(driver);
                        Console.WriteLine("Device: {0}", deviceName);
                        #endregion

                        #region get device state
                        if (!string.IsNullOrEmpty(deviceName))
                        {
                            if (!deviceName.Contains(";"))
                            {
                                GetState(deviceName);
                            }
                            else
                            {
                                var deviceNames = deviceName.Split(';');
                                deviceNames.ToList().ForEach(device =>
                                    {
                                        GetState(device);
                                    });
                            }
                        }
                        else
                        {
                            ErrorHandler.WriteError(new Exception("No device name found for driver: " + driver));
                        }
                        #endregion
                    });
                #endregion

                #region Tattoo2 drivers list
                var tattoo2Drivers = PrinterHandler.GetTattoo2Printers();
                #endregion

                #region read the serial number, type and average card print of the Tattoo2 printer
                if (tattoo2Drivers.Any())
                {
                    tattoo2Drivers.ForEach(tattoo2Driver =>
                    {
                        var online = GetTattoo2OnlineState(tattoo2Driver);
                        if (online)
                        {
                            var printerSerialNumber = GetTattoo2SerialNumber(tattoo2Driver);
                            var printerType = GetTattoo2Type(tattoo2Driver);
                            var printerNoofCards = GetTattoo2NumberofCards(tattoo2Driver);

                            var server = new PrinterMonitoringServer.FLSSolution();

                            var response = server.SendLatestPrinterFeeds(printerSerialNumber, printerSerialNumber, 0, Convert.ToInt32(printerNoofCards), true, printerType);

                            if (response.Successful)
                            {
                                ErrorHandler.WriteError(new Exception(string.Format("Success: {0}", printerSerialNumber)));
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(response.ErrMessage))
                                {
                                    ErrorHandler.WriteError(new Exception(string.Format("Failed: {0} with error {1}", printerSerialNumber, response.ErrMessage)));
                                }
                                else
                                {
                                    ErrorHandler.WriteError(new Exception(string.Format("Failed: {0} with no reported error", printerSerialNumber)));
                                }
                            }
                        }
                    });
                }
                #endregion
            }
            catch (Exception ex)
            {
                ErrorHandler.WriteError(ex);
            }
        }        

        private static void GetState(string deviceName)
        {
            try
            {
                #region get serial number
                var serialNumber = GetSerialNumber(deviceName);
                #endregion

                if (!string.IsNullOrEmpty(serialNumber))
                {
                    #region get online state
                    var state = GetOnlineState(deviceName);
                    if (state == StatusUtil.OnlineStatus.Ready || state == StatusUtil.OnlineStatus.Warning)
                    {
                        //Get Ribbon Count
                        var ribbonCount = GetRibbonCount(deviceName);
                        //Get Number of Cards Printed
                        var noofCards = GetNumberofCardsPrinted(deviceName);

                        var printerType = GetPrinterType(deviceName);

                        var server = new PrinterMonitoringServer.FLSSolution();

                        var response = server.SendLatestPrinterFeeds(serialNumber, serialNumber, Convert.ToInt32(ribbonCount), Convert.ToInt32(noofCards), true, printerType);

                        if (response.Successful)
                        {
                            ErrorHandler.WriteError(new Exception(string.Format("Success: {0}", serialNumber)));
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(response.ErrMessage))
                            {
                                ErrorHandler.WriteError(new Exception(string.Format("Failed: {0} with error {1}", serialNumber, response.ErrMessage)));
                            }
                            else
                            {
                                ErrorHandler.WriteError(new Exception(string.Format("Failed: {0} with no reported error", serialNumber)));
                            }
                        }
                    }
                    else if (state == StatusUtil.OnlineStatus.Off)
                    {
                        // send details
                        var server = new PrinterMonitoringServer.FLSSolution();

                        var response = server.SendLatestPrinterFeeds(serialNumber, serialNumber, 0, 0, false, string.Empty);

                        if (response.Successful)
                        {
                            ErrorHandler.WriteError(new Exception(string.Format("Success: {0}", serialNumber)));
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(response.ErrMessage))
                            {
                                ErrorHandler.WriteError(new Exception(string.Format("Failed: {0} with error {1}", serialNumber, response.ErrMessage)));
                            }
                            else
                            {
                                ErrorHandler.WriteError(new Exception(string.Format("Failed: {0} with no reported error", serialNumber)));
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    ErrorHandler.WriteError(new Exception(string.Format("No serial number found for device: {0}", deviceName)));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string GetDeviceName(string driver)
        {
            try
            {
                var supervisionRequest = new EspfSupervision();
                supervisionRequest.id = "220";
                supervisionRequest.jsonrpc = "2.0";
                supervisionRequest.method = "SUPERVISION.List";
                supervisionRequest.parameters = new EspfParamsSupervision { level = "0", device = driver };

                var request = SerializationTools.jsonSerialize(supervisionRequest, supervisionRequest.GetType());

                var response = EspfClientProcessor.SendRequest(BusinessHelper.CommProcType, BusinessHelper.IP, BusinessHelper.Port, request);
                
                var responsetype = new EspfResponse().GetType();
                var formattedresponse = (EspfResponse)SerializationTools.jsonDeserialize(response, responsetype);
                var responseid = formattedresponse.id;
                var responsejsonrpc = formattedresponse.jsonrpc;
                var responseresult = formattedresponse.result;
                var responseerror = formattedresponse.error;

                if (responseerror != null)
                {
                    var message = string.Format("Error response with: id: {0}, jsonrpc: {1}, result: {2}, error: {3}, device: {4}", responseid, responsejsonrpc, responseresult, responseerror, driver);
                    ErrorHandler.WriteError(new Exception(formattedresponse.error.message));
                    return responseresult;
                }
                else
                {
                    ErrorHandler.WriteError(new Exception(string.Format("No response to command: SUPERVISION.List for driver: {0}", driver)));
                }

                return responseresult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string GetSerialNumber(string deviceName)
        {
            try
            {
                var cmdRequest = new EspfCmd();
                cmdRequest.id = "CMD42";
                cmdRequest.jsonrpc = "2.0";
                cmdRequest.method = "CMD.SendCommand";
                cmdRequest.parameters = new EspfParamsCmd { command = "Rsn", device = deviceName, timeout = "5000" };

                var request = SerializationTools.jsonSerialize(cmdRequest, cmdRequest.GetType());

                var response = EspfClientProcessor.SendRequest(BusinessHelper.CommProcType, BusinessHelper.IP, BusinessHelper.Port, request);

                var responsetype = new EspfResponse().GetType();
                var formattedresponse = (EspfResponse)SerializationTools.jsonDeserialize(response, responsetype);
                var responseid = formattedresponse.id;
                var responsejsonrpc = formattedresponse.jsonrpc;
                var responseresult = formattedresponse.result;
                var responseerror = formattedresponse.error;

                if (responseerror != null)
                {
                    var message = string.Format("Error response with: id: {0}, jsonrpc: {1}, result: {2}, error: {3}, device: {4}", responseid, responsejsonrpc, responseresult, responseerror, deviceName);
                    ErrorHandler.WriteError(new Exception(formattedresponse.error.message));
                    return responseresult;
                }

                return responseresult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static StatusUtil.OnlineStatus GetOnlineState(string driver)
        {
            try
            {
                var stateRequest = new EspfSupervision();
                stateRequest.id = "1";
                stateRequest.jsonrpc = "2.0";
                stateRequest.method = "SUPERVISION.GetState";
                stateRequest.parameters = new EspfParamsSupervision { device = driver };

                var request = SerializationTools.jsonSerialize(stateRequest, stateRequest.GetType());

                var response = EspfClientProcessor.SendRequest(BusinessHelper.CommProcType, BusinessHelper.IP, BusinessHelper.Port, request);

                var responsetype = new EspfResponse().GetType();
                var formattedresponse = (EspfResponse)SerializationTools.jsonDeserialize(response, responsetype);
                var responseid = formattedresponse.id;
                var responsejsonrpc = formattedresponse.jsonrpc;
                var responseresult = formattedresponse.result;
                var responseerror = formattedresponse.error;

                Console.WriteLine("State: {0}", responseresult);

                if (responseerror != null)
                {
                    var message = string.Format("Error response with: id: {0}, jsonrpc: {1}, result: {2}, error: {3}, device: {4}", responseid, responsejsonrpc, responseresult, responseerror, driver);
                    ErrorHandler.WriteError(new Exception(formattedresponse.error.message));
                    return StatusUtil.OnlineStatus.Error;
                }

                responseresult = responseresult.Split(',')[0];
                return responseresult == "READY" ? StatusUtil.OnlineStatus.Ready : responseresult == "WARNING" ? StatusUtil.OnlineStatus.Warning : StatusUtil.OnlineStatus.Off;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string GetRibbonCount(string deviceName)
        {
            try
            {
                var cmdRequest = new EspfCmd();
                cmdRequest.id = "CMD42";
                cmdRequest.jsonrpc = "2.0";
                cmdRequest.method = "CMD.SendCommand";
                cmdRequest.parameters = new EspfParamsCmd { command = "Rrt;count", device = deviceName, timeout = "5000" };

                var request = SerializationTools.jsonSerialize(cmdRequest, cmdRequest.GetType());

                var response = EspfClientProcessor.SendRequest(BusinessHelper.CommProcType, BusinessHelper.IP, BusinessHelper.Port, request);

                var responsetype = new EspfResponse().GetType();
                var formattedresponse = (EspfResponse)SerializationTools.jsonDeserialize(response, responsetype);
                var responseid = formattedresponse.id;
                var responsejsonrpc = formattedresponse.jsonrpc;
                var responseresult = formattedresponse.result;
                var responseerror = formattedresponse.error;

                if (responseerror != null)
                {
                    var message = string.Format("Error response with: id: {0}, jsonrpc: {1}, result: {2}, error: {3}, device: {4}", responseid, responsejsonrpc, responseresult, responseerror, deviceName);
                    ErrorHandler.WriteError(new Exception(formattedresponse.error.message));
                    return responseresult;
                }

                return responseresult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string GetNumberofCardsPrinted(string deviceName)
        {
            try
            {
                var cmdRequest = new EspfCmd();
                cmdRequest.id = "CMD42";
                cmdRequest.jsonrpc = "2.0";
                cmdRequest.method = "CMD.SendCommand";
                cmdRequest.parameters = new EspfParamsCmd { command = "Rco;i", device = deviceName, timeout = "5000" };

                var request = SerializationTools.jsonSerialize(cmdRequest, cmdRequest.GetType());

                var response = EspfClientProcessor.SendRequest(BusinessHelper.CommProcType, BusinessHelper.IP, BusinessHelper.Port, request);

                var responsetype = new EspfResponse().GetType();
                var formattedresponse = (EspfResponse)SerializationTools.jsonDeserialize(response, responsetype);
                var responseid = formattedresponse.id;
                var responsejsonrpc = formattedresponse.jsonrpc;
                var responseresult = formattedresponse.result;
                var responseerror = formattedresponse.error;

                if (responseerror != null)
                {
                    var message = string.Format("Error response with: id: {0}, jsonrpc: {1}, result: {2}, error: {3}, device: {4}", responseid, responsejsonrpc, responseresult, responseerror, deviceName);
                    ErrorHandler.WriteError(new Exception(formattedresponse.error.message));
                    return responseresult;
                }

                return responseresult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string GetPrinterType(string deviceName)
        {
            try
            {
                var cmdRequest = new EspfCmd();
                cmdRequest.id = "CMD42";
                cmdRequest.jsonrpc = "2.0";
                cmdRequest.method = "CMD.SendCommand";
                cmdRequest.parameters = new EspfParamsCmd { command = "Rtp", device = deviceName, timeout = "5000" };

                var request = SerializationTools.jsonSerialize(cmdRequest, cmdRequest.GetType());

                var response = EspfClientProcessor.SendRequest(BusinessHelper.CommProcType, BusinessHelper.IP, BusinessHelper.Port, request);

                var responsetype = new EspfResponse().GetType();
                var formattedresponse = (EspfResponse)SerializationTools.jsonDeserialize(response, responsetype);
                var responseid = formattedresponse.id;
                var responsejsonrpc = formattedresponse.jsonrpc;
                var responseresult = formattedresponse.result;
                var responseerror = formattedresponse.error;

                if (responseerror != null)
                {
                    var message = string.Format("Error response with: id: {0}, jsonrpc: {1}, result: {2}, error: {3}, device: {4}", responseid, responsejsonrpc, responseresult, responseerror, deviceName);
                    ErrorHandler.WriteError(new Exception(formattedresponse.error.message));
                    return responseresult;
                }

                return responseresult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static bool GetTattoo2OnlineState(string driver)
        {
            try
            {
                var sn = false;

                IntPtr printerHandle = IomemHandler.OpenPebble(driver);
                if (printerHandle == IntPtr.Zero)
                {
                    ErrorHandler.WriteError(new Exception(string.Format("Error: communication with printer: {0} broken", driver)));
                }
                else
                {
                    sn = true;
                    IomemHandler.ClosePebble(printerHandle);
                }
                return sn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string GetTattoo2SerialNumber(string driver)
        {
            try
            {
                var sn = string.Empty;

                IntPtr printerHandle = IomemHandler.OpenPebble(driver);
                if (printerHandle == IntPtr.Zero)
                {
                    ErrorHandler.WriteError(new Exception(string.Format("Error: communication with printer: {0} broken", driver)));
                }
                else
                {
                    string escapeCommand = "\x1bRsn\x0d";

                    IomemHandler.SetTimeout(5000);
                    bool result = IomemHandler.WritePebble(printerHandle, escapeCommand, escapeCommand.Length);

                    if (!result)
                    {
                        ErrorHandler.WriteError(new Exception(string.Format("Error: fail to write data to printer: {0}", driver)));
                    }
                    else
                    {
                        IntPtr printerAnswer = Marshal.AllocHGlobal(512);
                        if (printerAnswer.Equals(IntPtr.Zero))
                        {
                            ErrorHandler.WriteError(new Exception(string.Format("Memory error response obtained for printer: {0}", driver)));
                        }
                        else
                        {
                            uint answerSize = 0;

                            // Warning: the buffer size must be greater or equal to 512
                            result = IomemHandler.ReadPebble(printerHandle, printerAnswer, 512, ref answerSize);
                            if (!result)
                            {
                                ErrorHandler.WriteError(new Exception(string.Format("Error: fail to read data from printer: {0}", driver)));
                            }
                            else
                            {
                                string answer = Marshal.PtrToStringAnsi(printerAnswer);
                                sn = answer;
                            }

                            Marshal.FreeHGlobal(printerAnswer);
                        }
                    }

                    IomemHandler.ClosePebble(printerHandle);
                }
                return sn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string GetTattoo2Type(string driver)
        {
            try
            {
                var sn = string.Empty;

                IntPtr printerHandle = IomemHandler.OpenPebble(driver);
                if (printerHandle == IntPtr.Zero)
                {
                    ErrorHandler.WriteError(new Exception(string.Format("Error: communication with printer: {0} broken", driver)));
                }
                else
                {
                    string escapeCommand = "\x1bRtp\x0d";

                    IomemHandler.SetTimeout(5000);
                    bool result = IomemHandler.WritePebble(printerHandle, escapeCommand, escapeCommand.Length);

                    if (!result)
                    {
                        ErrorHandler.WriteError(new Exception(string.Format("Error: fail to write data to printer: {0}", driver)));
                    }
                    else
                    {
                        IntPtr printerAnswer = Marshal.AllocHGlobal(512);
                        if (printerAnswer.Equals(IntPtr.Zero))
                        {
                            ErrorHandler.WriteError(new Exception(string.Format("Memory error response obtained for printer: {0}", driver)));
                        }
                        else
                        {
                            uint answerSize = 0;

                            // Warning: the buffer size must be greater or equal to 512
                            result = IomemHandler.ReadPebble(printerHandle, printerAnswer, 512, ref answerSize);
                            if (!result)
                            {
                                ErrorHandler.WriteError(new Exception(string.Format("Error: fail to read data from printer: {0}", driver)));
                            }
                            else
                            {
                                string answer = Marshal.PtrToStringAnsi(printerAnswer);
                                sn = answer;
                            }

                            Marshal.FreeHGlobal(printerAnswer);
                        }
                    }

                    IomemHandler.ClosePebble(printerHandle);
                }
                return sn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string GetTattoo2NumberofCards(string driver)
        {
            try
            {
                var sn = string.Empty;

                IntPtr printerHandle = IomemHandler.OpenPebble(driver);
                if (printerHandle == IntPtr.Zero)
                {
                    ErrorHandler.WriteError(new Exception(string.Format("Error: communication with printer: {0} broken", driver)));
                }
                else
                {
                    string escapeCommand = "\x1bRco;i\x0d";

                    IomemHandler.SetTimeout(5000);
                    bool result = IomemHandler.WritePebble(printerHandle, escapeCommand, escapeCommand.Length);

                    if (!result)
                    {
                        ErrorHandler.WriteError(new Exception(string.Format("Error: fail to write data to printer: {0}", driver)));
                    }
                    else
                    {
                        IntPtr printerAnswer = Marshal.AllocHGlobal(512);
                        if (printerAnswer.Equals(IntPtr.Zero))
                        {
                            ErrorHandler.WriteError(new Exception(string.Format("Memory error response obtained for printer: {0}", driver)));
                        }
                        else
                        {
                            uint answerSize = 0;

                            // Warning: the buffer size must be greater or equal to 512
                            result = IomemHandler.ReadPebble(printerHandle, printerAnswer, 512, ref answerSize);
                            if (!result)
                            {
                                ErrorHandler.WriteError(new Exception(string.Format("Error: fail to read data from printer: {0}", driver)));
                            }
                            else
                            {
                                string answer = Marshal.PtrToStringAnsi(printerAnswer);
                                sn = answer;
                            }

                            Marshal.FreeHGlobal(printerAnswer);
                        }
                    }

                    IomemHandler.ClosePebble(printerHandle);
                }
                return sn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
