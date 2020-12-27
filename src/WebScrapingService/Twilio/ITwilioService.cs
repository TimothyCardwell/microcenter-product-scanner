using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebScrapingService.Twilio
{
    public interface ITwilioService
    {
        Task SendNotification(HashSet<Product> products, string url);
    }
}