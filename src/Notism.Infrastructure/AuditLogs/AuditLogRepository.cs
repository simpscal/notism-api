using Notism.Domain.AuditLog;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.AuditLogs;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }
}