using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emk.Services.Docs.Referrals
{
    public class DocExaminationReferral : DocHospitalReferral
    {
        protected override string NsType => "2";
        protected override int DocCode => 33;

        public DocExaminationReferral(string filePath, int cartNoteId, int accountId) : base(filePath, cartNoteId, accountId)
        {
        }
    }
}
