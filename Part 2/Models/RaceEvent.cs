namespace Part_2.Models
{//start of namespace
    public class RaceEvent
    {//start of class
        public int EventID { get; set; }

        public int OrganizerID { get; set; }

        public int CategoryID { get; set; }

        public int RouteID { get; set; }

        public string EventName { get; set; } = "";
        
        public DateTime EventDate { get; set; }

        public string Location { get; set; } = "";

        public decimal EntryFee { get; set; }

        public string Status { get; set; } = "Open";

    }//end of class
}//end of namespace
