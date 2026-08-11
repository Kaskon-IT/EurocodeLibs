namespace CommonLibrary.Modelling
{
    public enum VerdelingType
    {
        /// <summary> n staven gelijkmatig over de verdeellijn; h.o.h. volgt. </summary>
        Gelijkmatig,

        /// <summary>
        /// Expliciete lijst h.o.h.-afstanden vanaf VerdeelStart (b.v. 50+3×150+50);
        /// het aantal staven (= afstanden + 1) en het eindpunt volgen hieruit.
        /// VerdeelEind wordt berekend/overschreven.
        /// </summary>
        ExacteHartOpHart,

        /// <summary>
        /// Beoogde (maximale) h.o.h.-maat; het aantal wordt berekend
        /// (ceil(L/s) + 1) en de staven worden vervolgens gelijkmatig verdeeld.
        /// Werkelijke h.o.h. ≤ beoogde maat.
        /// </summary>
        BeoogdeHartOpHart,
    }

    /// <summary> Verdeling van de staven van een <see cref="WapeningGroep"/> langs de verdeellijn. </summary>
    public class WapeningVerdeling
    {
        public VerdelingType Type { get; set; }

        /// <summary> Bij Gelijkmatig: opgegeven. Bij de h.o.h.-typen: berekend. </summary>
        public int Aantal { get; set; }

        /// <summary> Bij ExacteHartOpHart: n afstanden [mm]; Aantal = n + 1. </summary>
        public List<double> HartOpHartAfstanden { get; set; } = [];

        /// <summary> Bij BeoogdeHartOpHart: beoogde (maximale) h.o.h.-maat s [mm]. </summary>
        public double BeoogdeHartOpHart { get; set; }

        /// <summary>
        /// Offset [mm] vanaf VerdeelStart, in de richting van VerdeelEind.
        /// De verdeling begint op VerdeelStart + OffsetStart·richting.
        /// </summary>
        public double OffsetStart { get; set; }

        /// <summary>
        /// Offset [mm] vanaf VerdeelEind, terug richting VerdeelStart.
        /// De verdeling eindigt op VerdeelEind − OffsetEind·richting.
        /// </summary>
        public double OffsetEind { get; set; }

        public void Valideer()
        {
            if (OffsetStart < 0 || OffsetEind < 0)
            {
                throw new InvalidOperationException("OffsetStart en OffsetEind moeten ≥ 0 zijn.");
            }

            switch (Type)
            {
                case VerdelingType.Gelijkmatig when Aantal < 1:
                    throw new InvalidOperationException("Bij Gelijkmatig is Aantal ≥ 1 vereist.");
                case VerdelingType.ExacteHartOpHart when HartOpHartAfstanden.Count == 0 || HartOpHartAfstanden.Any(s => s <= 0):
                    throw new InvalidOperationException("Bij ExacteHartOpHart is een niet-lege lijst met positieve afstanden vereist.");
                case VerdelingType.BeoogdeHartOpHart when BeoogdeHartOpHart <= 0:
                    throw new InvalidOperationException("Bij BeoogdeHartOpHart is s > 0 vereist.");
            }
        }
    }
}
