using FluentValidation;


namespace LeaveManagement.Application.DTOs.Employee.Validators
{
    public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
    {
        public CreateEmployeeDtoValidator()
        {
            // İsim Kuralları
            RuleFor(p => p.FirstName)
                .NotEmpty().WithMessage("{PropertyName} alanı boş bırakılamaz.")
                .MaximumLength(50).WithMessage("{PropertyName} 50 karakterden uzun olamaz.");

            RuleFor(p => p.LastName)
                .NotEmpty().WithMessage("{PropertyName} alanı boş bırakılamaz.")
                .MaximumLength(50).WithMessage("{PropertyName} 50 karakterden uzun olamaz.");

            // Email Kuralları (Senin yakaladığın açık burada kapanıyor)
            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("Email adresi gereklidir.")
                .EmailAddress().WithMessage("Lütfen geçerli bir email adresi giriniz.");

            // Şifre Kuralları
            // İleride daha karmaşık (Büyük harf, sayı zorunluluğu vb.) kurallar ekleyebiliriz.
            RuleFor(p => p.Password)
                .NotEmpty().WithMessage("Şifre boş olamaz.")
                .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
        }
    }
}
