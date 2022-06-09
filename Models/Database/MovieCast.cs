namespace MovieProWonder.Models.Database
{
    //stores credit data returned by API
    public class MovieCast
    {
        public int Id { get; set; }
        
        //foreign key links cast member to movie
        public int MovieId { get; set; }
       
        //retrives detailed info on cast member
        public int CastID { get; set; }

        public string Name { get; set; }

        public string Department { get; set; }  
        public string Character { get; set; }   
        public string ImageUrl { get; set; }    

        //navigational property
        public Movie Movie { get; set; }    


        

    }
}
