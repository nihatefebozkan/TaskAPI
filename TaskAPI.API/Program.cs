using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore; //Entity Framework Core kütüphanesini içe aktarır.
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;
using TaskAPI.Core.Middleware;
using TaskAPI.Entities.Interfaces;
using TaskAPI.Infrastructure.Data; //veritabanı bağlantısı için gerekli olan DbContext'i içe aktarır.
using TaskAPI.Infrastructure.Repositories; //veritabanı işlemleri için gerekli olan repository sınıfını içe aktarır.
using TaskAPI.Service.Services;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT token"
                };

                return Task.CompletedTask;
            });
        });
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // eklendi: veritabanı bağlantısı için gerekli olan DbContext'i ekler ve SQL Server kullanır. Connection string, appsettings.json dosyasından alınır.
        builder.Services.AddScoped<ITaskRepository, TaskRepository>();
        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IAuthService, LoginService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey
                    (
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
                options.Events = new JwtBearerEvents 
                {
                    OnMessageReceived = context =>
                    {
                        var cookieToken = context.Request.Cookies["token"];

                        if (!string.IsNullOrEmpty(cookieToken))
                            context.Token = cookieToken;

                        return Task.CompletedTask;
                    }
                };
            });
        var app = builder.Build();
        app.UseMiddleware<ErrorHandler>();
        // Configure the HTTP request pipeline.

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}