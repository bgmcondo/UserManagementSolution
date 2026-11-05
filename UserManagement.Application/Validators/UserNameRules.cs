using FluentValidation;

namespace UserManagement.Application.Validators
{
    // Kullanıcı adı kuralları.
    //Hem CreateUserRequestDtoValidator hem de ChangeUserNameRequestDtoValidator için geçerli.
    //böylece kurallar tek bir yerde toplanmış olur.

    //Neden static?
    //ValidUserName bir extension method .Extension methodlar static class içinde tanımlanır. Bu yüzden UserNameRules static olmalı.
    //Ayrıca UserNameRules içinde herhangi bir state tutulmuyor, sadece kurallar tanımlanıyor. Bu yüzden static class uygun.
    //"@" izin verilen kurallar arasında değil. Çünkü login ekranında "@" içeren değer e-posta olarak yorumlanıyor. 


    //Kullanıcı adı kuralları:
    //RuleFor(x => x.UserName).ValidUserName();
    public static class UserNameRules
    {
        public static IRuleBuilderOptions<T, string> ValidUserName<T>(this IRuleBuilder<T, string> ruleBuilder)
        //T: CreateUserRequestDto veya ChangeUserNameRequestDto olabilir. Bu yüzden generic T kullanıyoruz.
        //String ise UserName alanının tipidir. Bu yüzden IRuleBuilder<T, string> kullanıyoruz.
        //IRuleBuilderOptions<T, string> döndürür. Bu sayede zincirleme kurallar eklenebilir.

        {
            return ruleBuilder
                .NotEmpty().WithMessage("Kullanıcı adı boş olamaz.")
                .Length(3,50).WithMessage("Kullanıcı adı 3 ile 50 karakter arasında olmalıdır.")
                .Matches(@"^[a-zA-Z0-9_.-]+$").WithMessage("Kullanıcı adı yalnızca harf, rakam, alt çizgi (_), nokta (.) ve tire (-) içerebilir.");
        }

    }
}
