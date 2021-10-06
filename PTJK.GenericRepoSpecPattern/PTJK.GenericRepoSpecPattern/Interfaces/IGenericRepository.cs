using PTJK.GenericRepoSpecPattern.Entities;
using PTJK.GenericRepoSpecPattern.Specifications;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTJK.GenericRepoSpecPattern.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> ListAllAsync();

        Task<T> GetEntityWithSpec(ISpecification<T> spec);
        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec);
    }
}
