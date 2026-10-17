using AutoMapper;
using GymManagement.Application.Features.Members.Commands.CreateMember;
using GymManagement.Application.Features.Members.DTOs;
using GymManagement.Application.Features.MembershipPlans.DTOs;
using GymManagement.Application.Features.Memberships.DTOs;
using GymManagement.Domain.Entities;

namespace GymManagement.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Member, MemberResponse>();
        CreateMap<CreateMemberCommand, Member>();
        CreateMap<Membership, MembershipResponse>()
            .ForMember(
                destination => destination.Status,
                options => options.MapFrom(source => source.Status.ToString()));
        CreateMap<MembershipPlan, MembershipPlanResponse>();
    }
}

