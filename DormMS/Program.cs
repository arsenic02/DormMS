using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

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
        //ClockSkew = TimeSpan.Zero
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
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IDormRepository, DormRepository>();
builder.Services.AddScoped<IDormService, DormService>();
builder.Services.AddScoped<IDormitoryRoomRepository, DormitoryRoomRepository>();
builder.Services.AddScoped<IDormitoryRoomService, DormitoryRoomService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

await app.Services.SeedDefaultUsersAsync(builder.Configuration);

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
