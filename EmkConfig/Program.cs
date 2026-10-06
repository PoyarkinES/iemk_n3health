using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace EmkConfig
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                using (var provider = Bootstrapper.BuildServiceProvider())
                using (var scope = provider.CreateScope())
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(ActivatorUtilities.CreateInstance<Forms.EmkMainForm>(scope.ServiceProvider));
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Ошибка запуска", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
