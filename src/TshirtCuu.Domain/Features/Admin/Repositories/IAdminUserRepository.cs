using TshirtCuu.Domain.Features.Admin.Models;

namespace TshirtCuu.Domain.Features.Admin.Repositories;

public interface IAdminUserRepository
{
    AdminUser? FindByUsername(string username);
}
