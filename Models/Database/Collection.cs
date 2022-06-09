namespace MovieProWonder.Models.Database
{
    public class Collection
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        //collection of collections--child property

        public ICollection<MovieCollection> MovieCollections  { get; set; } = new HashSet<MovieCollection>();


    }
}
