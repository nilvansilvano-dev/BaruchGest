using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FinanceiroAPI.Data;
using FinanceiroAPI.Repositories;
using FinanceiroAPI.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("Connection string 'SqlServer' não encontrada.");

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key não configurada.");

builder.Services.AddDbContext<FinanceiroDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IFinanceiroRepository, FinanceiroRepository>();
builder.Services.AddScoped<ResumoService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthSeeder>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "Controle Financeiro API",
        Version     = "v1",
        Description = "API para controle de receitas e despesas pessoais/empresariais."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In          = ParameterLocation.Header,
        Description = "Token JWT. Use: Bearer {token}",
        Name        = "Authorization",
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(opt =>
    opt.AddPolicy("FrontendPolicy", p =>
        p.WithOrigins("http://localhost:3000", "http://localhost:5173")
         .AllowAnyMethod()
         .AllowAnyHeader()));

var app = builder.Build();

// Seed usuarios de teste ao iniciar
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AuthSeeder>().SeedAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
