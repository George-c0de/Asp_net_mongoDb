﻿using System.Net;
using System.Net.Mail;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class EmailMessageSender
    {
        //private string email;
        //private string id;
        //private Result result;
        //private string name;

        //public EmailMessageSender(string email,string id,Result result,string name)
        //{
        //    this.email = email;
        //    this.id = id;
        //    this.result = result;
        //    this.name = name;
        //}

        public void Send(string email, string id, Result result, string name)
        {
            MailAddress from = new MailAddress("Test_Company@gmail.com", "Test_Company");
            //komy
            MailAddress to = new MailAddress(email);
            MailMessage m = new MailMessage(from, to);
            // тема письма
            m.Subject = "Тест";
            // текст письма
            string body;
            body = "<h5>" + name + "</h5>";
            body += "<p>" + "Количество правильных ответов по категориям" + "</p>";
            foreach (var el in result.Percentage_category)
            {
                body += "<p>" + "Категория: " + el.Key + " : " + el.Value + "</p>";
            }
            body += "<p>" + "Доля правильных ответов" + result.Percent + "</p>";
            body += "<p>" + "Ваши рекомендации" + "</p>";
            body += "<p>" + result.recommendations + "</p>";
            m.Body = body;
            // письмо представляет код html
            m.IsBodyHtml = true;
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            // логин и пароль
            smtp.Credentials = new NetworkCredential("Test_Company@gmail.com", "mypassword");
            smtp.EnableSsl = true;
            smtp.Send(m);
        }
    }
}
