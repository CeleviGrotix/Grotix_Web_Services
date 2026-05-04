using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Repositories;
// Quita el using de IAM.Infrastructure.Persistence.Contexts si lo tenías
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration; 

namespace GrotixBackend.IAM.Infrastructure.Persistence.Repositories
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly AppDbContext _context; 

        public IdentityRepository(AppDbContext context) 
        {
            _context = context;
        }

        public async Task AddAsync(Identity identity)
        {
            await _context.Identities.AddAsync(identity);
        }

        public async Task<Identity> GetByIdAsync(int id)
        {
            return await _context.Identities
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _context.Identities.AnyAsync(i => i.UserName == username);
        }
        public async Task<Identity> GetByUsernameAsync(string username)
        {
            return await _context.Identities
                .FirstOrDefaultAsync(i => i.UserName == username);
        }

        public Task UpdateAsync(Identity identity)
        {
            _context.Entry(identity).State = EntityState.Modified;
            return Task.CompletedTask;
        }
    }
}