using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.Ports;
using Emk.Domain.Entities;
using Emk.Infrastructure.ExternalServices.Mappers;
using Emk.PixSvc;

namespace Emk.Infrastructure.ExternalServices.Adapters
{
    public sealed class PixServiceAdapter : IPixClient
    {
        private readonly IPatientRepository _patients;
        private readonly EmkSettings _settings;
        private readonly Func<string, PixServiceClient> _clientFactory;

        public PixServiceAdapter(IPatientRepository patients, EmkSettings settings)
            : this(patients, settings, url => new PixServiceClient(
                new BasicHttpBinding(), new EndpointAddress(url)))
        {
        }

        public PixServiceAdapter(
            IPatientRepository patients,
            EmkSettings settings,
            Func<string, PixServiceClient> clientFactory)
        {
            _patients = patients ?? throw new ArgumentNullException(nameof(patients));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public Task<bool> AddPatientAsync(PatientAccountDto patient)
        {
            return SendAsync(patient, false);
        }

        public Task<bool> UpdatePatientAsync(PatientAccountDto patient)
        {
            return SendAsync(patient, true);
        }

        private async Task<bool> SendAsync(PatientAccountDto account, bool update)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            var hasConsent = await _patients.CheckConsentToShareAsync(account.PatientId).ConfigureAwait(false);
            Emk.PixSvc.PatientDto wcfPatient;
            if (hasConsent)
            {
                var patient = await _patients.GetByIdAsync(account.PatientId).ConfigureAwait(false);
                if (patient == null)
                    throw new ExternalServiceException("The patient could not be loaded for the PIX request.",
                        new InvalidOperationException("Patient " + account.PatientId + " was not found."));

                wcfPatient = PixServiceMapper.ToServiceDto(account, patient);
            }
            else
            {
                wcfPatient = PixServiceMapper.ToAnonymousServiceDto(
                    _settings.UnknownPatientFirstName, _settings.UnknownPatientGivenName);
            }

            PixServiceClient client = null;
            try
            {
                client = _clientFactory(_settings.PixUrl);
                var guid = _settings.Guid.ToString();
                var lpu = _settings.IdLPU.ToString();
                if (update)
                    await client.UpdatePatientAsync(guid, lpu, wcfPatient).ConfigureAwait(false);
                else
                    await client.AddPatientAsync(guid, lpu, wcfPatient).ConfigureAwait(false);

                return true;
            }
            catch (FaultException exception)
            {
                throw new ExternalServiceException("The PIX service rejected the patient request.", exception);
            }
            catch (Exception exception)
            {
                throw new ExternalServiceException("The PIX service request failed.", exception);
            }
            finally
            {
                CloseSafely(client);
            }
        }

        private static void CloseSafely(PixServiceClient client)
        {
            if (client == null)
                return;

            try
            {
                client.Close();
            }
            catch
            {
                client.Abort();
            }
        }
    }
}
