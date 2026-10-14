namespace Part_2.Models
{//start of namespace
    public class RaceRoute
    {//start of class
        public int RouteId { get; set; }

        public string RouteName { get; set; } = "";

        public decimal DistanceKm { get; set; }

        public string StartPoint { get; set; } = "";

        public string FinishPoint { get; set; } = "";

        public string? Description { get; set; }
    }//end of class
}//end of namespace
