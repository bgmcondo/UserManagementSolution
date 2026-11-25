using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Application.Common
{
     public enum PasswordCheckResult
    {
        Success,     // Parola doğru, sayaç sıfırlandı
        Failed,      // Parola yanlış, sayaç 1 arttı
        LockedOut    // Hesap kilitli (parola doğru olsa bile giriş yok)

    }
}
