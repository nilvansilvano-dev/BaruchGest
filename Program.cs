using Microsoft.EntityFrameworkCore;
using FinanceiroAPI.Data;
using FinanceiroAPI.Repositories;
using FinanceiroAPI.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("Connection string 'SqlServer' não encontrada.");

builder.Services.AddDbContext<FinanceiroDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IFinanceiroRepository, FinanceiroRepository>();
builder.Services.AddScoped<ResumoService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title   = "Controle Financeiro API",
        Version = "v1",
        Description = "API para controle de receitas e despesas pessoais/empresariais."
    });
});

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
