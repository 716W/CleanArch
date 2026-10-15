using AutoMapper;
using GymManagement.Application.Features.Members.Commands.CreateMember;
using GymManagement.Application.Features.Members.DTOs;
using GymManagement.Domain.Entities;

namespace GymManagement.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Member, MemberResponse>();
        CreateMap<CreateMemberCommand, Member>();
    }
}
