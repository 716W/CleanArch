using GymManagement.Application;
using GymManagement.Application.Features.Members.Commands.CreateMember;
using GymManagement.Application.Features.Members.Queries.GetMember;
using GymManagement.Application.Features.Members.Queries.GetMembers;
using GymManagement.Infrastructure;
using MediatR;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    });

    // Add services to the container.
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.MapControllers();

    // Configure the HTTP request pipeline.
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();

    // // Member Endpoints
    // app.MapPost("/api/members", async (CreateMemberCommand command, ISender sender, CancellationToken cancellationToken) =>
    // {
    //     var result = await sender.Send(command, cancellationToken);
    //     return Results.Created($"/api/members/{result.Id}", result);
    // });

    // app.MapGet("/api/members/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
    // {
    //     var result = await sender.Send(new GetMemberQuery(id), cancellationToken);
    //     return result is not null ? Results.Ok(result) : Results.NotFound();
    // });

    // app.MapGet("/api/members", async (ISender sender, CancellationToken cancellationToken) =>
    // {
    //     var result = await sender.Send(new GetMembersQuery(), cancellationToken);
    //     return Results.Ok(result);
    // });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


