using System;
using Emk.Domain.ValueObjects;

namespace Emk.Application.UseCases.Sending
{
    public class SendPatientDataRequest
    {
        private DateTime? _startDate;
        private DateTime? _endDate;
        private TreatmentPeriod _period;

        public DateTime? StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                _period = null;
            }
        }

        public DateTime? EndDate
        {
            get { return _endDate; }
            set
            {
                _endDate = value;
                _period = null;
            }
        }

        public TreatmentPeriod Period
        {
            get { return _period; }
            set
            {
                _period = value;
                _startDate = value == null ? (DateTime?)null : value.StartDate;
                _endDate = value == null ? (DateTime?)null : value.EndDate;
            }
        }

        public static SendPatientDataRequest ForPeriod(DateTime startDate, DateTime endDate)
        {
            if (startDate < endDate)
                return new SendPatientDataRequest { Period = new TreatmentPeriod(startDate, endDate) };

            return new SendPatientDataRequest { StartDate = startDate, EndDate = endDate };
        }

        /// <summary>
        /// Если задан (&gt; 0), отправляется только указанный счёт, период игнорируется
        /// </summary>
        public int AccountId { get; set; }
    }
}
