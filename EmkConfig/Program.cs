using System;
using System.Windows.Forms;
using Emk;
using Emk.Views;
using Newtonsoft.Json;

namespace EmkConfig
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new EmkMainForm());
            }
			catch (Exception e)
            {
                Log.Error(JsonConvert.SerializeObject(e));
            }
        }
	}
}
