namespace Part_2.Models
{//start of namespace
    public class Registration
    {//start of class
        public int RegistrationID { get; set; }

        public int EventID { get; set; }

        public int ParticipantID { get; set; }

        public DateTime RegistrationDate { get; set; }

        public int? RaceNumber { get; set; }

        public string PaymentStatus { get; set; } = "Pending";

        public string? FinishTime { get; set; }

        public int? Position { get; set; }

        public decimal? AveragePace { get; set; }
         
        public string? ResultStatus { get; set; } = "Registered";

    }//end of class
}//end of namespace
