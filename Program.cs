using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using FileTracking.Client.Common;

namespace FileTracking.Services
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (Constants.DiagnosticsDebugger)
                System.Diagnostics.Debugger.Launch();
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string pathToContentRoot = Path.GetDirectoryName(exePath);
            if (!Constants.IsService)
            {
                pathToContentRoot = Directory.GetCurrentDirectory();
            }
                var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(pathToContentRoot)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .UseUrls("http://+:" + Constants.ServerPort.ToString())
                .Build();
            if (Constants.IsService)
                host.RunAsCustomService();
            else
                host.Run();
        }
    }
}
