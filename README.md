Fluent protobuf-net
=================

Fluent protobuf-net offers an alternative to [protobuf-net](https://code.google.com/p/protobuf-net/) standard attributes-based mapping and string-based mapping. Fluent protobuf-net lets you write mappings in strongly typed C# code. This allows for easy refactoring, improved readability, more concise code and a natural seperation between mappings and classes.

Example
---------------------------------------------

Mapping using the standard protobuf-net attributes. 


        [ProtoContract]
        [ProtoInclude(1, typeof(Bird))]
        [ProtoInclude(2, typeof(Mammal))]
        public class Animal
        {
            [ProtoMember(1)]
            public string Name { get; set; }
            [ProtoMember(2, AsReference = true)]
            public AnimalGroup Group { get; set; }
        }

        [ProtoContract]
        public class Mammal : Animal
        {
            [ProtoMember(1)]
            public bool IsMonotreme { get; set; }
            [ProtoMember(2)]
            public bool LivesOnLand { get; set; }
        }

        [ProtoContract]
        public class Bird: Animal
        {
            [ProtoMember(1)]
            public bool CanFly { get; set; }
        }

        [ProtoContract]
        public class AnimalGroup
        {
            [ProtoMember(1)]
            public string Name { get; set; }
            [ProtoMember(2, AsReference = true)]
            public IList<Animal> Members { get; set; }
            [ProtoMember(3)]
            public bool AreWarmBlooded { get; set; }
        }
        
Now using Fluent protobuf-net. First the classes, without any special attributes:

        public class Animal
        {
            public string Name { get; set; }
            public AnimalGroup Group { get; set; }
        }

        public class Mammal : Animal
        {
            public bool IsMonotreme { get; set; }
            public bool LivesOnLand { get; set; }
        }

        public class Bird: Animal
        {
            public bool CanFly { get; set; }
        }

        public class AnimalGroup
        {
            public string Name { get; set; }
            public IList<Animal> Members { get; set; }
            public bool AreWarmBlooded { get; set; }
        }
        
And your mapping classes:

        public class AnimalMap : ClassMap<Animal>
        {
            public AnimalMap()
            {
                Map(a => a.Name, 1);
                References(a => a.Group, 2);
            }
        }

        public class MammalMap : SubclassMap<Mammal>
        {
            public MammalMap()
            {
                SubclassFieldId(2);

                Map(m => m.IsMonotreme, 1);
                Map(m => m.LivesOnLand, 2);
            }
        }

        public class BirdMap : SubclassMap<Bird>
        {
            public BirdMap()
            {
                SubclassFieldId(1);

                Map(b => b.CanFly, 1);
            }
        }

        public class AnimalGroupMap : ClassMap<AnimalGroup>
        {
            public AnimalGroupMap()
            {
                Map(g => g.Name, 1);
                References(g => g.Members, 2);
                Map(g => g.AreWarmBlooded, 3);
            }
        }
        
To get your RuntimeTypeModel (used to serialize/deserialize), you do:

        var config = Fluently.Configure()
                .Mappings(m => 
                    m.FluentMappings.AddFromAssemblyOf<CategoryMap>())
                .BuildConfiguration();
        config.RuntimeTypeModel.Serialize(...);
        

Thanks
---------------------------------------------

The basic architecture of Fluent protobuf-net was shamelessly copied from [Fluent NHibernate](https://github.com/jagregory/fluent-nhibernate). [Fluent NHibernate](https://github.com/jagregory/fluent-nhibernate) is GREAT; if you use NHibernate, use it.

Future
---------------------------------------------

The library was created in just a few short hours. A lot more is possible:
   - Support all of protobuf-net options (IsRequired, DataFormat, etc)
   - Automapping (with a standardised way to serialize the generated mappings)
   - Conventions
   
They will be added as I need them, and I'll gladly accept pull requests for features.

Have fun!
   
