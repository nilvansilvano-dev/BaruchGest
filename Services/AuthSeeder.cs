using Microsoft.EntityFrameworkCore;
using FinanceiroAPI.Data;
using FinanceiroAPI.Models;

namespace FinanceiroAPI.Services;

public class AuthSeeder
{
    private readonly FinanceiroDbContext _db;

    public AuthSeeder(FinanceiroDbContext db) => _db = db;

    public async Task SeedAsync()
    {
        if (await _db.Usuarios.AnyAsync()) return;

        _db.Usuarios.AddRange(
            new Usuario
            {
                Nome      = "Usuário Teste",
                Email     = "usuario@teste.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Teste@123"),
                Perfil    = "usuario"
            },
            new Usuario
            {
                Nome      = "Contador Teste",
                Email     = "contador@teste.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Teste@123"),
                Perfil    = "contador"
            }
        );

        await _db.SaveChangesAsync();
    }
}
