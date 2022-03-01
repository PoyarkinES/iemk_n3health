using Emk.Models;

namespace Emk.Services
{
    public interface ISettingsService
    {
        EmkSettings LoadSettings();
        void SaveSettings(EmkSettings settings);
    }
}