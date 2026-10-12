using Microsoft.EntityFrameworkCore;
using TshirtCuu.Domain.Features.Admin.Models;
using TshirtCuu.Domain.Features.Admin.Repositories;
using TshirtCuu.Services.Persistence;

namespace TshirtCuu.Services.Features.Admin;

public sealed class SqlAdminUserRepository(AppDbContext db) : IAdminUserRepository
{
    public AdminUser? FindByUsername(string username) =>
        db.AdminUsers.AsNoTracking()
            .FirstOrDefault(a => a.Username == username);
}
