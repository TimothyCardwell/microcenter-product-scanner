using System;

namespace WebScrapingService
{
    public class Product
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Price;
        public readonly string Link;

        public Product(string id, string name, string price, string link)
        {
            Id = id;
            Name = name;
            Price = price;
            Link = link;
        }

        public override bool Equals(object obj)
        {
            return obj is Product product && Id == product.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}