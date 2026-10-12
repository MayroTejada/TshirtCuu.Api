using TshirtCuu.Domain.Features.Admin.Models;
using TshirtCuu.Domain.Features.Admin.Repositories;
using TshirtCuu.Services.Persistence;

namespace TshirtCuu.Services.Features.Admin;

public sealed class InMemoryAdminUserRepository(InMemoryStore store) : IAdminUserRepository
{
    public AdminUser? FindByUsername(string username) =>
        store.Admins.FirstOrDefault(a =>
            string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase));
}
