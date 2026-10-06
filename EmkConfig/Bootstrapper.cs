using System;
using System.Configuration;
using Emk.Application.Services;
using Emk.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EmkConfig
{
    public static class Bootstrapper
    {
        public static ServiceProvider BuildServiceProvider()
        {
            var connection = ConfigurationManager.ConnectionStrings["EmkDb"];
            if (connection == null)
                throw new InvalidOperationException("Connection string 'EmkDb' is missing.");

            var services = new ServiceCollection();
            services.AddInfrastructure(connection.ConnectionString);
            ApplicationServiceExtensions.AddApplication((service, implementation) =>
                services.AddScoped(service, implementation));

            return services.BuildServiceProvider();
        }
    }
}
