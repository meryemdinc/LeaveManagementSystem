using FluentValidation;

namespace LeaveManagement.Application.DTOs.LeaveRequest.Validators
{
    public class CreateLeaveRequestDtoValidator : AbstractValidator<CreateLeaveRequestDto>
    {
        public CreateLeaveRequestDtoValidator()
        {
            RuleFor(p => p.StartDate)
                .LessThan(p => p.EndDate).WithMessage("{PropertyName} (Başlangıç), Bitiş tarihinden önce olmalıdır.");

            RuleFor(p => p.EndDate)
                .GreaterThan(p => p.StartDate).WithMessage("{PropertyName} (Bitiş), Başlangıç tarihinden sonra olmalıdır.");
            // --- DÜZELTME BURADA ---
            // Eskiden: RuleFor(p => p.LeaveType).IsInEnum();
            // Yeni: Artık int (ID) olduğu için 0'dan büyük olmalı diyoruz.
            RuleFor(p => p.LeaveTypeId)
                .GreaterThan(0).WithMessage("Lütfen geçerli bir izin türü seçiniz.");
            // ------

            RuleFor(p => p.Reason)
                .NotEmpty().WithMessage("İzin nedeni belirtmelisiniz.")
                .MaximumLength(300).WithMessage("İzin nedeni 300 karakteri geçemez.");
        }
    }
}
