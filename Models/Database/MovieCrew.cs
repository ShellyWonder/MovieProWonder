namespace MovieProWonder.Models.Database
{
    public class MovieCrew
    {
        public int Id { get; set; } 

        public int MovieId { get; set; }  
        
        //supplied by TMDB API
        public int CrewID { get; set; } 

        public string Department { get; set; }
        public string Name { get; set; }   
        
        public string Job { get; set; }

        public string ImageUrl { get; set; }    

        //nav prop that stores entire record for display
        public Movie Movie { get; set; }
    }
}
