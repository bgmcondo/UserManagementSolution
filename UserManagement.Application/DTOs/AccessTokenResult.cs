using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Application.DTOs
{
    // Token servisinin ürettiği access token ve bitiş zamanı
    public record AccessTokenResult(string Token, DateTime ExpiresAt);
}
