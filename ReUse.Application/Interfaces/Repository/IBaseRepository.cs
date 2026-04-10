using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.Interfaces.Repository
{

    public interface IBaseRepository<T> where T : class
    {
        public Task<T?> GetByIdAsync(Guid id);

        public void Update(T entity);
    }
}