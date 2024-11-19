using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emk.Interface
{
    public interface ILicenseRepository: IDbRepository
    {
        bool IsLicenseValid();
    }
}
