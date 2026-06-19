using Emk.Models;
using Emk.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Emk.Repository
{
    public class TreatRepository(string connectionString) : DbRepository(connectionString)
    {
        public async Task<IEnumerable<PatientTreat>> GetPatientsTreatsAsync(DateTime sinceDate, DateTime toDate)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "since", value: sinceDate),
                new(parameterName: "to", value: toDate)
            };
            var data = await Query(Resources.GetPatientsTreatsByPeriod, patientTreatMap, [.. param]).ConfigureAwait(false);
            return data;
        }

        public async Task<PatientAccount> GetPatientAccountByIdAsync(int accountId)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "account_id", value: accountId),
            };
            var data = await Query(Resources.GetPatientAccountById, PatientAccountMap, [.. param]).ConfigureAwait(false);
            return data.FirstOrDefault();
        }

        public async Task<List<PatientAccount>> GetPatientAccountsAsync(DateTime sinceDate, DateTime toDate)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "since", value: sinceDate),
            };

            if (toDate == DateTime.MinValue)
                param.Add(new SqlParameter(parameterName: "to", value: toDate));

            var data = await Query(
                toDate == DateTime.MinValue ? Resources.GetPatientAccountsByDate : Resources.GetPatientAccountsByPeriod,
                PatientAccountMap, [.. param]).ConfigureAwait(false);
            return data;
        }

        public async Task<IEnumerable<string>> GetCheckPracticIdAsync(int paccount)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "account_id", value: paccount),
            };

            var data = await Query(Resources.GetCheckPracticId, GetCheckPracticIdMap, [.. param]).ConfigureAwait(false);
            return data;
        }

        public async Task<IEnumerable<string>> GetCheckDocumentEsignAsync(int paccount)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "account_id", value: paccount),
            };

            var data = await Query(Resources.CheckDocumentEsignByFlag, (IDataReader reader) => checkDocumentEsignMap(reader, paccount), [.. param]).ConfigureAwait(false);
            data.AddRange(await Query(Resources.CheckDocumentEsignByDate, (IDataReader reader) => checkDocumentEsignMap(reader, paccount), [.. param]).ConfigureAwait(false));
            return data;
        }

        public async Task<List<string>> CheckDocumentAccessAsync(int accId)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "account_id", value: accId),
            };

            var data = await Query(Resources.CheckDocumentAccess, (IDataReader reader) => checkDocumentAccessAsync(reader, accId).GetAwaiter().GetResult(), [.. param]);
            return data;
        }

        public async Task<List<DocumentsDto>> GetDocumentByAccountIdAsync(int accountId)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "account_id", value: accountId),
            };

            var data = await Query(Resources.CheckDocumentAccess, getDocumentByAccountIdMap, param.ToArray());
            return data;
        }

        public async Task<int> CheckPatientConsentTransPersDataAsync(int patientId)
        {
            var result = await Scalar<int>($"SELECT COUNT() FROM patients WHERE patient_id = {patientId} AND consent_transf_pers_data = 'N'");
            return result;
        }

        public async Task<string> GetFileDirectoryAsync(int practicId)
        {
            return await Scalar<string>($"select dba.sf_get_param_value('PATH_EXT_DOCS',{practicId})");
        }

        private async Task<string> checkDocumentAccessAsync(IDataReader reader, int accId)
        {
            try
            {
                var practicId = reader.Get<int>("practice_id");
                var filePath = await getFullFilePathAsync(practicId, reader.Get<string>("efiles_path"), reader.Get<string>("efiles_name"));

                return !File.Exists(filePath)
                    ? $"Электронный документ {filePath}, для случая '{accId}', не найден или отсутствуют права доступа."
                    : string.Empty;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return e.Message;
            }
        }

        private string checkDocumentEsignMap(IDataReader reader, int paccount)
        {
            return reader.Get<int>("acc_cnt") == 1 
                ? $"Электронный документ, прикрепленный к случаю '{paccount}', был передан ранее, случай не отправлен."
                : string.Empty;
        }

        private DocumentsDto getDocumentByAccountIdMap(IDataReader reader)
        {
            return new DocumentsDto { 
                efiles_name = reader.Get<string>("efiles_name"), 
                uuid = reader.Get<string>("uuid"),
                account_id = reader.Get<int>("account_id"),
                date_approved = reader.Get<DateTime>("date_approved"),
                date_created = reader.Get<DateTime>("date_created"),
                date_sent = reader.Get<DateTime>("date_sent"),
                efiles_path = reader.Get<string>("efiles_path"),
                esign_files_id = reader.Get<int>("esign_files_id"),
                is_sign_cmn = reader.Get<int>("is_sign_cmn"),
                is_sign_pr = reader.Get<int>("is_sign_pr"),
                patient_id = reader.Get<int>("patient_id"),
                practice_id = reader.Get<short>("practice_id"),
                provider_id = reader.Get<int>("provider_id")
            };
        }

        private string checkCorrectFileName(string filename)
        {
            return filename[filename.Length - 1] == '\\' ? filename.Substring(0, filename.Length - 1) : filename;
        }

        private async Task<string> getFullFilePathAsync(int practicId, string filepath, string filename)
        {
            return $"{checkCorrectFileName(await GetFileDirectoryAsync(practicId))}\\{checkCorrectFileName(filepath)}\\{filename}";
        }

        private PatientTreat patientTreatMap(IDataReader reader)
        {
            return new PatientTreat()
            {
                PatientId = reader.Get<int>("patient_id"),
                TreatDate = reader.Get<DateTime?>("treat_date")?? DateTime.Now,
                PracticeId = reader.Get<int>("practice_id"),
                SpecialityCode = reader.Get<int>("Code"),
                SpecialityName = reader.Get<string>("name")
            };
        }

        private PatientAccount PatientAccountMap(IDataReader reader)
        {
            return new PatientAccount
            {
                PatientId = reader.Get<int>("patient_id"),
                TreatDate = reader.Get<DateTime?>("treat_date")?? DateTime.Now,
                EsfDate = reader.Get<DateTime?>("EsfDate") ?? DateTime.MinValue,
                PracticeId = reader.Get<int>("practice_id"),
                ProviderId = reader.Get<int>("provider_id"),
                AccountId = reader.Get<int>("account_id"),
                Code = reader.Get<int>("Code"),
                Name = reader.Get<string>("name"),
                DiagnoseName = reader.Get<string>("diagnosis_name"),
                DiagnoseCode = reader.Get<string>("diagnosis_code"),
                ListProcedures = reader.Get<string>("list_procedures")
            };
        }

        private string GetCheckPracticIdMap(IDataReader reader)
        {

            var tpr = reader.Get<int>("tpr");
            var ppr = reader.Get<int>("ppr");
            var acc = reader.Get<int>("acc");
            var tprName = reader.Get<string>("tpr_name");
            var pprName = reader.Get<string>("ppr_name");

            return tpr != ppr ? $"Случай лечения {acc} создан в практике: '{pprName}' лечение пациента создано в практике: '{tprName}', случай не отправлен." : String.Empty;
        }
    }
}
