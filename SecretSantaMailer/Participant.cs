namespace SecretSantaMailer
{
    public class Participant : IParticipantWithMail
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }
}