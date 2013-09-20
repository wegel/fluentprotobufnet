using System.Collections.Generic;

namespace FluentProtobufNet.Tests
{
    public class Category
    {
        public string Name { get; set; }
        public IList<Category> SubCategories { get; set; }
        public IList<Item> Items { get; set; }
        public Category ParentCategory { get; set; }
    }

    public class Item
    {
        public string SKU { get; set; }
        public string Definition { get; set; }
        public Category MainCategory { get; set; }
    }

    public class CategoryWithDescription : Category
    {
        public string Description { get; set; }
    }

    public class CategoryThirdLevel : CategoryWithDescription
    {
        public string ThirdLevel { get; set; }
    }
}
