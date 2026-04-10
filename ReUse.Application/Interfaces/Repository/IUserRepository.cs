using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Domain.Entities;

namespace ReUse.Application.Interfaces.Repository
{

    public interface IUserRepository : IBaseRepository<User>
    {
        void Add(User entity);

        void Update(User entity);

        void Remove(User entity);
    }
}