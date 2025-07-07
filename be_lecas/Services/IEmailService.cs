namespace be_lecas.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendWelcomeEmailAsync(string to, string userName);
        Task<bool> SendOrderConfirmationAsync(string to, string orderNumber, decimal total);
        Task<bool> SendPasswordResetAsync(string to, string resetToken);
        Task<bool> SendEmailVerificationAsync(string to, string userName, string verificationToken);
        Task SendOrderStatusUpdateAsync(string toName, string orderNumber, string status, string message);
    }
}

