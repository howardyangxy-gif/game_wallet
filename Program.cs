using app.Services;
using app.Infrastructure;
using app.Servers.Agent.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 註冊dbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// DI 註冊 Services
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<PlayerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 模組化註冊 Endpoints
app.MapAgentEndpoints();
app.MapGameEndpoints();
app.MapSessionEndpoints();
app.MapPlayerEndpoints();

app.MapGet("/", () => "Center API is running!");
app.Run();

