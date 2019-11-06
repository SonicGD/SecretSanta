using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace SecretSantaMailer
{
    public class MailSender<T> where T : IParticipantWithMail
    {
        private readonly SMTPMailConfig _smtpConfig;

        private readonly ILogger<MailSender<T>> _logger;
        private readonly ViewRenderService _renderer;

        public MailSender(SMTPMailConfig smtpConfig, ILogger<MailSender<T>> logger, ViewRenderService renderer)
        {
            _smtpConfig = smtpConfig;
            _logger = logger;
            _renderer = renderer;
        }

        public async Task SendMail(IDictionary<T, T> model, string subject, string template)
        {
            _logger.LogInformation("Start sending mail");
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect(_smtpConfig.Host, _smtpConfig.Port, !_smtpConfig.UseTLS);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_smtpConfig.UserName, _smtpConfig.Password);
                foreach (var participantsPair in model)
                {
                    var html = await _renderer.RenderToStringAsync(template, participantsPair);

                    var message = new MimeMessage {Subject = subject};
                    message.From.Add(MailboxAddress.Parse(_smtpConfig.FromEmail));

                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(html);

                    var builder = new BodyBuilder
                    {
                        TextBody = htmlDoc.DocumentNode.SelectSingleNode("//body").InnerText,
                        HtmlBody = html
                    };

                    message.Body = builder.ToMessageBody();

                    try
                    {
                        message.To.Clear();
                        message.To.Add(new MailboxAddress(participantsPair.Key.Email));
                        _logger.LogInformation("Send mail to {email}", participantsPair.Key.Email);
                        await client.SendAsync(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }

                client.Disconnect(true);
            }
        }
    }

    public interface IParticipantWithMail
    {
        string Email { get; set; }
    }

    public class SMTPMailConfig
    {
        public string FromEmail { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool UseTLS { get; set; }
    }
}
