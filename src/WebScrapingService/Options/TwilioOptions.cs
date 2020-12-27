namespace WebScrapingService.Options
{
    public class TwilioOptions
    {
        public const string SectionKey = "Twilio";

        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string FromPhoneNumber { get; set; }
        public string ToPhoneNumber { get; set; }
    }
}