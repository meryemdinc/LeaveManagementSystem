using AutoMapper;
using LeaveManagement.Application.Common.Helpers;
using LeaveManagement.Application.Common.Wrappers;
using LeaveManagement.Application.Contracts.Persistence;
using LeaveManagement.Application.Contracts.Services;
using LeaveManagement.Application.DTOs.LeaveRequest;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using LeaveManagement.Infrastructure.Hubs;

namespace LeaveManagement.Infrastructure.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public LeaveRequestService(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<Response<int>> CreateLeaveRequest(CreateLeaveRequestDto dto)
        {
            // 1. İzin Türünü Bul
            // (LeaveType artık Entity olduğu için veritabanından çekiyoruz)
            // Not: LeaveType tablosu henüz boş olabilir, migration sonrası dolduracağız.
            // Şimdilik ID'si 1 olanı "Yıllık İzin" varsayıyoruz.

            // 2. İş Günü Hesabı (Hafta sonları hariç)
            int requestedDays = DateHelper.CalculateBusinessDays(dto.StartDate, dto.EndDate);

            // 3. Çalışanı Bul ve Bakiye Kontrolü Yap
            var employee = await _unitOfWork.EmployeeRepository.GetByIdAsync(dto.EmployeeId);

            // "Gelen LeaveTypeId, LeaveTypeEnum.Annual (1) değerine eşit mi?" kontrolü
            if (dto.LeaveTypeId == (int)LeaveTypeEnum.Annual)
            {
                if (employee.AnnualLeaveAllowance < requestedDays)
                {
                    return new Response<int>("Yıllık izin bakiyeniz yetersiz!");
                }
            }

            // 4. Kayıt Hazırlığı
            var leaveRequest = _mapper.Map<LeaveRequest>(dto);
            leaveRequest.Status = LeaveStatus.Pending;
            leaveRequest.StartDate = dto.StartDate.ToUniversalTime();
            leaveRequest.EndDate = dto.EndDate.ToUniversalTime();

            // 5. Ekleme (Hafızaya)
            await _unitOfWork.LeaveRequestRepository.AddAsync(leaveRequest);

            // 6. Kaydet (Veritabanına)
            await _unitOfWork.Save();

            await _hubContext.Clients.All.SendAsync("ReceiveLeaveUpdate", "Yeni bir izin talebi oluşturuldu!");

            return new Response<int>(leaveRequest.Id, "İzin talebi başarıyla oluşturuldu.");
        }

        public async Task<Response<bool>> ApproveRequest(int id, bool approved)
        {
            var request = await _unitOfWork.LeaveRequestRepository.GetByIdAsync(id);
            if (request == null) return new Response<bool>("Talep bulunamadı.");

            if (approved)
            {
                request.Status = LeaveStatus.Approved;

                // ONAYLANINCA BAKİYEDEN DÜŞ
                // Enum kontrolü: Eğer yıllık izin ise
                if (request.LeaveTypeId == (int)LeaveTypeEnum.Annual)
                {
                    var employee = await _unitOfWork.EmployeeRepository.GetByIdAsync(request.EmployeeId);
                               int daysToDeduct = DateHelper.CalculateBusinessDays(request.StartDate, request.EndDate);

                    employee.AnnualLeaveAllowance -= daysToDeduct;
                    _unitOfWork.EmployeeRepository.Update(employee); // Güncelle işaretle
                }
            }
            else
            {
                request.Status = LeaveStatus.Rejected;
            }

            _unitOfWork.LeaveRequestRepository.Update(request);
            await _unitOfWork.Save(); // Hepsini tek seferde kaydet

            string message = approved ? $"İzin {id} onaylandı." : $"İzin {id} reddedildi.";
            await _hubContext.Clients.All.SendAsync("ReceiveLeaveUpdate", message);


            return new Response<bool>(true, message);
        }
    }
}