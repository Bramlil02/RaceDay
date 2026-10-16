namespace Part_2.Models
{//start of namespace
    public class EventOrganiser
    {//start of class

        //Primary Key
        public int OrganizerID { get; set; }

        //Foreign Key
        public string OrganizerName { get; set; } = "";

        //Foreign Key
        public string OrganizerEmail { get; set; } = "";

        //Foreign Key
        public string? PhoneNumber { get; set; }

    }//End of class
}//End of namspace
