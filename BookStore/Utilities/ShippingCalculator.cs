namespace BookStore.Utilities
{
    /// <summary>
    /// Phí ship theo cân nặng thật (mục 4.2.3): đến 3kg là 30.000đ,
    /// vượt 3kg thì cộng 15.000đ cho mỗi kg vượt, phần lẻ làm tròn lên 1kg.
    /// </summary>
    public static class ShippingCalculator
    {
        public const int BaseWeightGrams = 3000;
        public const decimal BaseFee = 30000m;
        public const decimal FeePerExtraKg = 15000m;

        public static decimal Calculate(int totalGrams)
        {
            if (totalGrams <= BaseWeightGrams) return BaseFee;

            decimal extraKg = Math.Ceiling((totalGrams - BaseWeightGrams) / 1000m);
            return BaseFee + extraKg * FeePerExtraKg;
        }
    }
}
