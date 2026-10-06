namespace Emk.Infrastructure.DocumentGeneration
{
    public sealed class DocConsultNotePdf : Emk.Services.Docs.DocConsultNotePdf
    {
        public DocConsultNotePdf(string filePath, int cartNoteId, int accountId)
            : base(filePath, cartNoteId, accountId)
        {
        }
    }
}
