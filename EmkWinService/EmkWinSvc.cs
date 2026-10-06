using Emk;
using Emk.Models;
using Emk.Services;
using System;
using System.Linq;
using System.ServiceProcess;
using System.Timers;
using Interlocked = System.Threading.Interlocked;

namespace EmkWinService
{
	public partial class EmkWinSvc : ServiceBase
	{
		private Timer _timer;
		private int _isRunning;
		private EmkSettings _settings;
		private DateTime _lastStart;
		public EmkWinSvc()
		{
			InitializeComponent();
			_timer = new Timer(60000);
			_timer.Elapsed += _timer_Elapsed;
			_lastStart = DateTime.MinValue;
		}

		private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (!ShouldStart() || Interlocked.Exchange(ref _isRunning, 1) != 0)
				return;

			try
			{
				_lastStart = DateTime.Now;
				Log.Info("Отправка данных запущена");
				var sendingService = new EmkSendingService(true);
				await sendingService.Run().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			finally
			{
				Interlocked.Exchange(ref _isRunning, 0);
			}
		}

        protected override async void OnStart(string[] args)
        {
            try
            {
                Log.Info("Запуск службы...");
                _settings = (await Factory.LoadSettingsAsync()).First();
                Log.Info(_settings.ToString());
                _timer.AutoReset = true;
                _timer.Start();
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
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
