using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore; //Entity Framework Core kütüphanesini içe aktarır.
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;
using TaskAPI.Core.Middleware;
using TaskAPI.Application.Interfaces;
using TaskAPI.Infrastructure.Data; //veritabanı bağlantısı için gerekli olan DbContext'i içe aktarır.
using TaskAPI.Infrastructure.Repositories; //veritabanı işlemleri için gerekli olan repository sınıfını içe aktarır.
using TaskAPI.Infrastructure.Security;
using TaskAPI.Application.Services;
using TaskAPI.Application.Dtos;
using TaskAPI.Application.Mapping;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // eklendi: veritabanı bağlantısı için gerekli olan DbContext'i ekler ve SQL Server kullanır. Connection string, appsettings.json dosyasından alınır.
        
        
        builder.Services.AddScoped<ITaskRepository, TaskRepository>();
        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddSingleton<IPepperKeyProvider, TaskAPI.Infrastructure.Security.ConfigurationProvider>();
        builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IPepperKeyProvider>().GetKey("key");
        }
        app.UseMiddleware<ErrorHandler>();
        // Configure the HTTP request pipeline.
         
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.MapScalarApiReference();

            app.UseWebAssemblyDebugging();
        }

        app.UseHttpsRedirection();

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseAuthorization();

        app.MapControllers();
        app.MapFallbackToFile("index.html");

        app.Run();
    }
}

//apimappingprofile api altında automapper

