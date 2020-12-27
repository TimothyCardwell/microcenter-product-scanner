using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebScrapingService.Options;
using WebScrapingService.Twilio;

namespace WebScrapingService
{
    public class MicroCenterStockChecker : IHostedService, IDisposable
    {
        private Timer _timer;
        private HashSet<Product> _inStockProducts;
        private readonly ITwilioService _twilioService;
        private readonly ILogger<MicroCenterStockChecker> _logger;
        private readonly MicroCenterOptions _microCenterConfiguration;

        public MicroCenterStockChecker(
            ITwilioService twilioService,
            ILogger<MicroCenterStockChecker> logger,
            IOptions<MicroCenterOptions> microCenterConfiguration)
        {
            _twilioService = twilioService;
            _logger = logger;
            _microCenterConfiguration = microCenterConfiguration.Value;

            _inStockProducts = new HashSet<Product>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CheckStock, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            return Task.FromResult(true);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private void CheckStock(object state)
        {
            _logger.LogInformation("Checking stock...");

            try
            {
                var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
                var stockUrl = $"{_microCenterConfiguration.BaseUrl}/{_microCenterConfiguration.Rtx3000SeriesSearchPath}&storeid={_microCenterConfiguration.StoreId}";
                var document = context.OpenAsync(stockUrl).GetAwaiter().GetResult();

                var myStoreOnlyClass = document.GetElementsByClassName("my-store-only").Single();
                var myStoreOnlyListItems = myStoreOnlyClass.GetElementsByTagName("li");
                var inStock = myStoreOnlyListItems[0];
                var totalFound = myStoreOnlyListItems[1];

                var inStockCount = Convert.ToInt32(inStock.TextContent.Split(" ")[0]);
                if (inStockCount > 0)
                {
                    var currentStock = new HashSet<Product>();
                    var products = document.GetElementsByClassName("product_wrapper");
                    foreach (var product in products)
                    {
                        var detailWrapper = product.GetElementsByClassName("detail_wrapper").Single();
                        var productInfo = detailWrapper.GetElementsByTagName("a").Single();

                        var graphicsCard = new Product(
                            productInfo.GetAttribute("data-id"),
                            productInfo.GetAttribute("data-name"),
                            productInfo.GetAttribute("data-price"),
                            _microCenterConfiguration.BaseUrl + productInfo.GetAttribute("href"));

                        currentStock.Add(graphicsCard);
                    }

                    var newStock = currentStock.Except(_inStockProducts);

                    // Notify
                    if (newStock.Any())
                    {
                        _twilioService.SendNotification(newStock.ToHashSet(), stockUrl).GetAwaiter().GetResult();
                    }

                    _inStockProducts = currentStock;
                }
                else
                {
                    _logger.LogInformation("No available product");
                    _inStockProducts.Clear();
                }

                _logger.LogInformation("Stock checking complete");
            }
            catch (Exception e)
            {
                // Just swallow the error so that program flow continues
                _logger.LogError(e, "Failed to check stock status");
            }
        }
    }
}