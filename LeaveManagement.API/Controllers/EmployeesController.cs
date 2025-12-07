using AutoMapper;
using FluentValidation;
using LeaveManagement.Application.Contracts.Persistence;
using LeaveManagement.Application.DTOs.Employee;
using LeaveManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ToListAsync için
using ClosedXML.Excel;

namespace LeaveManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
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
            employee.CreatedDate = DateTime.UtcNow;

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

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var employee = await _unitOfWork.EmployeeRepository.GetByIdAsync(id);
            if (employee == null) return NotFound();
            return Ok(employee);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] EmployeeDto employeeDto)
        {
            var employee = await _unitOfWork.EmployeeRepository.GetByIdAsync(id);
            if (employee == null) return NotFound();

            // Sadece değişmesine izin verdiğimiz alanları güncelleyelim
            employee.FirstName = employeeDto.FirstName;
            employee.LastName = employeeDto.LastName;
            employee.Email = employeeDto.Email;
            employee.AnnualLeaveAllowance = employeeDto.AnnualLeaveAllowance;
            employee.Role = employeeDto.Role;
            // Şifre güncelleme mantığı güvenlik gereği genelde ayrı yapılır, şimdilik atlıyoruz.

            _unitOfWork.EmployeeRepository.Update(employee);
            await _unitOfWork.Save();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var employee = await _unitOfWork.EmployeeRepository.GetByIdAsync(id);
            if (employee == null) return NotFound();

            // Hard Delete (Veritabanından tamamen siler)
            // İsterseniz Soft Delete (IsDeleted = true) yapabilirsiniz.
            _unitOfWork.EmployeeRepository.Delete(employee);
            await _unitOfWork.Save();

            return NoContent();
        }

        // GET: api/Employees/Export/Excel
        [HttpGet("Export/Excel")]
        public async Task<IActionResult> ExportExcel()
        {
            // 1. Verileri Çek
            var employees = await _unitOfWork.EmployeeRepository.GetAll().ToListAsync();

            // 2. Excel Dosyası Oluştur (Hafızada)
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Çalışanlar");

                // 3. Başlıkları Yaz
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Ad";
                worksheet.Cell(1, 3).Value = "Soyad";
                worksheet.Cell(1, 4).Value = "E-Posta";
                worksheet.Cell(1, 5).Value = "Rol";
                worksheet.Cell(1, 6).Value = "İzin Hakkı";
                worksheet.Cell(1, 7).Value = "Kayıt Tarihi";

                // Başlıkları Kalın Yap ve Arka Planı Boya
                var headerRange = worksheet.Range("A1:G1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
                headerRange.Style.Font.FontColor = XLColor.White;

                // 4. Verileri Satır Satır Yaz
                int row = 2;
                foreach (var emp in employees)
                {
                    worksheet.Cell(row, 1).Value = emp.Id;
                    worksheet.Cell(row, 2).Value = emp.FirstName;
                    worksheet.Cell(row, 3).Value = emp.LastName;
                    worksheet.Cell(row, 4).Value = emp.Email;
                    worksheet.Cell(row, 5).Value = emp.Role;
                    worksheet.Cell(row, 6).Value = emp.AnnualLeaveAllowance;
                    worksheet.Cell(row, 7).Value = emp.CreatedDate.ToShortDateString();
                    row++;
                }

                // 5. Sütun Genişliklerini Otomatik Ayarla
                worksheet.Columns().AdjustToContents();

                // 6. Dosyayı Stream Olarak Döndür
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Calisanlar_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
                }
            }
        }
    }
}