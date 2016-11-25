using Aos.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new PipeCommunicatorServer("testEchoService");
            var router = new Router(new Multiplexer(server));
            var echoService = new EchoService();
            router.AddAllRoutes(new ReflectionService(echoService));

            server.Start();
            while (!echoService.GotExitRequest)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
    }
}