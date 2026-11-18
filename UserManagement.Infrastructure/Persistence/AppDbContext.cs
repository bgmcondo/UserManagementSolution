using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;
using System;
using System.Collections.Generic;

using System.Text;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence
{
    public class AppDbContext:IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        

          protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity tablolarının yapılandırması için ŞART; silinirse Identity tabloları bozulur.
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AppUser>(e=>
            {
                e.Property(u => u.FullName).IsRequired().HasMaxLength(100);

                e.Property(u => u.Email).IsRequired().HasMaxLength(256);
                e.Property(u => u.UserName).IsRequired();
                e.Property(u=>u.Website).HasMaxLength(200);  
                e.Property(u=> u.PhoneNumber).HasMaxLength(30);
              //kendi Id’si olmayan ve tek başına var olamayan, bir sahibe ait nesne.

                e.OwnsOne(u => u.Address, a => {
                    // Ayrı tablo: Address'in içindeki Geo yüzünden, aynı tabloda tüm adres alanları NULL olunca
                    //(Geo alanı(Navigation property) dolu olsa bile adresi null sayıyor,Kodda  kordinatlara sadece adresten ulaşabildiğimiz 
                    //için yeni bir tablo oluşturup usersadress diye kaydediyoruz.
                    // EF adresi null sayıp koordinatı okuyamıyordu. Burada adresin varlığı satırdan anlaşılır.

                    a.ToTable("UserAddresses");
                    a.Property(x => x.Street).HasMaxLength(150);
                    a.Property(x => x.Suite).HasMaxLength(100);
                    a.Property(x => x.City).HasMaxLength(100);
                    a.Property(x=>x.ZipCode).HasMaxLength(20);
                    a.OwnsOne(x => x.Geo);

                    e.OwnsOne(u => u.Company, c =>
                    {
                        c.ToTable("UserCompanies");
                        c.Property(x=>x.Name).HasMaxLength(100);
                        c.Property(x=>x.CatchPhrase).HasMaxLength(250);
                        c.Property(x => x.Bs).HasMaxLength(250);

                    });
                });

            }
              
                     );

        }

    }
}
