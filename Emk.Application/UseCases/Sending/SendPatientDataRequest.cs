using System;

namespace Emk.Application.UseCases.Sending
{
    public class SendPatientDataRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Если задан (&gt; 0), отправляется только указанный счёт, период игнорируется
        /// </summary>
        public int AccountId { get; set; }
    }
}
