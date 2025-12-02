using AutoMapper;
using LeaveManagement.Application.Common.Wrappers;
using LeaveManagement.Application.Contracts.Persistence; // Okuma için Repo lazım
using LeaveManagement.Application.Contracts.Services;    // Yazma için Servis lazım
using LeaveManagement.Application.DTOs.LeaveRequest;
using LeaveManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LeaveManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveRequestsController : ControllerBase
    {
        // 1. Repository: Sadece Veri Okumak (GET) İçin
        private readonly ILeaveRequestRepository _leaveRequestRepository;

        // 2. Service: Veri Yazmak, Hesaplamak ve Bildirim (POST/PUT) İçin
        private readonly ILeaveRequestService _leaveRequestService;

        private readonly IMapper _mapper;

        public LeaveRequestsController(
            ILeaveRequestRepository leaveRequestRepository,
            ILeaveRequestService leaveRequestService,
            IMapper mapper)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _leaveRequestService = leaveRequestService;
            _mapper = mapper;
        }

        // YARDIMCI METOT
        private int GetUserIdFromToken()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "uid");
            if (userIdClaim == null) return 0;
            return int.Parse(userIdClaim.Value);
        }

        // --- GET METOTLARI (Doğrudan Repository Kullanabilir - Performans İçin) ---

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<LeaveRequestListDto>>> Get()
        {
            var leaveRequests = await _leaveRequestRepository.GetLeaveRequestsWithDetails();
            var leaveRequestsDto = _mapper.Map<List<LeaveRequestListDto>>(leaveRequests);
            return Ok(leaveRequestsDto);
        }

        [HttpGet("MyLeaves")]
        public async Task<ActionResult<List<LeaveRequestListDto>>> GetMyLeaves()
        {
            int userId = GetUserIdFromToken();
            var leaveRequests = await _leaveRequestRepository.GetLeaveRequestsOfEmployee(userId);
            var leaveRequestsDto = _mapper.Map<List<LeaveRequestListDto>>(leaveRequests);
            return Ok(leaveRequestsDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveRequestListDto>> Get(int id)
        {
            var leaveRequest = await _leaveRequestRepository.GetLeaveRequestsWithDetails(id);
            if (leaveRequest == null) return NotFound();

            var leaveRequestDto = _mapper.Map<LeaveRequestListDto>(leaveRequest);
            return Ok(leaveRequestDto);
        }

        // --- İŞLEM METOTLARI (Mutlaka Service Kullanmalı - Logic İçin) ---

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateLeaveRequestDto leaveRequestDto)
        {
            // Token'dan ID'yi alıp DTO'ya ekle (Güvenlik)
            leaveRequestDto.EmployeeId = GetUserIdFromToken();

            // Servise gönder (Bakiye kontrolü, Hafta sonu hesabı, SignalR burada yapılır)
            var response = await _leaveRequestService.CreateLeaveRequest(leaveRequestDto);

            if (response.Succeeded)
            {
                // Başarılıysa veriyi dön
                return Ok(response);
            }

            // Hata varsa (Örn: Bakiye yetersiz) 400 Bad Request dön
            return BadRequest(new { message = response.Message });
        }

        [HttpPut("ChangeApproval/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ChangeApproval(int id, [FromBody] ChangeLeaveRequestApprovalDto dto)
        {
            // Onaylama işlemi de serviste yapılmalı (Bakiye düşme, SignalR)
            var response = await _leaveRequestService.ApproveRequest(id, dto.Approved);

            if (response.Succeeded)
            {
                return Ok(response);
            }

            return BadRequest(new { message = response.Message });
        }
    }
}