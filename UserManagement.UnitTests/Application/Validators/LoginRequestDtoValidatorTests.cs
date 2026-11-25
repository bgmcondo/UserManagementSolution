using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UserManagement.Application.DTOs;
using UserManagement.Application.Validators;
using Xunit;

namespace UserManagement.UnitTests.Application.Validators
{
    public class LoginRequestDtoValidatorTests
    {
        private readonly LoginRequestDtoValidator _validator=new();

        // Kullanıcı adı ve şifre dolu → geçerli
        [Fact]
        public void Validate_ValidRequest_IsValid()
        {
            // Arrange
            var request = new LoginRequestDto("ayse", "Test123!");

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        //Kullanıcı adı ve eposta boş olamaz
        [Fact]
        public void Validate_EmptyUserNameOrEmail()
        {
            var request = new LoginRequestDto("","Test123!");

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();

        
        }

        //Şifre boş olamaz
        [Fact]
        public void Validate_EmptyPassword_IsInvalid()
        {
            var request = new LoginRequestDto("Ayse", " ");
            var result = _validator.Validate(request);
            result.IsValid.Should().BeFalse();
            // Hata listesinde tek bir hata olmalı ve o hata Password alanına ait olmalı (hata doğru sebepten mi çıktı?)
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Password");
        }

        // Kullanıcı adı/e-posta 256 karakterden uzun olamaz
        [Fact]
        public void Validate_UserNameOrEmailTooLong_IsInvalid()
        {
            // Arrange
            // new string('a', 257) → 257 tane 'a' harfinden oluşan bir yazı üretir
            var tooLong = new string('a', 257);
            var request = new LoginRequestDto(tooLong, "Test123!");

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "UserNameOrEmail");
        }

    }
}
