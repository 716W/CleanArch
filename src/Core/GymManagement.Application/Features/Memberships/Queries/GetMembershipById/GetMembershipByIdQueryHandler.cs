using AutoMapper;
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Features.Memberships.DTOs;
using MediatR;

namespace GymManagement.Application.Features.Memberships.Queries.GetMembershipById;

public class GetMembershipByIdQueryHandler : IRequestHandler<GetMembershipByIdQuery, MembershipResponse>
{
    private readonly IMembershipRepository _repository;
    private readonly IMapper _mapper;

    public GetMembershipByIdQueryHandler(IMembershipRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MembershipResponse> Handle(GetMembershipByIdQuery request, CancellationToken cancellationToken)
    {
        var membership = await _repository.GetByIdAsync(
        request.id,
        cancellationToken
        );

        if (membership is null)
            throw new KeyNotFoundException($"Membership with id {request.id} not found.");

        return _mapper.Map<MembershipResponse>(membership);
    }
}