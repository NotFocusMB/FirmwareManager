using System.ServiceProcess;

namespace FirmwareServer
{
    internal static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun = new ServiceBase[]
            {
                new FlashService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}