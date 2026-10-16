namespace Part_2.Models
{//start of namespace
    public class RaceEvent
    {//start of class
        //Primary Key
        public int EventID { get; set; }
        
        //Foreign Key
        public int OrganizerID { get; set; }
        
        //Foreign Key
        public int CategoryID { get; set; }
        
        //Foreign Key
        public int RouteID { get; set; }

        //Foreign Key
        public string EventName { get; set; } = "";
        
        //Foreign Key
        public DateTime EventDate { get; set; }
        
        //Foreign Key
        public string Location { get; set; } = "";
        
        //Foreign Key
        public decimal EntryFee { get; set; }

        //Foreign Key
        public string Status { get; set; } = "Open";

    }//end of class
}//end of namespace
