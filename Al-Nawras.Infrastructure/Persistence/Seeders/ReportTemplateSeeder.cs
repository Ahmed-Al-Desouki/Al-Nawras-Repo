using System.Text.Json;
using Al_Nawras.Application.Reporting.DTOs;
using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Al_Nawras.Infrastructure.Persistence.Seeders
{
    public static class ReportTemplateSeeder
    {
        public static async Task SeedAsync(AppDbContext context, ILogger logger)
        {
            if (await context.ReportTemplates.AnyAsync())
            {
                logger.LogInformation("ReportTemplateSeeder: templates already exist, skipping.");
                return;
            }

            var templates = BuildTemplates()
                .Select(definition => new ReportTemplate(
                    definition.Name,
                    definition.TemplateCode,
                    definition.Category,
                    definition.Description,
                    JsonSerializer.Serialize(definition),
                    isSystem: true))
                .ToList();

            await context.ReportTemplates.AddRangeAsync(templates);
            await context.SaveChangesAsync();
            logger.LogInformation("ReportTemplateSeeder: seeded {Count} report templates.", templates.Count);
        }

        private static List<ReportTemplateDefinitionDto> BuildTemplates()
        {
            return
            [
                new(
                    "shipment-tracking-register",
                    "Shipment Tracking Register",
                    "Track active shipments, carriers, milestones, customs status, and delivery dates.",
                    ReportTemplateCategory.ImportExport,
                    [
                        new("Shipments", "Operational shipment register for import-export teams.",
                        [
                            new("Shipment Number", "Text", true, "SH-202604-ABC123", "Internal shipment reference."),
                            new("Deal Number", "Text", true, "DL-202604-ABC123", "Link shipment to deal."),
                            new("Client", "Text", true, "Gulf Steel Trading LLC", "Customer or consignee."),
                            new("Carrier", "Text", true, "MSC Shipping", "Carrier or logistics provider."),
                            new("Tracking Number", "Text", false, "MSC-2026-7788", "Carrier tracking reference."),
                            new("ETD", "Date", false, "2026-04-25", "Estimated departure date."),
                            new("ETA", "Date", false, "2026-05-06", "Estimated arrival date."),
                            new("Status", "Text", true, "In Transit", "Pending, In Transit, Customs, Delivered.")
                        ])
                    ]),
                new(
                    "trade-finance-payments",
                    "Trade Finance and Payments",
                    "Monitor payment stages, references, exposure, and collection risk for each deal.",
                    ReportTemplateCategory.ImportExport,
                    [
                        new("Payments", "Track advance, on-shipment, and on-delivery payments.",
                        [
                            new("Deal Number", "Text", true, "DL-202604-ABC123", "Primary commercial deal number."),
                            new("Client", "Text", true, "Nile Textiles Co.", "Client legal or trading name."),
                            new("Payment Reference", "Text", false, "PAY-20260424-X12", "Bank or system reference."),
                            new("Payment Type", "Text", true, "Advance", "Advance, OnShipment, OnDelivery."),
                            new("Currency", "Text", true, "USD", "Invoice currency."),
                            new("Amount", "Number", true, "125000", "Payment amount in source currency."),
                            new("Due Date", "Date", true, "2026-05-10", "Contractual due date."),
                            new("Status", "Text", true, "Pending", "Pending, FullyPaid, Overdue.")
                        ])
                    ]),
                new(
                    "customs-clearance-checklist",
                    "Customs Clearance Checklist",
                    "Organize required customs documents, broker tasks, fees, and release readiness.",
                    ReportTemplateCategory.ImportExport,
                    [
                        new("Clearance", "Monitor customs checkpoints and missing paperwork.",
                        [
                            new("Shipment Number", "Text", true, "SH-202604-ABC123", "Shipment being cleared."),
                            new("Port", "Text", true, "Alexandria Port", "Entry port or dry port."),
                            new("Broker", "Text", false, "Delta Customs", "Assigned customs broker."),
                            new("Required Document", "Text", true, "Bill of Lading", "Document or permit needed."),
                            new("Received", "Text", true, "Yes", "Yes or No."),
                            new("Fee Amount", "Number", false, "3500", "Duty, VAT, or service fee."),
                            new("Release Status", "Text", true, "Awaiting Inspection", "Operational stage."),
                            new("Notes", "Text", false, "Inspection booked for 2026-04-29", "Useful context.")
                        ])
                    ]),
                new(
                    "supplier-quote-comparison",
                    "Supplier Quote Comparison",
                    "Compare vendor quotations, lead times, Incoterms, and landed cost assumptions.",
                    ReportTemplateCategory.ImportExport,
                    [
                        new("Quotes", "Evaluate suppliers before confirming a purchase.",
                        [
                            new("Request Reference", "Text", true, "RFQ-2026-019", "Internal sourcing reference."),
                            new("Supplier", "Text", true, "Beijing Export Partners", "Quoted supplier."),
                            new("Commodity", "Text", true, "PCB Assemblies", "Requested item."),
                            new("Incoterm", "Text", true, "FOB", "Commercial term."),
                            new("Lead Time (Days)", "Number", true, "28", "Quoted lead time."),
                            new("Unit Price", "Number", true, "12.7", "Supplier price per unit."),
                            new("Currency", "Text", true, "USD", "Quote currency."),
                            new("Score", "Number", false, "88", "Internal evaluation score.")
                        ])
                    ]),
                new(
                    "startup-kpi-dashboard",
                    "Startup KPI Dashboard",
                    "Lightweight monthly KPI template for startups tracking growth, activation, revenue, and churn.",
                    ReportTemplateCategory.Startup,
                    [
                        new("KPIs", "Summarize the operating metrics a startup reviews every month.",
                        [
                            new("Month", "Date", true, "2026-04-01", "Use the first day of each month."),
                            new("MRR", "Number", true, "42000", "Monthly recurring revenue."),
                            new("New Customers", "Number", true, "65", "Customers added in the month."),
                            new("Churned Customers", "Number", true, "7", "Customers lost in the month."),
                            new("Active Users", "Number", true, "3100", "Monthly active users."),
                            new("CAC", "Number", false, "130", "Customer acquisition cost."),
                            new("LTV", "Number", false, "950", "Estimated customer lifetime value."),
                            new("Notes", "Text", false, "Strong launch in KSA segment", "Context for the month.")
                        ])
                    ]),
                new(
                    "runway-burn-tracker",
                    "Runway and Burn Tracker",
                    "Track cash balance, monthly burn, forecast runway, and funding milestones for startups.",
                    ReportTemplateCategory.Startup,
                    [
                        new("Runway", "Core finance tracker for founders and operators.",
                        [
                            new("Month", "Date", true, "2026-04-01", "Reporting month."),
                            new("Opening Cash", "Number", true, "250000", "Cash at the start of the month."),
                            new("Net Burn", "Number", true, "42000", "Cash burn for the month."),
                            new("Revenue", "Number", false, "18000", "Collected operating revenue."),
                            new("Closing Cash", "Number", true, "208000", "Cash at month end."),
                            new("Runway Months", "Number", true, "5", "Estimated runway remaining."),
                            new("Funding Status", "Text", false, "Raising seed extension", "Investor update."),
                            new("Notes", "Text", false, "Hiring freeze introduced", "Important context.")
                        ])
                    ]),
                new(
                    "sales-pipeline-forecast",
                    "Sales Pipeline Forecast",
                    "Template for startups forecasting pipeline, stage conversion, and expected close value.",
                    ReportTemplateCategory.Startup,
                    [
                        new("Pipeline", "Forecast opportunities and confidence-adjusted revenue.",
                        [
                            new("Opportunity", "Text", true, "Acme Retail", "Prospect or account."),
                            new("Owner", "Text", true, "Sara Ahmed", "Account owner."),
                            new("Stage", "Text", true, "Negotiation", "Pipeline stage."),
                            new("Expected Close", "Date", false, "2026-05-14", "Expected close date."),
                            new("Deal Value", "Number", true, "15000", "Expected contract value."),
                            new("Probability", "Number", false, "65", "Percentage probability."),
                            new("MRR Impact", "Number", false, "2400", "Estimated recurring revenue."),
                            new("Notes", "Text", false, "Needs security review", "Important deal context.")
                        ])
                    ]),
                new(
                    "team-hiring-plan",
                    "Team Hiring Plan",
                    "Headcount planning template for startups aligning roles, hiring dates, and budget impact.",
                    ReportTemplateCategory.Startup,
                    [
                        new("Hiring Plan", "Coordinate recruiting priorities and cost planning.",
                        [
                            new("Role", "Text", true, "Senior Account Executive", "Role to hire."),
                            new("Department", "Text", true, "Sales", "Owning team."),
                            new("Priority", "Text", true, "High", "Hiring urgency."),
                            new("Target Start Date", "Date", false, "2026-06-01", "Planned onboarding date."),
                            new("Monthly Cost", "Number", true, "2800", "Expected monthly employment cost."),
                            new("Recruiter", "Text", false, "Internal", "Owner of hiring process."),
                            new("Status", "Text", true, "Open", "Open, Interviewing, Offer, Filled."),
                            new("Notes", "Text", false, "Needed for GCC expansion", "Why the role matters.")
                        ])
                    ])
            ];
        }
    }
}
