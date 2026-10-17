using AutoMapper;
using GymManagement.Application.Features.Memberships.Commands.CreateMemberships;
using GymManagement.Application.Features.Memberships.Queries.GetMembershipById;
using GymManagement.Application.Features.Memberships.Queries.GetMembershipsByMember;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembershipsController : ControllerBase
{
    private readonly ISender _sender;

    public MembershipsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateMembershipCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await _sender.Send(
            command,
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result
        );
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
    Guid id,
    CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMembershipByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("member/{memberId:guid}")]
    public async Task<IActionResult> GetByMemberId(
    Guid memberId,
    CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMembershipsByMemberQuery(memberId),
            cancellationToken);

        return Ok(result);
    }
}