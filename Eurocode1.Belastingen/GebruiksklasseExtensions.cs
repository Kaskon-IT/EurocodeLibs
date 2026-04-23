namespace Eurocode.Belastingen
{
    public static class GebruiksklasseExtensions
    {
        /// <summary>
        /// Haal de opgelegde belasting op aan de hand van de gebruiksklasse
        /// </summary>
        /// <param name="gebruiksklasse">de gebruiksklasse</param>
        /// <returns>Opgelagde belastingen</returns>
        public static OpgelegdeBelastingen GetOpgelegdeBelastingen(this GebruiksklasseEnum gebruiksklasse)
        {
            return gebruiksklasse switch
            {
                GebruiksklasseEnum.A_woongebouwen_vloeren or
                GebruiksklasseEnum.A_niet_gemeenschappelijk_vloeren => new OpgelegdeBelastingen(1.75, 3.0),
                
                GebruiksklasseEnum.A_woongebouwen_trappen or
                GebruiksklasseEnum.A_niet_gemeenschappelijk_trappen or
                GebruiksklasseEnum.A_woongebouwen_ontsluitingswegen => new OpgelegdeBelastingen(2.0, 3.0),

                GebruiksklasseEnum.A_woongebouwen_balkons or
                GebruiksklasseEnum.A_niet_gemeenschappelijk_balkons => new OpgelegdeBelastingen(2.5, 3.0),

                GebruiksklasseEnum.A_gemeenschappelijke_vloeren or
                GebruiksklasseEnum.B_kantoorgebouwen => new OpgelegdeBelastingen(3.0, 3.0),

                GebruiksklasseEnum.C1_bijeenkomstgebouwen_tafels or
                GebruiksklasseEnum.C2_bijeenkomstgebouwen_vaste_stoelen => new OpgelegdeBelastingen(4.0, 7.0),

                GebruiksklasseEnum.C3_bijeenkomstgebouwen_zonder_obstakels or
                GebruiksklasseEnum.C4_bijeenkomstgebouwen_fysieke_activiteiten or
                GebruiksklasseEnum.C5_bijeenkomst_grote_menigtes => new OpgelegdeBelastingen(5.0, 7.0),
                GebruiksklasseEnum.D1_winkelruimte_kleinhandel or
                GebruiksklasseEnum.D2_winkelruimte_warenhuizen => new OpgelegdeBelastingen(4.0, 7.0),
                GebruiksklasseEnum.E1_opslag_winkels => new OpgelegdeBelastingen(5.0, 7.0),
                GebruiksklasseEnum.E1_opslag_bibliotheken => new OpgelegdeBelastingen(2.5, 3.0),
                GebruiksklasseEnum.E1_opslag_overige => new OpgelegdeBelastingen(5.0, 10.0),
                GebruiksklasseEnum.E2_industrieel_gebruik => new OpgelegdeBelastingen(3.0, 7.0),
                GebruiksklasseEnum.F_garages_tot_25kN => new OpgelegdeBelastingen(2.5, 10.0),
                GebruiksklasseEnum.F_garages_25_tot_120kN => new OpgelegdeBelastingen(5.0, 40.0),
                GebruiksklasseEnum.H_daken_alleen_toegankelijk_voor_onderhoud_0_tot_20_graden => new OpgelegdeBelastingen(1.0, 1.5),
                GebruiksklasseEnum.H_daken_alleen_toegankelijk_voor_onderhoud_meer_dan_20_graden => new OpgelegdeBelastingen(0, 1.5),
                GebruiksklasseEnum.H_daken_van_ruimten_onder_maaiveld_geen_verkeersbelasting => new OpgelegdeBelastingen(4, 7.0),
                GebruiksklasseEnum.EigenOpgave => new OpgelegdeBelastingen(0.0, 0.0),
                _ => new OpgelegdeBelastingen(3.0, 3.0),
            };
        }

        /// <summary>
        /// Haal de momentaan-factoren op.
        /// </summary>
        /// <param name="gebruiksklasse">de gebruiksklasse</param>
        /// <returns>Momentaan-factoren (Ψ₀, Ψ₁ en Ψ₂) </returns>
        public static MomentaanFactoren GetMomentaanFactoren(this GebruiksklasseEnum gebruiksklasse)
        {
            return gebruiksklasse switch
            {
                GebruiksklasseEnum.A_gemeenschappelijke_vloeren or
                GebruiksklasseEnum.A_woongebouwen_vloeren or
                GebruiksklasseEnum.A_woongebouwen_trappen or
                GebruiksklasseEnum.A_woongebouwen_ontsluitingswegen or
                GebruiksklasseEnum.A_woongebouwen_balkons or
                GebruiksklasseEnum.A_niet_gemeenschappelijk_vloeren or
                GebruiksklasseEnum.A_niet_gemeenschappelijk_trappen or
                GebruiksklasseEnum.A_niet_gemeenschappelijk_balkons => new MomentaanFactoren(mom0: 0.4, mom1: 0.5, mom2: 0.3),
                GebruiksklasseEnum.B_kantoorgebouwen => new MomentaanFactoren(mom0: 0.5, mom1: 0.5, mom2: 0.3),
                GebruiksklasseEnum.C1_bijeenkomstgebouwen_tafels or
                GebruiksklasseEnum.C2_bijeenkomstgebouwen_vaste_stoelen or
                GebruiksklasseEnum.C3_bijeenkomstgebouwen_zonder_obstakels or
                GebruiksklasseEnum.C4_bijeenkomstgebouwen_fysieke_activiteiten or
                GebruiksklasseEnum.C5_bijeenkomst_grote_menigtes => new MomentaanFactoren(mom0: 0.6, mom1: 0.7, mom2: 0.6),
                GebruiksklasseEnum.D1_winkelruimte_kleinhandel or
                GebruiksklasseEnum.D2_winkelruimte_warenhuizen => new MomentaanFactoren(mom0: 0.4, mom1: 0.7, mom2: 0.6),
                GebruiksklasseEnum.E1_opslag_winkels or
                GebruiksklasseEnum.E1_opslag_bibliotheken or
                GebruiksklasseEnum.E1_opslag_overige or
                GebruiksklasseEnum.E2_industrieel_gebruik => new MomentaanFactoren(mom0: 1.0, mom1: 0.9, mom2: 0.8),
                GebruiksklasseEnum.F_garages_tot_25kN => new MomentaanFactoren(mom0: 0.7, mom1: 0.7, mom2: 0.6),
                GebruiksklasseEnum.F_garages_25_tot_120kN => new MomentaanFactoren(mom0: 0.7, mom1: 0.5, mom2: 0.3),
                GebruiksklasseEnum.H_daken_alleen_toegankelijk_voor_onderhoud_0_tot_20_graden or
                GebruiksklasseEnum.H_daken_alleen_toegankelijk_voor_onderhoud_meer_dan_20_graden or
                GebruiksklasseEnum.H_daken_van_ruimten_onder_maaiveld_geen_verkeersbelasting => new MomentaanFactoren(mom0: 0.0, mom1: 0.0, mom2: 0.0),
                GebruiksklasseEnum.EigenOpgave => new MomentaanFactoren(mom0: 0.4, mom1: 0.5, mom2: 0.3),
                _ => new MomentaanFactoren(mom0: 0.4, mom1: 0.5, mom2: 0.3),

            };
        }


    }



}
