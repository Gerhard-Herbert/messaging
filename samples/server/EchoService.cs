using Aos.Messaging;

namespace server
{
    public class EchoService
    {
        public bool GotExitRequest { get; private set; }

        [Routing("echo")]
        public string Echo(string fromClient)
        {
            return fromClient;
        }

        [Routing("exit")]
        public void Exit()
        {
            GotExitRequest = true;
        }
    }
}