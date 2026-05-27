using GrotixBackend.Shared.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using System.Threading.Tasks;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories
{
    /// <summary>
    /// Implementación de la Unidad de Trabajo para Entity Framework Core.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProfilesDbContext _context;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UnitOfWork"/>.
        /// </summary>
        /// <param name="context">El contexto de Profiles de Grotix.</param>
        public UnitOfWork(ProfilesDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Confirma todos los cambios realizados en los repositorios de forma atómica.
        /// </summary>
        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}