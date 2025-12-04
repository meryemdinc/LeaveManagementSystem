using AutoMapper;
using LeaveManagement.Application.DTOs.Employee;
using LeaveManagement.Application.DTOs.LeaveRequest;
using LeaveManagement.Domain.Entities;


namespace LeaveManagement.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // İki yönlü dönüşüm (ReverseMap)
            // LeaveRequest -> LeaveRequestListDto Dönüşümü
            CreateMap<LeaveRequest, LeaveRequestListDto>()
                // "Git, LeaveRequest içindeki LeaveType nesnesine bak, onun Name özelliğini al"
                .ForMember(dest => dest.LeaveType, opt => opt.MapFrom(src => src.LeaveType.Name))
                .ReverseMap();
            CreateMap<LeaveRequest, CreateLeaveRequestDto>().ReverseMap();
            CreateMap<Employee, EmployeeDto>().ReverseMap();
            CreateMap<Employee, CreateEmployeeDto>().ReverseMap();
        }
    }
}
