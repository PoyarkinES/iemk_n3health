using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Emk.Application.Ports;

namespace EmkWinService
{
    public partial class EmkWinSvc : ServiceBase
    {
        private readonly ServiceRunner _runner;
        private readonly ILoggerService _logger;
        private CancellationTokenSource _cancellation;
        private Task _runnerTask;

        public EmkWinSvc(ServiceRunner runner, ILoggerService logger)
        {
            _runner = runner ?? throw new ArgumentNullException(nameof(runner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            InitializeComponent();
            ServiceName = "EmkWinService";
        }

        protected override void OnStart(string[] args)
        {
            _logger.LogInfo("Запуск службы...");
            _cancellation = new CancellationTokenSource();
            _runnerTask = _runner.RunAsync(_cancellation.Token);
        }

        protected override void OnStop()
        {
            _logger.LogInfo("Останавливаю службу...");
            if (_cancellation == null)
                return;

            _cancellation.Cancel();
            try
            {
                if (_runnerTask != null && !_runnerTask.Wait(TimeSpan.FromSeconds(30)))
                    _logger.LogWarning("Остановка службы продолжается: текущая отправка ещё выполняется.");
            }
            catch (AggregateException exception)
            {
                _logger.LogError("Ошибка при остановке службы", exception);
            }

            _cancellation.Dispose();
        }
    }
}
