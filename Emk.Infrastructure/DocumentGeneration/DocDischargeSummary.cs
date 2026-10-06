namespace Emk.Infrastructure.DocumentGeneration
{
    public sealed class DocDischargeSummary : Emk.Services.Docs.DocDischargeSummary
    {
        public DocDischargeSummary(string filePath, int cartNoteId, int accountId)
            : base(filePath, cartNoteId, accountId)
        {
        }
    }
}
