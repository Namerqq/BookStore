using BookStore.DAO;
using BookStore.DTOs;
using BookStore.Utilities;

namespace BookStore.Tests
{
    public class OrderAndBusinessLogicTests
    {
        [Theory]
        [InlineData(0, 30000)]
        [InlineData(500, 30000)]
        [InlineData(3000, 30000)]
        [InlineData(3001, 45000)]
        [InlineData(4000, 45000)]
        [InlineData(4001, 60000)]
        public void ShippingCalculator_CalculatesTieredWeightFeesCorrectly(int totalGrams, decimal expectedFee)
        {
            Assert.Equal(expectedFee, ShippingCalculator.Calculate(totalGrams));
        }

        [Theory]
        [InlineData(OrderStatus.PendingPayment, OrderStatus.Pending, true)]
        [InlineData(OrderStatus.PendingPayment, OrderStatus.Cancelled, true)]
        [InlineData(OrderStatus.PendingPayment, OrderStatus.Delivered, false)]
        [InlineData(OrderStatus.Pending, OrderStatus.Processing, true)]
        [InlineData(OrderStatus.Pending, OrderStatus.OnHold, false)]
        [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
        [InlineData(OrderStatus.Pending, OrderStatus.Delivered, false)]
        [InlineData(OrderStatus.OnHold, OrderStatus.Processing, true)]
        [InlineData(OrderStatus.OnHold, OrderStatus.Cancelled, true)]
        [InlineData(OrderStatus.Processing, OrderStatus.Shipped, true)]
        [InlineData(OrderStatus.Processing, OrderStatus.OnHold, true)]
        [InlineData(OrderStatus.Processing, OrderStatus.Delivered, false)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Delivered, true)]
        [InlineData(OrderStatus.Shipped, OrderStatus.OnHold, true)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled, false)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Cancelled, false)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Pending, false)]
        [InlineData("UNKNOWN", OrderStatus.Pending, false)]
        public void OrderStatus_CanTransition_MatchesStateMachine(string current, string target, bool expectedAllowed)
        {
            Assert.Equal(expectedAllowed, OrderStatus.CanTransition(current, target));
        }

        [Fact]
        public void OrderDtos_ComputeLineTotalsAvailabilityAndCheckoutTotal()
        {
            var validLine = new CartLine
            {
                UnitPrice = 50000m,
                Quantity = 3,
                Stock = 5,
                ProductStatus = "ACTIVE"
            };
            Assert.Equal(150000m, validLine.LineTotal);
            Assert.True(validLine.IsAvailable);
            Assert.Equal(string.Empty, validLine.Warning);

            var inactiveLine = new CartLine { Stock = 10, Quantity = 1, ProductStatus = "INACTIVE" };
            Assert.False(inactiveLine.IsAvailable);
            Assert.Equal("Ngừng kinh doanh", inactiveLine.Warning);

            var outOfStockLine = new CartLine { Stock = 0, Quantity = 1, ProductStatus = "ACTIVE" };
            Assert.False(outOfStockLine.IsAvailable);
            Assert.Equal("Hết hàng", outOfStockLine.Warning);

            var exceedingLine = new CartLine { Stock = 2, Quantity = 4, ProductStatus = "ACTIVE" };
            Assert.False(exceedingLine.IsAvailable);
            Assert.Equal("Chỉ còn 2 sản phẩm", exceedingLine.Warning);

            var quote = new CheckoutQuote
            {
                Subtotal = 200000m,
                Discount = 25000m,
                ShippingFee = 30000m
            };
            Assert.Equal(205000m, quote.Total);
        }

        [Fact]
        public void ProductReviewDto_And_MoneyText_FormatExpectedStrings()
        {
            var review = new ProductReviewDto { Rating = 4 };
            Assert.Equal("★★★★☆", review.StarsText);

            var fiveStars = new ProductReviewDto { Rating = 5 };
            Assert.Equal("★★★★★", fiveStars.StarsText);

            Assert.Equal("125.000đ", MoneyText.Format(125000m));
        }
    }
}
