using Microsoft.EntityFrameworkCore;

using Notism.Domain.User;

namespace Notism.Infrastructure.Common;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}