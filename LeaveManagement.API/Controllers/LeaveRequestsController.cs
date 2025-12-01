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
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
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
            // Detaylı veriyi çekiyoruz (Infrastructure'da yazdığımız özel metod)
            // Not: Metod List dönüyordu, FirstOrDefault ile tekile indiriyoruz.
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
            // Veya detaylı istiyorsak: (await _leaveRequestRepository.GetLeaveRequestsWithDetails(id)).FirstOrDefault();

            if (leaveRequest == null)
            {
                return NotFound(); // 404 Döndür
            }

            var leaveRequestDto = _mapper.Map<LeaveRequestListDto>(leaveRequest);
            return Ok(leaveRequestDto);
        }

        // POST: api/LeaveRequests
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateLeaveRequestDto leaveRequestDto)
        {
            // 1. DOĞRULAMA (VALIDATION) İŞLEMİ
            var validationResult = await _validator.ValidateAsync(leaveRequestDto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            // 1. Gelen DTO'yu Entity'e çevir
            var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestDto);
            leaveRequest.StartDate = leaveRequest.StartDate.ToUniversalTime();
            leaveRequest.EndDate = leaveRequest.EndDate.ToUniversalTime();//tarihleri UTC formatına zorluyoruz
            // 2. Veritabanına kaydet
            await _leaveRequestRepository.CreateAsync(leaveRequest);

            // 3. Başarılı (201 Created) döndür
            return CreatedAtAction(nameof(Get), new { id = leaveRequest.Id }, leaveRequest);
        }


    }
}