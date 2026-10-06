using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emk.Application.Dto;

namespace Emk.Application.Ports
{
    public interface ITreatRepository
    {
        Task<List<PatientTreatDto>> GetByPeriodAsync(DateTime start, DateTime end);
        Task<List<PatientTreatDto>> GetByAccountIdAsync(int accountId);
        Task UpdateEsignFilesAsync(PatientTreatDto treat);
    }
}
