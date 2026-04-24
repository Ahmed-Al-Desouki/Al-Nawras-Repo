using Al_Nawras.Application.AuditLogs.Queries.GetAuditHistory;
using Al_Nawras.Application.Auth.Commands.GoogleLogin;
using Al_Nawras.Application.Auth.Commands.Login;
using Al_Nawras.Application.Auth.Commands.Register;
using Al_Nawras.Application.Auth.Interfaces;
using Al_Nawras.Application.ClientPortal.Queries.GetMyDealDetail;
using Al_Nawras.Application.ClientPortal.Queries.GetMyDeals;
using Al_Nawras.Application.ClientPortal.Queries.GetMyDocuments;
using Al_Nawras.Application.ClientPortal.Queries.GetMyPayments;
using Al_Nawras.Application.ClientPortal.Queries.GetMyShipments;
using Al_Nawras.Application.Clients.Commands.CreateClient;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Notifications;
using Al_Nawras.Application.Dashboard.Queries.GetDashboard;
using Al_Nawras.Application.Deals.Commands.CreateDeal;
using Al_Nawras.Application.Deals.Commands.MoveDealStatus;
using Al_Nawras.Application.Deals.Queries.GetDealById;
using Al_Nawras.Application.Deals.Queries.GetDeals;
using Al_Nawras.Application.Documents.Commands.UploadDocument;
using Al_Nawras.Application.Jobs;
using Al_Nawras.Application.Payments.Commands.CreatePayment;
using Al_Nawras.Application.Payments.Commands.MarkPaymentPaid;
using Al_Nawras.Application.Reports.Queries.ExportReport;
using Al_Nawras.Application.Reports.Queries.GetEmployeePerformance;
using Al_Nawras.Application.Reports.Queries.GetRevenueByPeriod;
using Al_Nawras.Application.Reporting.Commands.CreateReportTemplate;
using Al_Nawras.Application.Reporting.Commands.ReviewReportImport;
using Al_Nawras.Application.Reporting.Commands.UploadReportImport;
using Al_Nawras.Application.Reporting.Queries.DownloadReportImportSourceFile;
using Al_Nawras.Application.Reporting.Queries.DownloadReportTemplate;
using Al_Nawras.Application.Reporting.Queries.GetReportImportById;
using Al_Nawras.Application.Reporting.Queries.GetReportImports;
using Al_Nawras.Application.Reporting.Queries.GetReportTemplateById;
using Al_Nawras.Application.Reporting.Queries.GetReportTemplates;
using Al_Nawras.Application.Reporting.Queries.GetReportingOverview;
using Al_Nawras.Application.Shipments.Commands.CreateShipment;
using Al_Nawras.Application.Shipments.Commands.UpdateShipmentStatus;
using Al_Nawras.Infrastructure.BackgroundJobs;
using Al_Nawras.Infrastructure.Extensions;
using Al_Nawras.Infrastructure.Notifications;
using Al_Nawras.Infrastructure.Persistence;
using Al_Nawras.Infrastructure.Persistence.Interceptors;
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

            // Register DbContext with the interceptor injected
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                var interceptor = serviceProvider.GetRequiredService<AuditInterceptor>();

                options
                    .UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection"),
                        b => b.MigrationsAssembly("Al-Nawras.Infrastructure")
                    )
                    .AddInterceptors(interceptor);
            });

            services.AddJwtAuthentication(configuration);
            services.AddHttpContextAccessor();
            services.AddHostedService<OverduePaymentHostedService>();
            services.AddSignalR();
            services.Configure<EmailOptions>(configuration.GetSection("Email"));

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
            services.AddScoped<IAuditContext, AuditContext>();
            services.AddScoped<AuditInterceptor>();
            services.AddScoped<IOverduePaymentJob, OverduePaymentJob>();
            services.AddScoped<IExcelExportService, ExcelExportService>();
            services.AddScoped<IReportingWorkbookService, ReportingWorkbookService>();
            services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
            services.AddScoped<IEmailSender, SmtpEmailSender>();
            services.AddSingleton<IRealtimeNotifier, SignalRRealtimeNotifier>();

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
            services.AddScoped<GetMyDealsHandler>();
            services.AddScoped<GetMyDealDetailHandler>();
            services.AddScoped<GetMyShipmentsHandler>();
            services.AddScoped<GetMyPaymentsHandler>();
            services.AddScoped<GetMyDocumentsHandler>();
            services.AddScoped<GetAuditHistoryHandler>();
            services.AddScoped<GetRevenueByPeriodHandler>();
            services.AddScoped<GetEmployeePerformanceHandler>();
            services.AddScoped<ExportReportHandler>();
            services.AddScoped<GetReportTemplatesHandler>();
            services.AddScoped<GetReportTemplateByIdHandler>();
            services.AddScoped<CreateReportTemplateHandler>();
            services.AddScoped<ReviewReportImportHandler>();
            services.AddScoped<DownloadReportImportSourceFileHandler>();
            services.AddScoped<DownloadReportTemplateHandler>();
            services.AddScoped<UploadReportImportHandler>();
            services.AddScoped<GetReportImportsHandler>();
            services.AddScoped<GetReportImportByIdHandler>();
            services.AddScoped<GetReportingOverviewHandler>();

            return services;
        }
    }
}
