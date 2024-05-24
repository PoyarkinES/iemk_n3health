using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Emk;
using Newtonsoft.Json;

namespace EmkWinService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            try
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new EmkWinSvc()
                };
                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception e)
            {
                Log.Error(JsonConvert.SerializeObject(e));
            }
        }
    }
}
