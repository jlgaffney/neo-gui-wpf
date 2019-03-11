namespace Neo.Gui.Cross.Services
{
    public interface IGasClaimCalculationService
    {
        Fixed8 CalculateAvailableBonusGas();

        Fixed8 CalculateUnavailableBonusGas();
    }
}
