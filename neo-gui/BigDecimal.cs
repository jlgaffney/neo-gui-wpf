using System.Numerics;

namespace Neo
{
    public struct BigDecimal
    {
        private readonly BigInteger value;
        private readonly byte decimals;

        public BigInteger Value => value;
        public byte Decimals => decimals;

        public BigDecimal(BigInteger value, byte decimals)
        {
            this.value = value;
            this.decimals = decimals;
        }

        public override string ToString()
        {
            var divisor = BigInteger.Pow(10, decimals);
            var result = BigInteger.DivRem(value, divisor, out var remainder);

            if (remainder == BigInteger.Zero) return result.ToString();

            return $"{result}.{remainder.ToString("d" + decimals)}".TrimEnd('0');
        }
    }
}
