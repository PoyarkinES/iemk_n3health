namespace Emk.Infrastructure.DocumentGeneration
{
    public sealed class DocConsultNote : Emk.Services.Docs.DocConsultNote
    {
        public DocConsultNote(string filePath, int cartNoteId, int accountId, string idMis)
            : base(filePath, cartNoteId, accountId, idMis)
        {
        }
    }
}
