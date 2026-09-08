namespace CommonLibrary.Modelling
{
    /// <summary>
    /// Losse wapeningsstaaf (analoog aan Tekla SingleRebar): precies één
    /// <see cref="StaafShape"/> in wereldcoördinaten, geen verdeling.
    /// </summary>
    public class WapeningStaaf : BaseWapeningGroep
    {
        public override void Valideer()
        {
            base.Valideer();
            if (Shapes.Length != 1)
            {
                throw new InvalidOperationException("Een losse staaf heeft precies 1 staafvorm.");
            }
        }
    }
}
