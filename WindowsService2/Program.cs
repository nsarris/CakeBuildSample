using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace WindowsService2
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = HostFactory.New(x =>
            {
                x.Service<WindowsService>(s =>
                {
                    s.ConstructUsing(name => {
                        var configuration = JsonConvert.DeserializeObject<Configuration>(System.IO.File.ReadAllText("config.json"));
                        return new WindowsService(configuration);
                    });
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalSystem();

                x.SetDescription("This as a test service");
                x.SetDisplayName("TestService #2");
                x.SetServiceName("TestService2");
            });

            var rc = host.Run();

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}
