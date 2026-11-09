namespace RaceDayMVC.Helpers
{
    // Small display helpers used by the views.
    public static class Format
    {
        // 1 -> 1st, 2 -> 2nd, 47 -> 47th
        public static string Ordinal(int number)
        {
            int lastTwo = number % 100;

            if (lastTwo >= 11 && lastTwo <= 13)
                return number + "th";

            switch (number % 10)
            {
                case 1: return number + "st";
                case 2: return number + "nd";
                case 3: return number + "rd";
                default: return number + "th";
            }
        }

        public static string Money(decimal amount)
        {
            return amount <= 0 ? "Free" : "R " + amount.ToString("N2");
        }
    }
}
