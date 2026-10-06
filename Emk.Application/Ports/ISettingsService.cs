using Emk.Domain.Entities;

namespace Emk.Application.Ports
{
    public interface ISettingsService
    {
        EmkSettings LoadSettings();
        void SaveSettings(EmkSettings settings);
    }
}
