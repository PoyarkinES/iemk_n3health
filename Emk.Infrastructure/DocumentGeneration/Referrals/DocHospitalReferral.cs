namespace Emk.Infrastructure.DocumentGeneration.Referrals
{
    public class DocHospitalReferral : Emk.Services.Docs.DocHospitalReferral
    {
        public DocHospitalReferral(string filePath, int cartNoteId, int accountId)
            : base(filePath, cartNoteId, accountId)
        {
        }
    }
}
