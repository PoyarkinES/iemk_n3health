using System;
using Emk.Application.UseCases.Sending;
using Emk.Application.UseCases.Updating;
using Emk.Application.UseCases.Validation;

namespace Emk.Application.Services
{
    public static class ApplicationServiceExtensions
    {
        /// <summary>
        /// Регистрирует Use Cases в любом DI-контейнере через переданный делегат (интерфейс, реализация).
        /// Реализации портов регистрируются на стороне Infrastructure.
        /// </summary>
        public static void AddApplication(Action<Type, Type> register)
        {
            if (register == null)
                throw new ArgumentNullException(nameof(register));

            register(typeof(IValidateSendingRulesUseCase), typeof(ValidateSendingRulesUseCase));
            register(typeof(ISendPatientDataUseCase), typeof(SendPatientDataUseCase));
            register(typeof(IUpdatePatientDataUseCase), typeof(UpdatePatientDataUseCase));
        }
    }
}
