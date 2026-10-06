namespace Emk.Infrastructure.DocumentGeneration
{
    public sealed class DocControlCardDispensaryObservation : Emk.Services.Docs.DocControlCardDispensaryObservation
    {
        public DocControlCardDispensaryObservation(string filePath, int cartNoteId, int accountId)
            : base(filePath, cartNoteId, accountId)
        {
        }
    }
}
