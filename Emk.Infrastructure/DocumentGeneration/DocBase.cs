namespace Emk.Infrastructure.DocumentGeneration
{
    public abstract class DocBase : Emk.Services.Docs.DocBase
    {
        protected DocBase(string filePath, int cartNoteId, int accountId)
            : base(filePath, cartNoteId, accountId)
        {
        }
    }
}
