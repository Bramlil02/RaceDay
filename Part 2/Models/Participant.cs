namespace Part_2.Models
{//Start of namespace
    public class Participant
    {//Start of class
        public int ParticipantID { get; set; }
        public string Firstname { get; set; } = "";

        public string Lastname { get; set; } = "";

        public string ParticipantsEmail { get; set; } = "";

        public string? PhoneNumber { get; set; }

        public DateTime DateOfBirth {  get; set; }


    }//End of Class
}//End of namespace
