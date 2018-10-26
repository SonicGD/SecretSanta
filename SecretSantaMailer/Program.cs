using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SecretSanta;

namespace SecretSantaMailer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("You must provide path to participants file as first argument");
            }

            var participantsPath = args[0];

            if (!File.Exists(participantsPath))
            {
                throw new ArgumentException($"File {participantsPath} doesn't exists");
            }

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("./config.json")
                .Build();

            var json = File.ReadAllText(participantsPath);
            var participants = JsonConvert.DeserializeObject<List<Participant>>(json);
            var santasList = SecretSantaGenerator.Generate(participants, new Dictionary<Participant, Participant>());

            var smtpConfig = new SMTPMailConfig
            {
                Host = configuration["SANTA_SMTP_HOST"],
                Port = int.Parse(configuration["SANTA_SMTP_PORT"]),
                FromEmail = configuration["SANTA_SMTP_FROM_EMAIL"],
                UserName = configuration["SANTA_SMTP_USER_NAME"],
                Password = configuration["SANTA_SMTP_PASSWORD"],
                UseTLS = bool.Parse(configuration["SANTA_SMTP_USE_TLS"])
            };

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole();
            var logger = loggerFactory.CreateLogger<MailSender<Participant>>();
            var mailer = new MailSender<Participant>(smtpConfig, logger);
            await mailer.SendMail(santasList, "Secret Santa 2018", "~/Views/Mail.cshtml");
        }
    }
}