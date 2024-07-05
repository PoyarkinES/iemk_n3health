using Emk;
using Emk.Models;
using Emk.Services;
using System;
using System.Linq;
using System.ServiceProcess;
using System.Timers;
using Newtonsoft.Json;

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
			_timer = new Timer(60);
			_timer.Elapsed += _timer_Elapsed;
			_lastStart = DateTime.MinValue;
		}

		private void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
            try
            {
                if (!ShouldStart())
                    return;
                _lastStart = DateTime.Now;
                Log.Info("Отправка данных запущена");
                var s = new EmkSendingService(true);
                _isRunning = true;
                s.Run();
                _isRunning = false;

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Log.Info("Запуск службы...");
                _settings = Factory.LoadSettings().FirstOrDefault();
                Log.Info(_settings.ToString());
                _timer.AutoReset = true;
                _timer.Start();
            }
            catch (Exception e)
            {
                Log.Error(JsonConvert.SerializeObject(e));
                Stop();
            }
        }

        protected override void OnStop()
		{
			Log.Info("Останавливаю службу...");
			_timer.Stop();
			_timer.Dispose();
		}

		private bool ShouldStart()
		{
            try
            {
                if (_isRunning)
                    return false;
                if (_settings.UpdateTime == TimeSpan.Zero)
                {
                    Log.Error($"Не удалось распознать значение времени {_settings.UpdateTime}");
                    throw new ArgumentNullException("UpdateTime", $"Не удалось распознать значение времени {_settings.UpdateTime}");
                }
                if (DateTime.Now.TimeOfDay >= _settings.UpdateTime)
                {
                    if (_lastStart.Date < DateTime.Now.Date)
                        return true;
                }

                return false;
            }
			catch (Exception e)
            {
                Log.Error(e.Message);
                return false;
            }
        }

	}
}
