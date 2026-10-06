namespace Emk.Infrastructure.DocumentGeneration.Referrals
{
    public sealed class DocConsultReferral : Emk.Services.Docs.DocConsultReferral
    {
        public DocConsultReferral(string filePath, int cartNoteId, int accountId)
            : base(filePath, cartNoteId, accountId)
        {
        }
    }
}
