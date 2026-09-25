using Microsoft.EntityFrameworkCore; //Entity Framework Core kütüphanesini içe aktarır.
using Scalar.AspNetCore;
using TaskAPI.Core.Middleware;
using TaskAPI.Entities.Interfaces;
using TaskAPI.Infrastructure.Data; //veritabanı bağlantısı için gerekli olan DbContext'i içe aktarır.
using TaskAPI.Infrastructure.Repositories; //veritabanı işlemleri için gerekli olan repository sınıfını içe aktarır.
using TaskAPI.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // eklendi: veritabanı bağlantısı için gerekli olan DbContext'i ekler ve SQL Server kullanır. Connection string, appsettings.json dosyasından alınır.
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, LoginService>();
builder.Services.AddScoped<ITokenService, TokenService>();
var app = builder.Build();
app.UseMiddleware<ErrorHandler>();
// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();