using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace GrotixBackend.IAM.Infrastructure.Repositories
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly IamDbContext _context; 

        public IdentityRepository(IamDbContext context) 
        {
            _context = context;
        }

        public async Task AddAsync(Identity identity)
        {
            await _context.Identities.AddAsync(identity);
        }

        public async Task<Identity?> GetByIdAsync(int id)
        {
            return await _context.Identities
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Identities.AnyAsync(i => i.UserName == email);
        }

        public async Task<Identity?> GetByEmailAsync(string email)
        {
            return await _context.Identities
                .FirstOrDefaultAsync(i => i.UserName == email);
        }

        public Task UpdateAsync(Identity identity)
        {
            _context.Entry(identity).State = EntityState.Modified;
            return Task.CompletedTask;
        }
    }
}