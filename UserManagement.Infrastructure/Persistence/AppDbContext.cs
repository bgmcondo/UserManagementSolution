using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;
using System;
using System.Collections.Generic;

using System.Text;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<AppUser> Users;

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>(e=>
            {
                e.Property(u => u.FullName).IsRequired().HasMaxLength(100);

                e.Property(u => u.Email).IsRequired().HasMaxLength(256);
                e.Property(u => u.UserName).IsRequired();
                e.Property(u=>u.Website).HasMaxLength(200);  
                e.Property(u=> u.PhoneNumber).HasMaxLength(30);
              //kendi Id’si olmayan ve tek başına var olamayan, bir sahibe ait nesne.
                e.OwnsOne(u => u.Address, a => {
                    a.Property(x => x.Street).HasMaxLength(150);
                    a.Property(x => x.Suite).HasMaxLength(100);
                    a.Property(x => x.City).HasMaxLength(100);
                    a.Property(x=>x.ZipCode).HasMaxLength(20);
                    a.OwnsOne(x => x.Geo);

                    e.OwnsOne(u => u.Company, c =>
                    {
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
