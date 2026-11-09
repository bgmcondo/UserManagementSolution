using MassTransit;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UserManagement.Application.Events;
using UserManagement.Application.Interfaces;

namespace UserManagement.Infrastructure.Messaging
{
    //rabbitmq'dan gelen UserCreatedEvent mesajlarını dinler ve hoşgeldin epostası gönderir
    //MassTransit kuyruğa mesaj geldiğinde consume metodunu kendisi çağırır 

    public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly IEmailSender _emailSender;

        public UserCreatedConsumer(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }
        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            //kuyruktan gelen asıl event 
            var evt = context.Message;

            //kullanıcıdan gelen metni html e koymadan önce kodla
            // Örn: "<script>" → "&lt;script&gt;" (zararsız metin olarak görünür)
            var safeName = WebUtility.HtmlEncode(evt.FullName);
            var subject = "UserManagement'a hoşgeldin!";
            var body = $"""
                <h1>Merhaba {safeName}!</h1> 
                <p>Hesabın başarıyla oluşturuldu</p>
                """;
            //try catch yok hata olursa masstransit görür,tekrar dener.
            //olmazsa mesajı _error kuyruğuna taşır.Mesaj kaybolmaz

            await _emailSender.SendAsync(evt.Email,subject,body,context.CancellationToken);
        }
    }
}
