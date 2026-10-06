using System.Collections.Generic;
using System.Threading.Tasks;
using Emk.Application.Dto;

namespace Emk.Application.Ports
{
    public interface ISettingsRepository
    {
        Task<List<EmkSettingsDto>> LoadSettingsAsync(bool force = false);
    }
}
