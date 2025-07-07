using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace be_lecas.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly bool _useTls;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
            _useTls = bool.Parse(_configuration["EmailSettings:UseTls"] ?? "true");
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    EnableSsl = _useTls,
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_smtpUsername),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string to, string userName)
        {
            var subject = "Chào mừng bạn đến với LECAS Fashion!";
            var body = $@"
                <h2>Chào mừng {userName}!</h2>
                <p>Cảm ơn bạn đã đăng ký tài khoản tại LECAS Fashion.</p>
                <p>Chúng tôi rất vui mừng được phục vụ bạn!</p>
                <br>
                <p>Trân trọng,</p>
                <p>Đội ngũ LECAS Fashion</p>";

            return await SendEmailAsync(to, subject, body);
        }

        public async Task<bool> SendOrderConfirmationAsync(string to, string orderNumber, decimal total)
        {
            var subject = $"Xác nhận đơn hàng #{orderNumber}";
            var body = $@"
                <h2>Xác nhận đơn hàng</h2>
                <p>Đơn hàng #{orderNumber} của bạn đã được xác nhận.</p>
                <p>Tổng tiền: {total:N0} VNĐ</p>
                <p>Chúng tôi sẽ thông báo khi đơn hàng được giao.</p>
                <br>
                <p>Trân trọng,</p>
                <p>Đội ngũ LECAS Fashion</p>";

            return await SendEmailAsync(to, subject, body);
        }

        public async Task<bool> SendPasswordResetAsync(string to, string resetToken)
        {
            var subject = "Đặt lại mật khẩu - LECAS Fashion";
            var body = $@"
                <h2>Đặt lại mật khẩu</h2>
                <p>Bạn đã yêu cầu đặt lại mật khẩu.</p>
                <p>Mã xác nhận: <strong>{resetToken}</strong></p>
                <p>Mã này có hiệu lực trong 10 phút.</p>
                <br>
                <p>Trân trọng,</p>
                <p>Đội ngũ LECAS Fashion</p>";

            return await SendEmailAsync(to, subject, body);
        }

        public async Task<bool> SendEmailVerificationAsync(string to, string userName, string verificationToken)
        {
            var subject = "Xác nhận email - LECAS Fashion";
            var verificationUrl = $"http://localhost:3000/verify-email?token={verificationToken}";
            
            var body = $@"
                <h2>Xác nhận email</h2>
                <p>Chào {userName}!</p>
                <p>Cảm ơn bạn đã đăng ký tài khoản tại LECAS Fashion.</p>
                <p>Vui lòng nhấp vào liên kết bên dưới để xác nhận email của bạn:</p>
                <p><a href='{verificationUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Xác nhận Email</a></p>
                <p>Hoặc copy link này vào trình duyệt: {verificationUrl}</p>
                <p>Liên kết này có hiệu lực trong 24 giờ.</p>
                <br>
                <p>Trân trọng,</p>
                <p>Đội ngũ LECAS Fashion</p>";

            return await SendEmailAsync(to, subject, body);
        }
    }
}

