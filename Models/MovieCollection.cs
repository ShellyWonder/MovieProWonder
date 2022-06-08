using MovieProWonder.Models.Database;
using System.Collections.ObjectModel;

namespace MovieProWonder.Models
{
    public class MovieCollection
    {
        //primary key
        public int Id { get; set; }
       
        //two foreign keys
        public int CollectionId { get; set; }
        public int MovieId { get; set; }

        public int Order { get; set; }

        //store entire record pointed to foreign key
        public Collection Collection { get; set; }

        public Movie Movie { get; set; }
    }
}
