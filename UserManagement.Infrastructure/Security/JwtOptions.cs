using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Infrastructure.Security
{
    //jwt ayarı tutan sınıf
    //"Sealed" bir başka sınıfın onu miras alamayacağını belirten anahtar kelime
    //değerler appsettingjson daki jwt bölümünden otomatik doldurulur
public sealed class JwtOptions
    {
        //appsettingjsondaki bölümün adı
        //const sayesinde "Jwt" metnini her yerde elle yazıp yanlış yazma riskine girmiyoruz

        public const string SectionName = "jwt";

        //tokenın kimin ürettiğini belirtir(token içindeki issuer claimi
        //doğrulama sırasında bu değer kontrol edilir;eşleşmezse token reddedilir

        public string Issuer { get; init; } = string.Empty;

        //tokenı kimin için üretildiği belirtir
        //örn:frontend uygulaması veya token kullanacak bir başka istemci
        public string Audience { get; init; }=string.Empty;

        //tokenı imzalamak ve doğrulamak için kullanılan gizli anahtar
        // HMAC-SHA256 için en az 32 karakter olmalı.
        //bu değer repoya konmaz

        public string SecretKey {  get; init; }=string.Empty;

        //Access token'ın kaç dakika geçerli olacağı 
        //süre dolunca token geçersiz oluyor
        //ayarlarda belirtilmezse varsayılan 60 dk
        public int AccessTokenExpirationMinutes { get; init; } = 60;

        //refresh token kaç gün geçerli olacağı
        //refresh token,access token süresi dolduğunda kullanıcıya tekrar giriş yaptırmadan yeni access token için kullanılır
        //ayarlarda belirtilmezse varsayılan 7 gün
        public int RefreshTokenExpirationDays { get; init; } = 7;

    }
}
