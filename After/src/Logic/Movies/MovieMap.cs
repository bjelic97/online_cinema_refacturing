using FluentNHibernate;
using FluentNHibernate.Mapping;
using Logic.Entities;

namespace Logic.Mappings
{
    public class MovieMap : ClassMap<Movie>
    {
        public MovieMap()
        {
            Id(x => x.Id);

            DiscriminateSubClassesOnColumn("LicensingModel");

            Map(x => x.Name);
            Map(Reveal.Member<Movie>("LicensingModel")).CustomType<int>(); // Reveal - maps unpublic fields & props 
        }
    }

    public class TwoDaysMovieMap : ClassMap<Movie>
    {
        public TwoDaysMovieMap()
        {
            DiscriminatorValue(1);
        }
    }

    public class LifeLongMovieMap : ClassMap<Movie>
    {
        public LifeLongMovieMap()
        {
            DiscriminatorValue(2);
        }
    }
}
