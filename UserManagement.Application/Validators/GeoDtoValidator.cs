using FluentValidation;
using UserManagement.Application.DTOs;

namespace UserManagement.Application.Validators
{
    // Kordinat doğrulaması.
    // Enlem(Latitude) -90 ile 90 arasında olmalı. 
    // Boylam(Longitude) -180 ile 180 arasında olmalı.
    public  class GeoDtoValidator:AbstractValidator<GeoDto>   
    {

        public GeoDtoValidator()
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .WithMessage("Enlem ve Boylam değerleri belirtilen -90 ve 90 aralığında olmalıdır.");
            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage("Enlem ve Boylam değerleri belirtilen -180 ve 180 aralığında olmalıdır.");
        }
    }
}
