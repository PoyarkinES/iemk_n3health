namespace Emk.Repository.Interface
{
    public interface ILicenseRepository: IDbRepository
    {
        bool IsLicenseValid();
    }
}
