using AutoMapper;
using FluentValidation;
using LeaveManagement.Application.Contracts.Persistence;
using LeaveManagement.Application.DTOs.LeaveRequest;
using LeaveManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateLeaveRequestDto> _validator;

        public LeaveRequestsController(ILeaveRequestRepository leaveRequestRepository, 
            IMapper mapper,
           IValidator<CreateLeaveRequestDto> validator)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _mapper = mapper;
            _validator = validator;
        }


        // Token'ın içindeki "uid" (User Id) bilgisini okuyan yardımcı metot
        private int GetUserIdFromToken()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "uid");
   
            if (userIdClaim == null) return 0;

            return int.Parse(userIdClaim.Value);
        }


        // GET: api/LeaveRequests
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<LeaveRequestListDto>>> Get()
        {
            // 1. Repository'den veriyi (Entity Listesi) çek
            var leaveRequests = await _leaveRequestRepository.GetLeaveRequestsWithDetails();

            // 2. Entity Listesini -> DTO Listesine çevir
            var leaveRequestsDto = _mapper.Map<List<LeaveRequestListDto>>(leaveRequests);

            // 3. 200 OK ile döndür
            return Ok(leaveRequestsDto);
        }

        // GET: api/LeaveRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveRequestListDto>> Get(int id)
        {
            // Generic Repository yerine, Include içeren özel metodumuzu kullanalım:
            var leaveRequest = await _leaveRequestRepository.GetLeaveRequestsWithDetails(id);

            if (leaveRequest == null)
            {
                return NotFound();
            }

            var leaveRequestDto = _mapper.Map<LeaveRequestListDto>(leaveRequest);
            return Ok(leaveRequestDto);
        }


        // POST: api/LeaveRequests
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateLeaveRequestDto leaveRequestDto)
        {
            // 1. DOĞRULAMA
            var validationResult = await _validator.ValidateAsync(leaveRequestDto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            // 2. MAPPING
            var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestDto);

            // 3. TARİHLERİ UTC YAP
            leaveRequest.StartDate = leaveRequest.StartDate.ToUniversalTime();
            leaveRequest.EndDate = leaveRequest.EndDate.ToUniversalTime();

            // --- KRİTİK GÜVENLİK HAMLESİ ---
            // Kullanıcı JSON içinde hangi ID'yi gönderirse göndersin,
            // biz Token'dan okuduğumuz "gerçek" kullanıcı ID'sini basıyoruz.
            leaveRequest.EmployeeId = GetUserIdFromToken();
            // --------------------------------

            // 4. KAYDET
            await _leaveRequestRepository.CreateAsync(leaveRequest);

            return CreatedAtAction(nameof(Get), new { id = leaveRequest.Id }, leaveRequest);
        }

        // GET: api/LeaveRequests/MyLeaves
        [HttpGet("MyLeaves")] // Adres: .../api/LeaveRequests/MyLeaves
        [Authorize] // Herkes girebilir (Admin veya Employee)
        public async Task<ActionResult<List<LeaveRequestListDto>>> GetMyLeaves()
        {
            // 1. Token'dan giriş yapan kişinin ID'sini al
            int userId = GetUserIdFromToken();

            // 2. O kişiye ait izinleri çek
            var leaveRequests = await _leaveRequestRepository.GetLeaveRequestsOfEmployee(userId);

            var leaveRequestsDto = _mapper.Map<List<LeaveRequestListDto>>(leaveRequests);
            return Ok(leaveRequestsDto);
        }


    }
}