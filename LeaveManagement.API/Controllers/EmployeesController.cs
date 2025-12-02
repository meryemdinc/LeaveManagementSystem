using AutoMapper;
using FluentValidation;
using LeaveManagement.Application.Contracts.Persistence;
using LeaveManagement.Application.DTOs.Employee;
using LeaveManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ToListAsync için

namespace LeaveManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        // ARTIK REPOSITORY DEĞİL, UNIT OF WORK KULLANIYORUZ
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateEmployeeDto> _validator;

        public EmployeesController(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CreateEmployeeDto> validator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateEmployeeDto employeeDto)
        {
            // 1. Validasyon
            var validationResult = await _validator.ValidateAsync(employeeDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            // 2. Mapping
            var employee = _mapper.Map<Employee>(employeeDto);

            // Şifre hashleme (Şimdilik düz metin)
            employee.PasswordHash = employeeDto.Password;

            // Varsayılan Yıllık İzin Hakkı (Yeni başlayan birine 14 gün verelim)
            employee.AnnualLeaveAllowance = 14;

            // 3. EKLEME (Hafızaya)
            await _unitOfWork.EmployeeRepository.AddAsync(employee);

            // 4. KAYDETME (Veritabanına) - İŞTE EKSİK OLAN BUYDU!
            await _unitOfWork.Save();

            return Ok(employee.Id); // Artık gerçek ID dönecek (1, 2, 3...)
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            // UnitOfWork üzerinden çekiyoruz
            var employees = await _unitOfWork.EmployeeRepository.GetAll().ToListAsync();
            return Ok(employees);
        }
    }
}