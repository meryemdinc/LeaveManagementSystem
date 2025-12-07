using LeaveManagement.Application.DTOs.Email;


namespace LeaveManagement.Application.Contracts.Infrastructure
{
    public interface IEmailSender
    {
        Task<bool> SendEmail(EmailRequest email);
    }
}