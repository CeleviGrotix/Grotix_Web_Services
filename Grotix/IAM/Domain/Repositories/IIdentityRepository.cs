using GrotixBackend.IAM.Domain.Model.Aggregates;
using System;
using System.Threading.Tasks;

namespace GrotixBackend.IAM.Domain.Repositories
{
    public interface IIdentityRepository
    {
        Task<Identity?> GetByIdAsync(int id);
        Task<Identity?> GetByEmailAsync(string email);
        Task AddAsync(Identity identity);
        Task UpdateAsync(Identity identity);

        Task<bool> ExistsByEmailAsync(string Email);
    }
}