using Microsoft.Owin.Hosting;
using System;

namespace WindowsService1
{
    public class WindowsService
    {
        private readonly Configuration configuration;
        private IDisposable webApp;

        public WindowsService(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public void Start()
        {
            if (webApp == null)
            {
                var host = System.Diagnostics.Debugger.IsAttached ? "localhost" : "*";
                var baseAddress = $"http://{host}:{configuration.Port}/";
                
                webApp = WebApp.Start<OwinStartup>(baseAddress);
            }
        }
        public void Stop()
        {
            webApp?.Dispose();
        }
    }
}
