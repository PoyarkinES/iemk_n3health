namespace Emk.Application.UseCases.Sending
{
    public class SendPatientDataResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Count { get; set; }
    }
}
