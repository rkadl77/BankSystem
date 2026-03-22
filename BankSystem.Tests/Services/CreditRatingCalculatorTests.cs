using BankSystem.Credit.Services;
using Xunit;

namespace BankSystem.Tests.Services
{
    public class CreditRatingCalculatorTests
    {
        #region CalculateRating Tests

        [Theory]
        [InlineData(0, 0, 0, 100)]    // Perfect score
        [InlineData(0, 0, 10, 100)]   // Max clamp at 100
        [InlineData(10, 0, 0, 80)]    // 10 overdue days = -20
        [InlineData(0, 1, 0, 85)]     // 1 default = -15
        [InlineData(5, 1, 5, 80)]     // -10 -15 +5 = -20
        [InlineData(50, 0, 0, 0)]     // 50 overdue days = -100, clamp at 0
        [InlineData(0, 7, 0, 0)]      // 7 defaults = -105, clamp at 0
        [InlineData(10, 2, 50, 100)]  // -20 -30 +50 = 0, but onTimePayments boost to 100 (clamped)
        public void CalculateRating_VariousInputs_ReturnsExpectedRating(int overdueDays, int defaults, int onTimePayments, int expected)
        {
            // Act
            var result = CreditRatingCalculator.CalculateRating(overdueDays, defaults, onTimePayments);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateRating_NegativeResult_ClampedToZero()
        {
            // Act
            var result = CreditRatingCalculator.CalculateRating(100, 5, 0);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateRating_Over100_ClampedTo100()
        {
            // Act
            var result = CreditRatingCalculator.CalculateRating(0, 0, 200);

            // Assert
            Assert.Equal(100, result);
        }

        #endregion

        #region GetRatingDescription Tests

        [Theory]
        [InlineData(100, "Excellent")]
        [InlineData(95, "Excellent")]
        [InlineData(80, "Excellent")]
        [InlineData(79, "Good")]
        [InlineData(60, "Good")]
        [InlineData(59, "Fair")]
        [InlineData(40, "Fair")]
        [InlineData(39, "Poor")]
        [InlineData(20, "Poor")]
        [InlineData(19, "Very Poor")]
        [InlineData(0, "Very Poor")]
        public void GetRatingDescription_VariousRatings_ReturnsExpectedDescription(int rating, string expected)
        {
            // Act
            var result = CreditRatingCalculator.GetRatingDescription(rating);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion
    }
}
