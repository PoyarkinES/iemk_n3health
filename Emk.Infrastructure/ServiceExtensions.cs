using Emk.Application.Ports;
using Emk.Domain.Entities;
using Emk.Infrastructure.Configuration;
using Emk.Infrastructure.ExternalServices.Adapters;
using Emk.Infrastructure.Logging;
using Emk.Infrastructure.Persistence.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Emk.Infrastructure
{
    public static class InfrastructureServiceExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<IPatientRepository>(_ => new OdbcPatientRepository(connectionString));
            services.AddScoped<ITreatRepository>(_ => new OdbcTreatRepository(connectionString));
            services.AddScoped<IEmkRepository>(_ => new OdbcEmkRepository(connectionString));
            services.AddScoped<ILicenseRepository>(_ => new OdbcLicenseRepository(connectionString));
            services.AddScoped<IDoctorRepository>(_ => new OdbcDoctorRepository(connectionString));
            services.AddScoped<ISettingsRepository>(_ => new OdbcSettingsRepository(connectionString));
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IDoctorFileService, DoctorFileService>();
            services.AddScoped<ILoggerService, FileLoggerService>();
            services.AddScoped<ServiceSettingsService>();
            services.AddScoped<EmkSettings>(provider => provider.GetRequiredService<ServiceSettingsService>().LoadSettings());
            services.AddScoped<IPixClient, PixServiceAdapter>();
            services.AddScoped<IEmkClient, EmkServiceAdapter>();
        }
    }
}
