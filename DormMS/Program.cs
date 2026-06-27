using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.Services.Implementations;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Repositories.Implementations;
using DormMS.Server.Models;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using DormMS.Server.Services;
using DormMS.Server.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    throw new InvalidOperationException(
        "JWT key is missing. Configure JwtSettings:Key in environment variables (JwtSettings__Key).");
}

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.MapInboundClaims = false;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };
});

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
// .AddJwtBearer(options =>
// {
//     options.Authority = builder.Configuration["Auth0:Domain"];
//     options.Audience = builder.Configuration["Auth0:Audience"];
// });

builder.Services.AddAuthorization(options =>
{
    // options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AdminOrDormManager", policy =>
     policy.RequireRole("Admin", "DormManager"));
});
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IDormRepository, DormRepository>();
builder.Services.AddScoped<IDormService, DormService>();
builder.Services.AddScoped<IDormitoryRoomRepository, DormitoryRoomRepository>();
builder.Services.AddScoped<IDormitoryRoomService, DormitoryRoomService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICompetitionRepository, CompetitionRepository>();
builder.Services.AddScoped<ICompetitionService, CompetitionService>();
builder.Services.AddScoped<ICompetitionApplicationRepository, CompetitionApplicationRepository>();
builder.Services.AddScoped<ICompetitionApplicationService, CompetitionApplicationService>();
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IDocumentGenerationService, DocumentGenerationService>();
builder.Services.AddScoped<IRoomSwapRequestRepository, RoomSwapRequestRepository>();
builder.Services.AddScoped<IRoomSwapRequestService, RoomSwapRequestService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDormManagerRepository, DormManagerRepository>();

var app = builder.Build();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

QuestPDF.Drawing.FontManager.RegisterFont(
    File.OpenRead(Path.Combine(builder.Environment.WebRootPath, "fonts", "DejaVuSans.ttf")));
QuestPDF.Drawing.FontManager.RegisterFont(
    File.OpenRead(Path.Combine(builder.Environment.WebRootPath, "fonts", "DejaVuSans-Bold.ttf")));

await app.Services.SeedDefaultUsersAsync(builder.Configuration);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowReact");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
