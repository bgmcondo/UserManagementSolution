using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Infrastructure.Seeding.Models
{
    // JSON'daki "company" nesnesi
    public sealed class JsonPlaceholderCompany
    {
        public string Name { get; init; } = string.Empty;
        public string CatchPhrase { get; init; } = string.Empty;
        public string Bs { get; init; } = string.Empty;
    }
}
