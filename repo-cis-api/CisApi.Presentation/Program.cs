using CisApi.Business.Interfaces;
using CisApi.Business.Services;
using CisApi.Data;
using CisApi.Data.Interfaces;
using CisApi.Data.Repositories;
using CisApi.Presentation.Middleware;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// MongoDB
builder.Services.AddSingleton<IMongoDbContext, MongoDbContext>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configure HTTP Client para a Users API
builder.Services.AddHttpClient<IUsersApiAuthService, UsersApiAuthService>(client =>
{
    var baseUrl = builder.Configuration["UsersApi:BaseUrl"] ?? "http://localhost:8080";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        new UnprocessableEntityObjectResult(context.ModelState);
});

// Dependency Injection
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();
builder.Services.AddScoped<IIdeaRepository, IdeaRepository>();
builder.Services.AddScoped<VoteService>();
builder.Services.AddScoped<IIdeaService, IdeaService>();

var app = builder.Build();

app.UsePathBase("/cis-api/v1");
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<UsersApiAuthMiddleware>();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));
app.MapControllers();
app.Run();