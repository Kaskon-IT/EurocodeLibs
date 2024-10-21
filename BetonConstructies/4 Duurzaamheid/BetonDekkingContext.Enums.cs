using System.ComponentModel;

namespace Eurocode.BetonConstructies
{



    public partial class BetonDekkingContext
    {

        public enum BetonStortOndergrondEnum { [Description("Gladde bekisting of n.v.t.")] GladdeBekistingOfNvt = 1, [Description("Werkvloer")] Werkvloer = 2, [Description("Op of tegen de grond")] OpOfTegenGrond = 3 }


        public enum BetonAfwerkingOppervlakEnum { [Description("Glad")] Glad = 1, [Description("Nabewerkt of oneffen")] NabewerktOnEffen = 2 }



    }


}
