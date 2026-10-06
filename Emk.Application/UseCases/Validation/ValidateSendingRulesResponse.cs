using System.Collections.Generic;
using System.Linq;

namespace Emk.Application.UseCases.Validation
{
    public class ValidateSendingRulesResponse
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; set; } = new List<string>();
    }
}
