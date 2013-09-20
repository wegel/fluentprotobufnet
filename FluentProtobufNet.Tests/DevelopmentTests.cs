using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using ProtoBuf.Meta;

namespace FluentProtobufNet.Tests
{
    [TestFixture]
    public class DevelopmentTests
    {
        [Test]
        public Configuration CanBuildConfiguration()
        {
            return Fluently.Configure()
                .Mappings(m => 
                    m.FluentMappings.AddFromAssemblyOf<CategoryMap>())
                .BuildConfiguration();
        }

        [Test]
        public void CorrectlyMapsSingleLevelSubclasses()
        {
            var config = CanBuildConfiguration();

            var types = config.RuntimeTypeModel.GetTypes().Cast<MetaType>();
            var category =
                types.SingleOrDefault(t => t.Type == typeof (Category));

            Assert.IsTrue(category.HasSubtypes);
            Assert.IsTrue(category.GetSubtypes()[0].DerivedType.Type == typeof(CategoryWithDescription));
        }

        [Test]
        public void CorrectlyMapsUpToThirdLevelSubclass()
        {
            var config = CanBuildConfiguration();

            var types = config.RuntimeTypeModel.GetTypes().Cast<MetaType>();
            var categoryWithDescription =
                types.SingleOrDefault(t => t.Type == typeof(CategoryWithDescription));

            Assert.IsTrue(categoryWithDescription.HasSubtypes);
            Assert.IsTrue(categoryWithDescription.GetSubtypes()[0].DerivedType.Type == typeof(CategoryThirdLevel));
        }
    }

    public class CategoryMap : ClassMap<Category>
    {
        public CategoryMap()
        {
            Map(m => m.Name, 1);
            Map(m => m.SubCategories, 2);
            Map(m => m.Items, 3);
            References(m => m.ParentCategory, 4);

        }
    }

    public class CategoryWithDescriptionMap : SubclassMap<CategoryWithDescription>
    {
        public CategoryWithDescriptionMap()
        {
            SubclassFieldId(1);
            Map(c => c.Description, 1);
        }
    }

    public class CategoryThirdLevelMap : SubclassMap<CategoryThirdLevel>
    {
        public CategoryThirdLevelMap()
        {
            SubclassFieldId(2);
            Map(c => c.ThirdLevel, 1);
        }
    }

    public class ItemMap : ClassMap<Item>
    {
        public ItemMap()
        {
            Map(m => m.SKU, 1);

        }
    }


}
