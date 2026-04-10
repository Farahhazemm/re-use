using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ReUse.Application.Interfaces;
using ReUse.Application.Interfaces.Services.Auth;
using ReUse.Application.Interfaces.Services.UserProfile;
using ReUse.Infrastructure.Services.Auth;
using ReUse.Infrastructure.Services.UserProfile;
//using ReUse.Application.Interfaces.IRepositories;
//using ReUse.Application.Interfaces.Services.UserProfile;
//using ReUse.Application.Interfaces.UnitOfWork;
//using ReUse.Infrastructure.Persistence.Repositories;
//using ReUse.Infrastructure.Persistence.UnitOfWork;
//using ReUse.Infrastructure.Services.UserProfile;

namespace ReUse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
           IConfiguration configuration)
    {

        // #region Repositories
        //services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        // #endregion

        #region UnitOfWork
        services.AddScoped<IUnitOfWork, ReUse.Infrastructure.UnitOfWork.UnitOfWork>();
        #endregion

        #region Services
        //services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IAuthService, JwtAuthService>();
        services.AddScoped<IUserService, UserService>();
        #endregion


        return services;
    }


}