using EcomCourse.Api.Customers;
using EcomCourse.Api.Middleware;
using EcomCourse.Application;
using EcomCourse.Application.Customers.GetCustomerById;
using EcomCourse.Application.Customers.RegisterCustomer;
using EcomCourse.Domain.Customers;
using EcomCourse.Infrastructure;
using EcomCourse.Infrastructure.Persistence;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () =>
{
    return "Everything is okay";
})
.WithName("GetHealthCheck");

app.MapPost("/customers/register", async (
    RegisterCustomerRequest request,
    ISender sender,
    CancellationToken cancellationToken) =>
{
    var command = new RegisterCustomerCommand(
        request.UserId,
        request.Name,
        request.Email,
        request.Street,
        request.City,
        request.PostalCode,
        request.Country);

    var result = await sender.Send(command, cancellationToken);

    if (result.IsFailure)
    {
        var error = new
        {
            result.Error.Code,
            result.Error.Description
        };

        if (result.Error == CustomerErrors.EmailAlreadyExists)
        {
            return Results.Conflict(error);
        }

        return Results.BadRequest(error);
    }

    return Results.Created($"/customers/{result.Value}", result.Value);
})
.WithName("RegisterCustomer")
.WithTags("Customers");

app.MapGet("/customers/{id:guid}", async (
    Guid id,
    ISender sender,
    CancellationToken cancellationToken) =>
{
    var query = new GetCustomerByIdQuery(id);

    var result = await sender.Send(query, cancellationToken);

    if (result.IsFailure)
    {
        return Results.NotFound(new
        {
            result.Error.Code,
            result.Error.Description
        });
    }

    return Results.Ok(result.Value);
})
.WithName("GetCustomerById")
.WithTags("Customers");

app.MapControllers();

app.Run();

public partial class Program;
