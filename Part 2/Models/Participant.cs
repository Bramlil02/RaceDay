namespace Part_2.Models
{//Start of namespace
    public class Participant
    {//Start of class
        //Primary Key
        public int ParticipantID { get; set; }

        //Foreign Key
        public string Firstname { get; set; } = "";

        //Foreign Key
        public string Lastname { get; set; } = "";

        //Foreign Key
        public string ParticipantsEmail { get; set; } = "";

        //Foreign Key
        public string? PhoneNumber { get; set; }

        //Foreign Key
        public DateTime DateOfBirth {  get; set; }


    }//End of Class
}//End of namespace
