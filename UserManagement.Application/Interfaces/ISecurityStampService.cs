using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Application.Interfaces
{
        public interface ISecurityStampService
    {
        // Kullanıcının güncel stamp'i: önce Redis, yoksa DB (sonra Redis'e yazılır).
        // Kullanıcı silinmişse null.
        Task<string?> GetCurrentAsync(Guid userId, CancellationToken ct);
    }
}
