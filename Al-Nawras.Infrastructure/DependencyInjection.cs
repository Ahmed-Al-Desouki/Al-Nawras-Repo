using Al_Nawras.Application.Auth.Commands.Login;
using Al_Nawras.Application.Auth.Interfaces;
using Al_Nawras.Application.Clients.Commands.CreateClient;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Deals.Commands.CreateDeal;
using Al_Nawras.Application.Deals.Commands.MoveDealStatus;
using Al_Nawras.Application.Deals.Queries.GetDealById;
using Al_Nawras.Application.Deals.Queries.GetDeals;
using Al_Nawras.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("ImportExport.Infrastructure")
                ));

            // DbContext as both interfaces
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

            // Repositories
            services.AddScoped<IDealRepository, DealRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IShipmentRepository, ShipmentRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();

            // Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();

            // Handlers
            services.AddScoped<LoginHandler>();
            services.AddScoped<CreateDealHandler>();
            services.AddScoped<MoveDealStatusHandler>();
            services.AddScoped<GetDealByIdHandler>();
            services.AddScoped<GetDealsHandler>();
            services.AddScoped<CreateClientHandler>();

            return services;
        }
    }
}
