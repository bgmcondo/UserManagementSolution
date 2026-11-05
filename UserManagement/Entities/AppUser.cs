using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.Domain.ValueObjects;

namespace UserManagement.Domain.Entities
{
  public  class AppUser:IdentityUser<Guid>
    {
   
        public string FullName { get; set; } = string.Empty;
        public string ? Website { get; set; }

        public Address? Address { get; set; }

        public Company? Company { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
