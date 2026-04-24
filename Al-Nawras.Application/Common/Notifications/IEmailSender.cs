namespace Al_Nawras.Application.Common.Notifications
{
    public interface IEmailSender
    {
        Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody,
            CancellationToken cancellationToken = default);
    }
}
