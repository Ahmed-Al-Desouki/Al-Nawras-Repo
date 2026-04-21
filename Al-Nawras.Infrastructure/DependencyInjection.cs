using Al_Nawras.Application.Auth.Commands.GoogleLogin;
using Al_Nawras.Application.Auth.Commands.Login;
using Al_Nawras.Application.Auth.Commands.Register;
using Al_Nawras.Application.Auth.Interfaces;
using Al_Nawras.Application.Clients.Commands.CreateClient;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Dashboard.Queries.GetDashboard;
using Al_Nawras.Application.Deals.Commands.CreateDeal;
using Al_Nawras.Application.Deals.Commands.MoveDealStatus;
using Al_Nawras.Application.Deals.Queries.GetDealById;
using Al_Nawras.Application.Deals.Queries.GetDeals;
using Al_Nawras.Application.Documents.Commands.UploadDocument;
using Al_Nawras.Application.Payments.Commands.CreatePayment;
using Al_Nawras.Application.Payments.Commands.MarkPaymentPaid;
using Al_Nawras.Application.Shipments.Commands.CreateShipment;
using Al_Nawras.Application.Shipments.Commands.UpdateShipmentStatus;
using Al_Nawras.Infrastructure.Extensions;
using Al_Nawras.Infrastructure.Persistence;
using Al_Nawras.Infrastructure.Persistence.Repositories;
using Al_Nawras.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                    b => b.MigrationsAssembly("Al-Nawras.Infrastructure")
                ));

            services.AddJwtAuthentication(configuration);

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
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddScoped<RegisterHandler>();
            services.AddScoped<GoogleLoginHandler>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();

            // Handlers
            services.AddScoped<LoginHandler>();
            services.AddScoped<CreateDealHandler>();
            services.AddScoped<MoveDealStatusHandler>();
            services.AddScoped<GetDealByIdHandler>();
            services.AddScoped<GetDealsHandler>();
            services.AddScoped<CreateClientHandler>();
            services.AddScoped<CreateShipmentHandler>();
            services.AddScoped<UpdateShipmentStatusHandler>();
            services.AddScoped<CreatePaymentHandler>();
            services.AddScoped<MarkPaymentPaidHandler>();
            services.AddScoped<UploadDocumentHandler>();
            services.AddScoped<GetDashboardHandler>();

            return services;
        }
    }
}
