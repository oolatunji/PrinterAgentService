using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace PrinterAgentService
{
    public partial class PrinterAgentScheduler : ServiceBase
    {
        private Timer scheduler = null;

        private Timer echoScheduler = null;        

        public PrinterAgentScheduler()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            PASLibrary.ErrorHandler.WriteError(new Exception("Printer Agent Service Started."));

            Int32 timerInterval = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings.Get("FeedTimer"));

            bool sendEcho = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings.Get("SendEcho"));

            if (sendEcho)
            {
                echoScheduler = new Timer();
                this.echoScheduler.Interval = 10000;
                this.echoScheduler.Elapsed += new ElapsedEventHandler(this.send_Echo_Elapsed);
                this.echoScheduler.Enabled = true;
            }

            scheduler = new Timer();
            this.scheduler.Interval = 60000 * timerInterval;
            this.scheduler.Elapsed += new ElapsedEventHandler(this.scheduler_Elapsed);
            this.scheduler.Enabled = true;            
        }

        private void scheduler_Elapsed(object sender, ElapsedEventArgs e)
        {
            PASLibrary.PASWorkflow.SendRequest();
        }

        private void send_Echo_Elapsed(object sender, ElapsedEventArgs e)
        {
            PASLibrary.PASWorkflow.SendEcho();
        }

        protected override void OnStop()
        {
            scheduler.Enabled = false;

            bool sendEcho = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings.Get("SendEcho"));
            if (sendEcho)
            {
                echoScheduler.Enabled = false;
            }
            PASLibrary.ErrorHandler.WriteError(new Exception("Printer Agent Service Started."));
        }
    }
}
