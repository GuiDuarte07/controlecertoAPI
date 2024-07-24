namespace Finantech.DTOs.Events
{
    public class EmailEvent
    {
        public List<string> Emails { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public EmailEvent(List<string> emails, string subject, string body)
        {
            Emails = emails;
            Subject = subject;
            Body = body;
        }
    }
}
