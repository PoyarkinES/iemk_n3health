using Emk.Application.Dto;

namespace Emk.Application.UseCases.Validation
{
    public class ValidateSendingRulesRequest
    {
        public int AccountId { get; set; }
        public int PatientTreatId { get; set; }

        /// <summary>
        /// Уже загруженный случай лечения; если задан, повторная загрузка по счёту не выполняется
        /// </summary>
        public PatientTreatDto Treat { get; set; }
    }
}
