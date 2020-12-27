using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using WebScrapingService.Options;

namespace WebScrapingService.Twilio
{
    public class TwilioService : ITwilioService
    {
        private readonly TwilioOptions _twilioCenterConfiguration;
        private readonly ILogger<TwilioService> _logger;

        public TwilioService(
            IOptions<TwilioOptions> twilioConfiguration,
            ILogger<TwilioService> logger)
        {
            _logger = logger;
            _twilioCenterConfiguration = twilioConfiguration.Value;
            TwilioClient.Init(_twilioCenterConfiguration.AccountSid, _twilioCenterConfiguration.AuthToken);
        }

        public async Task SendNotification(HashSet<Product> products, string url)
        {
            try
            {
                var result = await MessageResource.CreateAsync(
                    body: $"Found {products.Count} graphic cards available at {url}",
                    from: _twilioCenterConfiguration.FromPhoneNumber,
                    to: _twilioCenterConfiguration.ToPhoneNumber
                );

                _logger.LogInformation("Successfuly sent SMS message: {@Result}", result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send SMS message");
            }
        }
    }
}