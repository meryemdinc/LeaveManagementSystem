using AutoMapper;
using LeaveManagement.Application.Contracts.Persistence;
using LeaveManagement.Application.DTOs.Employee;
using LeaveManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace LeaveManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        // Özel bir EmployeeRepository yazmadığımız için Generic olanı kullanıyoruz
        private readonly IGenericRepository<Employee> _employeeRepository;
    private readonly IMapper _mapper;
        private readonly IValidator<CreateEmployeeDto> _validator; 
        public EmployeesController(
            IGenericRepository<Employee> employeeRepository,
            IMapper mapper,
            IValidator<CreateEmployeeDto> validator)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _validator = validator;
        }

        [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateEmployeeDto employeeDto)
    {
            // 1. DOĞRULAMA (VALIDATION) İŞLEMİ
            var validationResult = await _validator.ValidateAsync(employeeDto);

            // Eğer kurallara uymayan bir durum varsa 400 Bad Request döndür
            if (!validationResult.IsValid)
            {
                // Hataları listeler (Örn: "Email geçersiz", "Şifre kısa")
                return BadRequest(validationResult.Errors);
            }

            var employee = _mapper.Map<Employee>(employeeDto);

        // Şifre hashleme vb. işlemleri Identity kısmında yapacağız, şimdilik düz kaydediyoruz.
        employee.PasswordHash = employeeDto.Password;

        await _employeeRepository.CreateAsync(employee);

        return Ok(employee.Id); // Oluşan çalışanın ID'sini döner
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var employees = await _employeeRepository.GetAllAsync();
        return Ok(employees);
    } }
}
