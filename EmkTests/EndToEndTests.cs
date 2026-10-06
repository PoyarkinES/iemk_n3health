using System;
using System.Threading.Tasks;
using Emk.Application.Ports;
using Emk.Application.Services;
using Emk.Application.UseCases.Sending;
using Emk.Application.UseCases.Updating;
using Emk.Application.UseCases.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmkTests.Application
{
    [TestClass]
    public class EndToEndTests
    {
        [TestMethod]
        public async Task SendAndUpdateFlows_UseRegisteredApplicationUseCases()
        {
            var settings = new InMemorySettingsRepository();
            var license = new InMemoryLicenseRepository();
            var patients = new InMemoryPatientRepository();
            var treats = new InMemoryTreatRepository();
            var emkRepository = new InMemoryEmkRepository();
            var pixClient = new InMemoryPixClient();
            var emkClient = new InMemoryEmkClient();
            var logger = new InMemoryLogger();
            treats.Treats.Add(new Emk.Application.Dto.PatientTreatDto
            {
                TreatId = 5,
                AccountId = 10,
                PatientId = 7,
                PracticeId = 1,
                TreatDate = new DateTime(2024, 1, 10)
            });

            var services = new ServiceCollection();
            services.AddSingleton<ISettingsRepository>(settings);
            services.AddSingleton<ILicenseRepository>(license);
            services.AddSingleton<IPatientRepository>(patients);
            services.AddSingleton<ITreatRepository>(treats);
            services.AddSingleton<IEmkRepository>(emkRepository);
            services.AddSingleton<IPixClient>(pixClient);
            services.AddSingleton<IEmkClient>(emkClient);
            services.AddSingleton<ILoggerService>(logger);
            ApplicationServiceExtensions.AddApplication((service, implementation) =>
                services.AddTransient(service, implementation));

            using (var provider = services.BuildServiceProvider())
            {
                var send = provider.GetRequiredService<ISendPatientDataUseCase>();
                var update = provider.GetRequiredService<IUpdatePatientDataUseCase>();
                var validation = provider.GetRequiredService<IValidateSendingRulesUseCase>();

                var sendResponse = await send.ExecuteAsync(new SendPatientDataRequest
                {
                    StartDate = new DateTime(2024, 1, 9),
                    EndDate = new DateTime(2024, 1, 11)
                });
                var updateResponse = await update.ExecuteAsync(
                    new UpdatePatientDataRequest { AccountId = 10 });

                Assert.IsTrue(sendResponse.Success);
                Assert.AreEqual(1, sendResponse.Count);
                Assert.IsNotNull(validation);
                Assert.IsTrue(updateResponse.Success);
                CollectionAssert.AreEqual(new[] { 10 }, pixClient.Added);
                CollectionAssert.AreEqual(new[] { 10 }, pixClient.Updated);
                CollectionAssert.AreEqual(new[] { 10 }, emkClient.Added);
                CollectionAssert.AreEqual(new[] { 10 }, emkClient.Updated);
            }
        }
    }
}
