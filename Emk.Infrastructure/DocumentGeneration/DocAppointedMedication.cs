namespace Emk.Infrastructure.DocumentGeneration
{
    public sealed class DocAppointedMedication : Emk.Services.Docs.DocAppointedMedication
    {
        public DocAppointedMedication(string filePath, int cartNoteId, int accountId)
            : base(filePath, cartNoteId, accountId)
        {
        }
    }
}
