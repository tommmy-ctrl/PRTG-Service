using System.ServiceProcess;
using PRTGService;

namespace PRTGService
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new PRTGService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
