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
    public class Sender
    {
        private readonly IConfiguration _configuration;
        private readonly MailSender<Participant> _mailSender;
        private readonly ILogger<Sender> _logger;

        public Sender(IConfiguration configuration, MailSender<Participant> mailSender,
            ILogger<Sender> logger)
        {
            _configuration = configuration;
            _mailSender = mailSender;
            _logger = logger;
        }

        public async Task SendAsync()
        {
            if (string.IsNullOrEmpty(_configuration["SANTA_PARTICIPANT_PATH"]))
            {
                throw new ArgumentException("You must provide path to participants file as first argument");
            }

            var participantsPath = _configuration["SANTA_PARTICIPANT_PATH"];

            if (!File.Exists(participantsPath))
            {
                throw new ArgumentException($"File {participantsPath} doesn't exists");
            }

            _logger.LogInformation("Load participants list");
            var json = File.ReadAllText(participantsPath);
            var participants = JsonConvert.DeserializeObject<List<Participant>>(json);
            _logger.LogInformation("Participants: {count}", participants.Count);
            var santasList =
                SecretSantaGenerator.Generate(participants, new Dictionary<Participant, Participant>());
            _logger.LogInformation("List generated");
            try
            {
                await _mailSender.SendMail(santasList, "Secret Santa 2019", "~/Views/Mail/Notification.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, ex.ToString());
            }
        }
    }
}
