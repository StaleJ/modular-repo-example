using Customer.Contracts;
using Customer.Infrastructure;
using LoanApplication.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Read connection strings
var customerDb = builder.Configuration.GetConnectionString("CustomerDb") ?? "Data Source=./customer.db";
var loanAppDb = builder.Configuration.GetConnectionString("LoanAppDb") ?? "Data Source=./loanapp.db";

// Register modules (each with its own DbContext/connection)
builder.Services.AddCustomerModule(customerDb);
builder.Services.AddLoanApplicationModule(loanAppDb);

// MVC & Controllers (include controllers from module assemblies)
builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(Customer.Infrastructure.Api.External.CustomersController).Assembly)
    .AddApplicationPart(typeof(LoanApplication.Infrastructure.Api.External.LoanApplicationsController).Assembly);

// Swagger with grouped documents per module & visibility
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("Customer.Internal", new OpenApiInfo { Title = "Customer - Internal", Version = "v1" });
    c.SwaggerDoc("Customer.External", new OpenApiInfo { Title = "Customer - External", Version = "v1" });
    c.SwaggerDoc("LoanApplication.Internal", new OpenApiInfo { Title = "LoanApplication - Internal", Version = "v1" });
    c.SwaggerDoc("LoanApplication.External", new OpenApiInfo { Title = "LoanApplication - External", Version = "v1" });

    // Use the ApiExplorer GroupName from controllers to include in matching doc
    c.DocInclusionPredicate((docName, apiDesc) => apiDesc.GroupName == docName);
});

var app = builder.Build();

// Ensure databases are created (dev/demo only)
using (var scope = app.Services.CreateScope())
{
    var custDb = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    await custDb.Database.MigrateAsync();
    var loanDb = scope.ServiceProvider.GetRequiredService<LoanAppDbContext>();
    await loanDb.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/Customer.Internal/swagger.json", "Customer - Internal");
        c.SwaggerEndpoint("/swagger/Customer.External/swagger.json", "Customer - External");
        c.SwaggerEndpoint("/swagger/LoanApplication.Internal/swagger.json", "LoanApplication - Internal");
        c.SwaggerEndpoint("/swagger/LoanApplication.External/swagger.json", "LoanApplication - External");
        c.DisplayOperationId();
        c.DisplayRequestDuration();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    });
}

app.MapControllers();

app.Run();
