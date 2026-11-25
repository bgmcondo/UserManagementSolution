using FluentAssertions;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.Application.DTOs;
using UserManagement.Application.Validators;
using Xunit;

namespace UserManagement.UnitTests.Application.Validators
{
    public class GeoDtoValidatorTests
    {

        // Test edilen gerçek validator. Bağımlılığı yok → mock da yok!
        private readonly GeoDtoValidator _validator = new();
        //Test edilen gerçek validator.Bağımlılık yok mock da yok çünkü veritabanı redis vb bağımlılıklara gerek yok

        //İstanbulun koordinatları geçerli  olmalı
        [Fact]
        public void Validate_ValidCoords_IsValid()
        {

            var geo = new GeoDto(41, 29);
            // Act
            var result = _validator.Validate(geo);

            // Assert
            result.IsValid.Should().BeTrue();

        
        }


        //Enlem 90'dan büyük olamaz
        [Fact]

        public void Validate_LatToHigh_IsInvalid()
        {
            var geo = new GeoDto(100, 29); 

            //Act
            var result = _validator.Validate(geo);

            result.IsValid.Should().BeFalse();

        }

        //Boylam 180'den büyük olamaz

        [Fact]
        public void Validate_LongToHigh_IsInvalid()
        {

          var geo=new GeoDto(41, 200);
            // Act
          var result = _validator.Validate(geo);

           // Assert
           result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Longitude");



        }

    }
}
