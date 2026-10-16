namespace Part_2.Models
{//Start of namespace
    public class Category
    {//Start of class
        //Primary Key
        public int CategoryID { get; set; }

        //Foreign Key
        public string CategoryName { get; set; } = "";

        //Foreign Key
        public string? Description { get; set; }
    }//End of class
}//End of namespace
