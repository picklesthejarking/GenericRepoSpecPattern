using PTJK.GenericRepoSpecPattern.Entities;
using PTJK.GenericRepoSpecPattern.Specifications;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace PTJK.GenericRepoSpecPattern.Data
{
    public class SpecificationEvaluator<TEntity> where TEntity : BaseEntity
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> spec)
        {
            var query = inputQuery;

            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

            return query;
        }
    }
}
