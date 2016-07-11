// -----------------------------------------------------------------------
// <copyright file="iomemdll_x64.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace printer_sdk
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.InteropServices;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class iomemdll
    {
        /// <summary>
        /// Opens the pebble.
        /// </summary>
        /// <param name="printerName">Name of the printer.</param>
        /// <returns>true if the operation succeeded else false</returns>
        [DllImport("iomem.dll", EntryPoint = "OpenPebble", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern IntPtr OpenPebble(string printerName);

        /// <summary>
        /// Closes the pebble.
        /// </summary>
        /// <param name="printerHandle">The printer handle.</param>
        /// <returns>true if the operation succeeded else false</returns>
        [DllImport("iomem.dll", EntryPoint = "ClosePebble", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool ClosePebble(IntPtr printerHandle);

        /// <summary>
        /// Reads the pebble.
        /// </summary>
        /// <param name="printerHandle">The printer handle.</param>
        /// <param name="answer">The answer.</param>
        /// <param name="bufferSize">the size of the buffer</param>
        /// <param name="answerSize">Size of the answer.</param>
        /// <returns>true if the operation succeeded else false</returns>
        [DllImport("iomem.dll", EntryPoint = "ReadPebble", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool ReadPebble(IntPtr printerHandle, IntPtr answer, int bufferSize, ref uint answerSize);

        /// <summary>
        /// Writes the pebble.
        /// </summary>
        /// <param name="printerHandle">The printer handle.</param>
        /// <param name="escapeCommand">The escape command.</param>
        /// <param name="neededMore">if non negative then some data stay in the port buffer</param>
        /// <returns>true if the operation succeeded else false</returns>
        [DllImport("iomem.dll", EntryPoint = "WritePebble", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool WritePebble(IntPtr printerHandle, string escapeCommand, int neededMore);

        /// <summary>
        /// Gets the iomem version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns>true if the operation succeeded else false</returns>
        [DllImport("iomem.dll", EntryPoint = "GetIomemVersion", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool GetIomemVersion(IntPtr version);

        /// <summary>
        /// Gets the timeout.
        /// </summary>
        /// <returns>the timeout</returns>
        [DllImport("iomem.dll", EntryPoint = "GetTimeout", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int GetTimeout();

        /// <summary>
        /// Sets the timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>true if the operation succeeded else false</returns>
        [DllImport("iomem.dll", EntryPoint = "SetTimeout", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool SetTimeout(int timeout);

        /// <summary>
        /// Provide the status of the printer port.
        /// </summary>
        /// <param name="printerHandle">The printer handle.</param>
        /// <param name="answerSize">Size of the answer.</param>
        /// <returns>true if the printer has the status ready, else false</returns>
        /// <remarks>Return always true if the port is an Ethernet one.</remarks>
        [DllImport("iomem.dll", EntryPoint = "GetStatusEvo", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool GetStatusEvo(IntPtr printerHandle, ref uint answerSize);
    }
}
