using System;
using System.Linq;
using System.Threading.Tasks;
using Emk.Application.Ports;
using Emk.Domain.DomainExceptions;

namespace Emk.Application.UseCases.Updating
{
    public class UpdatePatientDataUseCase : IUpdatePatientDataUseCase
    {
        private readonly ITreatRepository _treatRepository;
        private readonly IEmkRepository _emkRepository;
        private readonly ILicenseRepository _licenseRepository;
        private readonly IPixClient _pixClient;
        private readonly IEmkClient _emkClient;
        private readonly ILoggerService _logger;

        public UpdatePatientDataUseCase(
            ITreatRepository treatRepository,
            IEmkRepository emkRepository,
            ILicenseRepository licenseRepository,
            IPixClient pixClient,
            IEmkClient emkClient,
            ILoggerService logger)
        {
            _treatRepository = treatRepository ?? throw new ArgumentNullException(nameof(treatRepository));
            _emkRepository = emkRepository ?? throw new ArgumentNullException(nameof(emkRepository));
            _licenseRepository = licenseRepository ?? throw new ArgumentNullException(nameof(licenseRepository));
            _pixClient = pixClient ?? throw new ArgumentNullException(nameof(pixClient));
            _emkClient = emkClient ?? throw new ArgumentNullException(nameof(emkClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UpdatePatientDataResponse> ExecuteAsync(UpdatePatientDataRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                var treats = await _treatRepository.GetByAccountIdAsync(request.AccountId);
                var treat = treats?.FirstOrDefault();
                if (treat == null)
                    throw new NoPatientAccountsException();

                var signTask = _emkRepository.CheckDocumentEsignAsync(request.AccountId);
                var accessTask = _licenseRepository.IsValidAsync();
                await Task.WhenAll(signTask, accessTask);

                if (!accessTask.Result)
                    throw new InvalidLicenseException();
                if (!signTask.Result)
                    throw new DocumentSignatureException();

                if (!await _pixClient.UpdatePatientAsync(PatientAccountFactory.Create(treat)))
                    return Fail($"PIX не обновил пациента по счёту {request.AccountId}");

                if (await _emkClient.UpdateCaseAsync(treat) <= 0)
                    return Fail($"EMK не обновил случай по счёту {request.AccountId}");

                _logger.LogInfo($"Счёт {request.AccountId} обновлён");
                return new UpdatePatientDataResponse { Success = true, Message = "Данные обновлены." };
            }
            catch (DomainException ex)
            {
                return Fail(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка обновления счёта {request.AccountId}", ex);
                return new UpdatePatientDataResponse { Success = false, Message = "Внутренняя ошибка при обновлении данных. Подробности в журнале." };
            }
        }

        private UpdatePatientDataResponse Fail(string message)
        {
            _logger.LogWarning(message);
            return new UpdatePatientDataResponse { Success = false, Message = message };
        }
    }
}
