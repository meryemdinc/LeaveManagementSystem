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

        // GET: api/Employees/Export/Template
        [HttpGet("Export/Template")]
        public IActionResult DownloadTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Personel_Sablonu");

                // 1. BAŞLIKLAR (Import mantığımızla birebir aynı sırada olmalı)
                // Hatırlatma: Import kodunda 1. sütunu (ID) atlayıp 2'den başlıyorduk.
                worksheet.Cell(1, 1).Value = "ID (Boş Bırakın)";
                worksheet.Cell(1, 2).Value = "Ad";
                worksheet.Cell(1, 3).Value = "Soyad";
                worksheet.Cell(1, 4).Value = "E-Posta";
                worksheet.Cell(1, 5).Value = "Rol (Employee/Admin)";
                worksheet.Cell(1, 6).Value = "İzin Hakkı";

                // Başlık Stili
                var headerRange = worksheet.Range("A1:F1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // 2. ÖRNEK VERİ (Kullanıcı ne yazacağını anlasın diye)
                worksheet.Cell(2, 1).Value = ""; // ID boş
                worksheet.Cell(2, 2).Value = "Ahmet";
                worksheet.Cell(2, 3).Value = "Yılmaz";
                worksheet.Cell(2, 4).Value = "ahmet.yilmaz@ornek.com";
                worksheet.Cell(2, 5).Value = "Employee";
                worksheet.Cell(2, 6).Value = 14;

                // Sütunları genişlet
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Personel_Yukleme_Sablonu.xlsx");
                }
            }
        }

        // POST: api/Employees/Import/Excel
        [HttpPost("Import/Excel")]
        public async Task<ActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            // Excel dosyasını okumak için
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1); // İlk sayfayı al
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Başlığı atla (Skip 1)

                    foreach (var row in rows)
                    {
                        // Excel Sütunları:
                        // 1: ID (Boşver)
                        // 2: Ad, 3: Soyad, 4: Email, 5: Rol, 6: İzin Hakkı

                        var email = row.Cell(4).GetValue<string>();

                        // Mükerrer Kayıt Kontrolü (Aynı mail varsa atla)
                        var exists = await _unitOfWork.EmployeeRepository.GetAll()
                            .AnyAsync(x => x.Email == email);

                        if (exists) continue; // Bu kişiyi atla, sonrakine geç

                        var employee = new Employee
                        {
                            FirstName = row.Cell(2).GetValue<string>(),
                            LastName = row.Cell(3).GetValue<string>(),
                            Email = email,
                            Role = row.Cell(5).GetValue<string>() ?? "Employee", // Boşsa Employee yap
                            AnnualLeaveAllowance = row.Cell(6).GetValue<int>(),

                            // Varsayılan Şifre: "123456" (Gerçek hayatta mail atılır)
                            PasswordHash = "123456",
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false
                        };

                        await _unitOfWork.EmployeeRepository.AddAsync(employee);
                    }

                    await _unitOfWork.Save();
                }
            }

            return Ok(new { message = "İçe aktarım tamamlandı." });
        }
    }
}