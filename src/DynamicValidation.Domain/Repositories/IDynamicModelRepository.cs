using System.Collections.Generic;
using System.Threading.Tasks;
using DynamicValidation.Domain.Entities;

namespace DynamicValidation.Domain.Repositories
{
    /// <summary>
    /// Model repository arayüzü
    /// </summary>
    public interface IDynamicModelRepository
    {
        Task<DynamicModel> GetByIdAsync(int id);
        Task<DynamicModel> GetByNameAsync(string name);
        Task<List<DynamicModel>> GetAllAsync();
        Task<DynamicModel> AddAsync(DynamicModel model);
        Task UpdateAsync(DynamicModel model);
        Task DeleteAsync(int id);
    }
}