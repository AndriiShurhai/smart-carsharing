using SmartCarSharing.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartCarSharing.Core.Services
{
    public interface ICarService
    {
        Task<List<Car>> GetAllCarsAsync();
        Task<List<Car>> GetFilteredCarsAsync(string search);
    }
}