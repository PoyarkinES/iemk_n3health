namespace Emk.Infrastructure.DocumentGeneration
{
    public sealed class DocPrescription : Emk.Services.Docs.DocPrescription
    {
        public DocPrescription(string filePath, int cartNoteId, int accountId)
            : base(filePath, cartNoteId, accountId)
        {
        }
    }
}
