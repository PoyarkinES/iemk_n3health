using Emk;
using Emk.Models;
using Emk.Services;
using System;
using System.Linq;
using System.ServiceProcess;
using System.Timers;

namespace EmkWinService
{
	public partial class EmkWinSvc : ServiceBase
	{
		private Timer _timer;
		private bool _isRunning;
		private EmkSettings _settings;
		private DateTime _lastStart;
		public EmkWinSvc()
		{
			InitializeComponent();
			_timer = new Timer(60000);
			_timer.Elapsed += _timer_Elapsed;
			_lastStart = DateTime.MinValue;
		}

		private void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if(!ShouldStart())
				return;
			_lastStart = DateTime.Now;
            Log.Info("Отправка данных запущена");
			var s = new EmkSendingService(true);
            _isRunning = true;
            s.Run();
			_isRunning = false;
		}

		protected override void OnStart(string[] args)
		{
			Log.Info("Запуск службы...");
			_settings = Factory.LoadSettings().First();
			Log.Info(_settings.ToString());
			_timer.AutoReset = true;
			_timer.Start();
		}

		protected override void OnStop()
		{
			Log.Info("Останавливаю службу...");
			_timer.Stop();
			_timer.Dispose();
		}

		private bool ShouldStart()
		{
			if (_isRunning)
				return false;
			if(_settings.UpdateTime == TimeSpan.Zero) {
				Log.Error($"Не удалось распознать значение времени {_settings.UpdateTime}");
				throw new ArgumentNullException("UpdateTime", $"Не удалось распознать значение времени {_settings.UpdateTime}");
			}
			if (DateTime.Now.TimeOfDay >= _settings.UpdateTime) {
                if(_lastStart.Date < DateTime.Now.Date)
                    return true;
			}
            
			return false;
		}

	}
}
