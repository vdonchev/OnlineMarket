namespace OnlineMarket
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Wintellect.PowerCollections;

    public class Product : IComparable<Product>
    {
        private const string ToStringTemplate = "{0}({1})";

        public Product(string name, double price, string type)
        {
            this.Name = name;
            this.Price = price;
            this.Type = type;
        }

        public string Name { get; private set; }

        public double Price { get; private set; }

        public string Type { get; private set; }

        public int CompareTo(Product other)
        {
            var compareResult = this.Price.CompareTo(other.Price);
            if (compareResult == 0)
            {
                compareResult = this.Name.CompareTo(other.Name);
                if (compareResult == 0)
                {
                    compareResult = this.Type.CompareTo(other.Type);
                }
            }

            return compareResult;
        }

        private bool Equals(Product other)
        {
            return string.Equals(this.Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Product) obj);
        }

        public override int GetHashCode()
        {
            return (this.Name != null ? this.Name.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format(ToStringTemplate, this.Name, this.Price);
        }
    }

    public enum FilterType
    {
        From,
        To
    }

    public class ProductStore
    {
        private const int MaxCount = 10;
        private const string PrintTemplate = "Ok: {0}";
        private const string ProductAdded = "Ok: Product {0} added successfully";
        private const string ProductDoesNotExist = "Error: Type {0} does not exists";
        private const string ProductAlreadyExist = "Error: Product {0} already exists";

        private readonly HashSet<Product> allProducts; 
        private readonly Dictionary<string, SortedSet<Product>> productsByType;
        private readonly OrderedDictionary<double, SortedSet<Product>> productsByPrice;

        public ProductStore()
        {
            this.allProducts = new HashSet<Product>();
            this.productsByType = new Dictionary<string, SortedSet<Product>>();
            this.productsByPrice = new OrderedDictionary<double, SortedSet<Product>>();
        }

        public void Add(string name, double price, string type)
        {
            var newProduct = new Product(name, price, type);
            if (this.allProducts.Contains(newProduct))
            {
                Console.WriteLine(ProductAlreadyExist, name);
                return;
            }

            this.allProducts.Add(newProduct);

            // Add to products by type
            if (!this.productsByType.ContainsKey(type))
            {
                this.productsByType.Add(type, new SortedSet<Product>());
            }

            this.productsByType[type].Add(newProduct);

            // Add to products by price
            if (!this.productsByPrice.ContainsKey(price))
            {
                this.productsByPrice.Add(price, new SortedSet<Product>());
            }

            this.productsByPrice[price].Add(newProduct);

            Console.WriteLine(ProductAdded, name);
        }

        public void Filter(string type)
        {
            if (!this.productsByType.ContainsKey(type))
            {
                Console.WriteLine(ProductDoesNotExist, type);
                return;
            }

            var res = this.productsByType[type].Take(10);
            Console.WriteLine(PrintTemplate, string.Join(", ", res));
        }

        public void Filter(double min, double max)
        {
            var res = new List<Product>();
            var allInRange = this.productsByPrice.Range(min, true, max, true);
            var shouldBreak = false;
            foreach (var inRangeSet in allInRange)
            {
                foreach (var product in inRangeSet.Value)
                {
                    if (res.Count >= MaxCount)
                    {
                        shouldBreak = true;
                        break;
                    }
                    res.Add(product);
                }

                if (shouldBreak)
                {
                    break;
                }
            }

            Console.WriteLine(PrintTemplate, string.Join(", ", res));
        }

        public void Filter(double value, FilterType type)
        {
            if (type == FilterType.From)
            {
                this.FilterFrom(value);
                return;
            }

            this.FilterTo(value);
        }

        private void FilterFrom(double value)
        {
            var res = new List<Product>();
            var allInRange = this.productsByPrice.RangeFrom(value, true);
            var shouldBreak = false;
            foreach (var inRangeSet in allInRange)
            {
                foreach (var product in inRangeSet.Value)
                {
                    if (res.Count >= MaxCount)
                    {
                        shouldBreak = true;
                        break;
                    }
                    res.Add(product);
                }

                if (shouldBreak)
                {
                    break;
                }
            }

            Console.WriteLine(PrintTemplate, string.Join(", ", res));
        }

        private void FilterTo(double value)
        {
            var res = new List<Product>();
            var allInRange = this.productsByPrice.RangeTo(value, true);
            var shouldBreak = false;
            foreach (var inRangeSet in allInRange)
            {
                foreach (var product in inRangeSet.Value)
                {
                    if (res.Count >= MaxCount)
                    {
                        shouldBreak = true;
                        break;
                    }
                    res.Add(product);
                }

                if (shouldBreak)
                {
                    break;
                }
            }

            Console.WriteLine(PrintTemplate, string.Join(", ", res));
        }
    }

    public static class OnlineMarket
    {
        public static void Main()
        {
            var store = new ProductStore();

            var command = Console.ReadLine().Split().ToArray();
            while (command[0] != "end")
            {
                if (command[0] == "add")
                {
                    store.Add(command[1], double.Parse(command[2]), command[3]);
                }
                else
                {
                    var argsCount = command.Length;
                    if (argsCount <= 4)
                    {
                        store.Filter(command[3]);
                    }
                    else if (argsCount >= 7)
                    {
                        store.Filter(double.Parse(command[4]), double.Parse(command[6]));
                    }
                    else
                    {
                        if (command[3] == "from")
                        {
                            store.Filter(double.Parse(command[4]), FilterType.From);
                        }
                        else
                        {
                            store.Filter(double.Parse(command[4]), FilterType.To);
                        }
                    }
                }

                command = Console.ReadLine().Split().ToArray();
            }
        }
    }
}
