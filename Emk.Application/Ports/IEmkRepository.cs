using System.Collections.Generic;
using System.Threading.Tasks;
using Emk.Application.Dto;

namespace Emk.Application.Ports
{
    public interface IEmkRepository
    {
        Task<List<DocumentDto>> GetDocumentsByAccountIdAsync(int accountId);
        Task<bool> CheckDocumentEsignAsync(int accountId);
    }
}
