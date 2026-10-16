namespace Part_2.Models
{//start of namespace
    public class Registration
    {//start of class
        //Primary Key
        public int RegistrationID { get; set; }
        //Foreign Keys
        public int EventID { get; set; }
        //Foreign Key
        public int ParticipantID { get; set; }
        //Foreign Key
        public DateTime RegistrationDate { get; set; }
        //Foreign Key
        public int? RaceNumber { get; set; }
        //Foreign Key
        public string PaymentStatus { get; set; } = "Pending";
        //Foreign Key
        public string? FinishTime { get; set; }
        //Foreign Key
        public int? Position { get; set; }
        //Foreign Key
        public decimal? AveragePace { get; set; }
        //Foreign Key
        public string? ResultStatus { get; set; } = "Registered";

    }//end of class
}//end of namespace
