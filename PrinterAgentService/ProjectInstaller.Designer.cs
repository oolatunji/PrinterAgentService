namespace PrinterAgentService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.printerAgentServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.printerAgentServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // printerAgentServiceProcessInstaller
            // 
            this.printerAgentServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.printerAgentServiceProcessInstaller.Password = null;
            this.printerAgentServiceProcessInstaller.Username = null;
            // 
            // printerAgentServiceInstaller
            // 
            this.printerAgentServiceInstaller.ServiceName = "Printer Agent Service";
            this.printerAgentServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.printerAgentServiceProcessInstaller,
            this.printerAgentServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller printerAgentServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller printerAgentServiceInstaller;
    }
}