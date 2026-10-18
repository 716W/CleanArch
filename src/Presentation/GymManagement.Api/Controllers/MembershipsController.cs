using GymManagement.Application.Features.Memberships.Commands.CreateMemberships;
using GymManagement.Application.Features.Memberships.DTOs;
using GymManagement.Application.Features.Memberships.Queries.GetMembershipById;
using GymManagement.Application.Features.Memberships.Queries.GetMembershipsByMember;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers;

[ApiController]
[Route("api/memberships")]
[Produces("application/json")]
public class MembershipsController : ControllerBase
{
    private readonly ISender _sender;

    public MembershipsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(MembershipResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MembershipResponse>> Create(
        [FromBody] CreateMembershipCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MembershipResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MembershipResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMembershipByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("member/{memberId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<MembershipResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<MembershipResponse>>> GetByMemberId(
        [FromRoute] Guid memberId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMembershipsByMemberQuery(memberId),
            cancellationToken);

        return Ok(result);
    }
}