namespace Part_2.Models
{//start of namespace
    public class RaceRoute
    {//start of class

        //Primary Key
        public int RouteId { get; set; }
        //Foreign Key
        public string RouteName { get; set; } = "";
        //Foreign Key
        public decimal DistanceKm { get; set; }
        //Foreign Key
        public string StartPoint { get; set; } = "";
        //Foreign Key
        public string FinishPoint { get; set; } = "";
        //Foreign Key
        public string? Description { get; set; }

    }//end of class
}//end of namespace
