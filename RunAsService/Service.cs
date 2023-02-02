using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace RunAsService
{
    ///<summary>
    /// defines a Windows Service starts a windows Process, defined by the appsettings.json file options
    ///</summary>
    public class Service : ServiceControl
    {
        Logger log = LogManager.GetCurrentClassLogger();
   
        // the wrapper service
        private Process MainService;
        // service options
        public string ExeName { get; }
        public string Arguments { get; }
        public string WorkingDir { get; }
        
        // ctors
        public Service() { }
        public Service(string exeName, string arguments, string workingDir)
        {
            this.ExeName = exeName;
            this.Arguments = arguments;
            this.WorkingDir = workingDir;
        }
        ///<summary>
        /// service start method: run the executable specified into the appsettings.json file inside this service
        ///</summary>
        public bool Start(HostControl hostControl)
        {
            log.Info("Service Starting...");
            RunProgram(ExeName, Arguments, WorkingDir, log);
            return true;
        }
        ///<summary>
        /// service stop method: kill the wrapped process 
        ///</summary>
        public bool Stop(HostControl hostControl)
        {
            log.Info("Service Stopping...");
            if (MainService != null)
                MainService.Kill(true);

            while (!MainService.HasExited)
                Task.Delay(250).Wait();

            log.Info($"Service exited with code: {MainService.ExitCode}");
            return true;
        }

        ///<summary>
        /// run a specified executable contained into the wrapper service
        ///</summary>
        private void RunProgram(string exeName, string arguments, string workingDir, ILogger log)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = exeName;
            info.Arguments = arguments;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.WorkingDirectory = workingDir;

            MainService = Process.Start(info);

            // redirect output incoming from stdout / stderr to the log file
            StringBuilder stderrBuilder = new StringBuilder();
            MainService.OutputDataReceived += (s, r) => log.Info($"{r.Data}");
            MainService.ErrorDataReceived += (s, r) => { log.Error($"{r.Data}"); stderrBuilder.Append(r.Data); };

            MainService.BeginErrorReadLine();
            MainService.BeginOutputReadLine();
        }
    }
}
