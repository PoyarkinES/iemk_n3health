using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.Ports;
using Emk.Infrastructure.Persistence;

namespace Emk.Infrastructure.Persistence.Implementations
{
    public sealed class OdbcEmkRepository : OdbcRepository, IEmkRepository
    {
        public OdbcEmkRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<DocumentDto>> GetDocumentsByAccountIdAsync(int accountId)
        {
            return await Query(SqlResources.Get("CheckDocumentAccess"), MapDocument,
                Parameter("accId", accountId)).ConfigureAwait(false);
        }

        public async Task<bool> CheckDocumentEsignAsync(int accountId)
        {
            var byFlag = await Query(SqlResources.Get("CheckDocumentEsignByFlag"),
                reader => reader.Get<int>("acc_cnt"), Parameter("account_id", accountId)).ConfigureAwait(false);
            var byDate = await Query(SqlResources.Get("CheckDocumentEsignByDate"),
                reader => reader.Get<int>("acc_cnt"), Parameter("account_id", accountId)).ConfigureAwait(false);
            return byFlag.Contains(1) || byDate.Contains(1);
        }

        private static DocumentDto MapDocument(IDataReader reader)
        {
            return new DocumentDto
            {
                EsignFilesId = reader.Get<int>("esign_files_id"),
                AccountId = reader.Get<int>("account_id"),
                PatientId = reader.Get<int>("patient_id"),
                PracticeId = reader.Get<int>("practice_id"),
                FileName = reader.Get<string>("efiles_name"),
                FilePath = reader.Get<string>("efiles_path"),
                Uuid = reader.Get<string>("uuid"),
                IsSignedByCommission = reader.Get<int>("is_sign_cmn") != 0,
                IsSignedByDoctor = reader.Get<int>("is_sign_pr") != 0,
                DateCreated = reader.Get<System.DateTime>("date_created")
            };
        }
    }
}
