using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.Ports;
using Emk.Domain.Entities;
using Emk.EmkSvc;
using Emk.Infrastructure.ExternalServices.Mappers;

namespace Emk.Infrastructure.ExternalServices.Adapters
{
    public sealed class EmkServiceAdapter : IEmkClient
    {
        private readonly EmkSettings _settings;
        private readonly Func<string, EmkServiceClient> _clientFactory;

        public EmkServiceAdapter(EmkSettings settings)
            : this(settings, url => new EmkServiceClient(
                new BasicHttpBinding(), new EndpointAddress(url)))
        {
        }

        public EmkServiceAdapter(EmkSettings settings, Func<string, EmkServiceClient> clientFactory)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public Task<int> AddCaseAsync(PatientTreatDto treat, bool isUpdate = false)
        {
            return SendAsync(treat, isUpdate);
        }

        public Task<int> UpdateCaseAsync(PatientTreatDto treat)
        {
            return SendAsync(treat, true);
        }

        private async Task<int> SendAsync(PatientTreatDto treat, bool update)
        {
            if (treat == null)
                throw new ArgumentNullException(nameof(treat));

            EmkServiceClient client = null;
            try
            {
                client = _clientFactory(_settings.EmkUrl);
                var caseDto = EmkServiceMapper.ToServiceDto(treat, _settings.IdLPU.ToString());
                var guid = _settings.Guid.ToString();
                if (update)
                    await client.UpdateCaseAsync(guid, caseDto).ConfigureAwait(false);
                else
                    await client.AddCaseAsync(guid, caseDto).ConfigureAwait(false);

                return 0;
            }
            catch (Exception exception)
            {
                throw new ExternalServiceException("The EMK service request failed.", exception);
            }
            finally
            {
                CloseSafely(client);
            }
        }

        private static void CloseSafely(EmkServiceClient client)
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
