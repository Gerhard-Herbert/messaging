using Aos.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client
{
    class Program
    {
        const string pipeName = "testPipe";
        static void Main(string[] args)
        {
            var process = Process.Start("server.exe");

            // Register the echo service 
            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.RegisterService("echoService", "pipe://testEchoService");

            // Create the echo service proxy
            var serviceFactory = new ServiceInterfaceFactory(serviceLocator, new PipeChannelFactory());
            var echoService = serviceFactory.CreateServiceInterface("echoService");

            // Send some messages
            var request = Message.CreateRequest("echo");
            for (int i = 0; i < 10; i++)
            {
                request.Params = $"This is message number {i}";
                Log.Debug($"sending: {request.Params.ToString()}");
                echoService.Request(request, (r) => Log.Debug($"received: {r.Result}"), null);
            }

            // Tell the service to exit
            echoService.Request(Message.CreateRequest("exit"), null, null);
        }
    }
}