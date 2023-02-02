//using log4net;
//using log4net.Config;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace RunAsService
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load Log4net configuration
            //var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            //XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            //ILog log = LogManager.GetLogger(nameof(Program));

            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(AppContext.BaseDirectory, "nlog.config"));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            //Task.Delay(10000).Wait();

            var program = configuration.GetSection("program").Value;
            var workingDir = configuration.GetSection("workingDir").Value;
            var arguments = configuration.GetSection("args").Value;
            var serviceName = configuration.GetSection("serviceName").Value;
            var displayName = configuration.GetSection("displayName").Value;
            var description = configuration.GetSection("description").Value;

            HostFactory.Run(x =>
            {
                //x.UseLog4Net();
                x.UseNLog();

                x.Service<Service>(s => {
                    s.ConstructUsing(name => new Service(program, arguments, workingDir));
                    s.WhenStarted((service, hostControl) => service.Start(hostControl));
                    s.WhenStopped((service, hostControl) => service.Stop(hostControl));
                });
                
                x.RunAsNetworkService();

                x.EnableServiceRecovery(r => r.RestartService(TimeSpan.FromMinutes(10)));

                x.SetServiceName(serviceName);
                x.SetDisplayName(displayName);
                x.SetDescription(description);
                x.StartAutomaticallyDelayed();

            });
        }
    }
}
