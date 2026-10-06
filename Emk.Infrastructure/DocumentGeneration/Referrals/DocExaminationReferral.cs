namespace Emk.Infrastructure.DocumentGeneration.Referrals
{
    public sealed class DocExaminationReferral : Emk.Services.Docs.DocExaminationReferral
    {
        public DocExaminationReferral(string filePath, int cartNoteId, int accountId)
            : base(filePath, cartNoteId, accountId)
        {
        }
    }
}
