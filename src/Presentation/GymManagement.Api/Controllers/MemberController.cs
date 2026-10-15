using GymManagement.Application.Features.Members.Commands.CreateMember;
using GymManagement.Application.Features.Members.Commands.DeleteMember;
using GymManagement.Application.Features.Members.Commands.UpdateMember;
using GymManagement.Application.Features.Members.DTOs;
using GymManagement.Application.Features.Members.Queries.GetMember;
using GymManagement.Application.Features.Members.Queries.GetMembers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MemberController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MemberResponse>> Create(
        [FromBody] CreateMemberCommand command,
        CancellationToken ct
    )
    {
        var member = await sender.Send(command, ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = member.Id },
            member
        );
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct
    )
    {
        var member = await sender.Send(
            new GetMemberQuery(id),
            ct
        );

        if (member is null)
            return NotFound();

        return Ok(member);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MemberResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MemberResponse>>> GetAll(
        CancellationToken ct
    )
    {
        var members = sender.Send(
            new GetMembersQuery(),
            ct
        );

        return Ok(members);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MemberResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateMemberCommand command,
        CancellationToken ct
    )
    {
        if (id != command.id)
        {
            ModelState.AddModelError(
                nameof(command.id),
                "Route ID must match the request ID ."
            );
            return ValidationProblem(ModelState);
        }

        var member = await sender.Send(command, ct);

        if (member is null)
            return NotFound();

        return Ok(member);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct
    )
    {
        var deleted = await sender.Send(
            new DeleteMemberCommand(id),
            ct
        );

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}