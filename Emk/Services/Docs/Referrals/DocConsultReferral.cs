namespace Emk.Services.Docs.Referrals
{
    public class DocConsultReferral : DocHospitalReferral
    {
        protected override string NsType => "3";
        protected override int DocCode => 34;

        public DocConsultReferral(string filePath, int cartNoteId, int accountId) : base(filePath, cartNoteId, accountId)
        {
        }
    }
}
