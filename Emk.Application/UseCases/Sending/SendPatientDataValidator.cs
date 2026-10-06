using System;
using System.Collections.Generic;

namespace Emk.Application.UseCases.Sending
{
    public class SendPatientDataValidator
    {
        public List<string> Validate(SendPatientDataRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var errors = new List<string>();
            if (request.AccountId > 0)
                return errors;

            if (!request.StartDate.HasValue || !request.EndDate.HasValue)
                errors.Add("Необходимо указать счёт или период (дата начала и дата окончания).");
            else if (request.StartDate.Value > request.EndDate.Value)
                errors.Add("Дата начала периода не может быть больше даты окончания.");

            return errors;
        }
    }
}
