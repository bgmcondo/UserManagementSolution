using Azure.Core;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.Application.DTOs;
using UserManagement.Application.Validators;
using Xunit;

namespace UserManagement.UnitTests.Application.Validators
{
    public  class ChangeUserNameRequestDtoValidatorTests
    {
        private readonly ChangeUserNameRequestDtoValidator _validator=new();

        // Geçerli kullanıcı adları: harf, rakam, _ . - kullanılabilir
        [Theory]
        [InlineData("ayse")]
        [InlineData("ayse_yilmaz")]
        [InlineData("ayse.yilmaz-01")]
        [InlineData("abc")]               // tam sınırda: 3 karakter


        public void Validate_ValidUserName_IsValid(string newUserName)
        {
            var request = new ChangeUserNameRequestDto(newUserName,"Test123!");
            var result= _validator.Validate(request);
            result.IsValid.Should().BeTrue();

        }

        // Geçersiz kullanıcı adları
        [Theory]
        [InlineData("")]                  // boş
        [InlineData("ab")]                // çok kısa (en az 3)
        [InlineData("ayse@test")]         // "@" yasak (login'de e-posta sanılır)
        [InlineData("ayse yilmaz")]       // boşluk yasak
        [InlineData("ayşe")]              // Türkçe karakter yasak (sadece a-z)
        public void Validate_InvalidUserName_IsInvalid(string newUserName)
        {
            //Arrange
            var request = new ChangeUserNameRequestDto(newUserName, "Test123!");
            var result = _validator.Validate(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().OnlyContain(e => e.PropertyName == newUserName);


        }

        //50 karakterden fazla  kullanıcı adı  geçersiz 
        //İnline Data'ya new string yazılamadığı için ayrı fact
        [Fact]

        public void Validate_UserNameTooLong_IsInvalid()
        {
            //Arrange
            var request = new ChangeUserNameRequestDto(String new);

        }
    }
}
