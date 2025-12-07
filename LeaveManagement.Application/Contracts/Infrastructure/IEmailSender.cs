using LeaveManagement.Application.DTOs.Email;

//IEmailSender.cs
namespace LeaveManagement.Application.Contracts.Infrastructure
{
    public interface IEmailSender
    {
        Task<bool> SendEmail(EmailRequest email);
    }
}