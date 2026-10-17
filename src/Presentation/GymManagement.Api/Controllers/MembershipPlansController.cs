using GymManagement.Application.Features.MembershipPlans.Commands.ActivateMembershipPlan;
using GymManagement.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;
using GymManagement.Application.Features.MembershipPlans.Commands.DeactivateMembershipPlan;
using GymManagement.Application.Features.MembershipPlans.Commands.UpdateMembershipPlan;
using GymManagement.Application.Features.MembershipPlans.DTOs;
using GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlanById;
using GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlans;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers;

[ApiController]
[Route("api/membership-plans")]
[Produces("application/json")]
public class MembershipPlansController(ISender sender) : ControllerBase
{
    /// <summary>Creates a new membership plan.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MembershipPlanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MembershipPlanResponse>> Create(
        [FromBody] CreateMembershipPlanCommand command,
        CancellationToken ct)
    {
        var plan = await sender.Send(command, ct);

        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    /// <summary>Returns all membership plans, optionally filtered by active status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MembershipPlanResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MembershipPlanResponse>>> GetAll(
        [FromQuery] bool? activeOnly,
        CancellationToken ct)
    {
        var plans = await sender.Send(new GetMembershipPlansQuery(activeOnly), ct);

        return Ok(plans);
    }

    /// <summary>Returns a single membership plan by its ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MembershipPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MembershipPlanResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        // Handler throws KeyNotFoundException when not found.
        // Global exception handling (added in the next task) will map this to 404.
        var plan = await sender.Send(new GetMembershipPlanByIdQuery(id), ct);

        return Ok(plan);
    }

    /// <summary>Updates an existing membership plan. IsActive is not affected by this endpoint.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MembershipPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MembershipPlanResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateMembershipPlanCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
        {
            ModelState.AddModelError(nameof(command.Id), "Route ID must match the request body ID.");
            return ValidationProblem(ModelState);
        }

        var plan = await sender.Send(command, ct);

        if (plan is null)
            return NotFound();

        return Ok(plan);
    }

    /// <summary>Activates a membership plan so it can be subscribed to.</summary>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var success = await sender.Send(new ActivateMembershipPlanCommand(id), ct);

        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>Deactivates a membership plan. Existing membership records are preserved.</summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var success = await sender.Send(new DeactivateMembershipPlanCommand(id), ct);

        if (!success)
            return NotFound();

        return NoContent();
    }
}
