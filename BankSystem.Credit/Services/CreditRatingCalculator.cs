namespace BankSystem.Credit.Services
{
    public static class CreditRatingCalculator
    {
        public static int CalculateRating(int overdueDays, int defaults, int onTimePayments)
        {
            var rating = 100 - (overdueDays * 2) - (defaults * 15) + (onTimePayments * 1);
            return Math.Clamp(rating, 0, 100);
        }

        public static string GetRatingDescription(int rating)
        {
            return rating switch
            {
                >= 80 => "Excellent",
                >= 60 => "Good",
                >= 40 => "Fair",
                >= 20 => "Poor",
                _ => "Very Poor"
            };
        }
    }
}
