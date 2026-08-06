namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Berekent de verankeringslengte van wapening volgens EC2 §8.3 (buigstralen) en §8.4 (verankering).
    /// Opgezet als herbruikbare, zuivere rekenklasse (net als <see cref="J3ConsoleCalculator"/>), zodat de
    /// berekening zowel als aanvulling op de J3-console als los kan worden ingezet.
    /// </summary>
    public static class VerankeringslengteCalculator
    {
        public static VerankeringResult Bereken(VerankeringslengteInput i)
        {
            bool isTrek = i.StaafType == VerankeringStaafType.Trekstaaf;
            bool isGebogen = i.StaafVorm == VerankeringStaafVorm.Gebogen;

            // 8.4.2 (2) Uiterst opneembare aanhechtspanning
            double fctd = BepaalFctd(i.Beton);
            double eta1 = i.GoedeAanhechting ? 1.0 : 0.7;
            double eta2 = i.Diameter <= 32.0 ? 1.0 : (132.0 - i.Diameter) / 100.0;
            double fbd = 2.25 * eta1 * eta2 * fctd; // (8.2)
            double fcd = BepaalFcdBuiging(i.Beton);
            double fbt = i.Fbt;
            double ab = i.Ab;

            // 8.4.3 (2) Basisverankeringslengte
            double lbRqd = (i.Diameter / 4.0) * (i.RekenwaardeStaafspanning / fbd); // (8.3)

            // Tabel 8.2 α-factoren
            double alpha1 = BepaalAlpha1(isTrek, isGebogen, i.Diameter, i.Cd);
            double alpha2 =  BepaalAlpha2(isTrek, isGebogen, i.Diameter, i.Cd);
            double alpha3 = i.Alpha3;
            double alpha4 = i.Alpha4;
            double alpha5 = i.Alpha5;

            // (8.5) product α2·α3·α5 ≥ 0,7
            double productAlpha235 = Math.Max(alpha2 * alpha3 * alpha5, 0.7);

            // 8.4.4 (1) minimum verankeringslengte (8.6)/(8.7)
            double lbMin = isTrek
                ? Math.Max(0.3 * lbRqd, Math.Max(10.0 * i.Diameter, 100.0))   // (8.6) trekstaaf
                : Math.Max(0.6 * lbRqd, Math.Max(10.0 * i.Diameter, 100.0));  // (8.7) drukstaaf

            // 8.4.4 (1) rekenwaarde verankeringslengte (8.4)
            double lbd = Math.Max(alpha1 * productAlpha235 * alpha4 * lbRqd, lbMin);

            // 8.3 buigstralen
            double buigrolDiameter = BepaalMinimaleBuigrolDiameter(i.Diameter);         // Tabel 8.1N
            double buigrolDiameterBeton = BepaalBuigrolDiameterBeton(i);                // (8.1)
            double buigstraal = buigrolDiameter / 2.0;                                  // r = Øm/2

            var result = new VerankeringResult
            {
                Diameter = i.Diameter,
                RekenwaardeStaafspanning = i.RekenwaardeStaafspanning,
                StaafType = i.StaafType,
                StaafVorm = i.StaafVorm,

                Fctd = fctd,
                Eta1 = eta1,
                Eta2 = eta2,
                Fbd = fbd,
                Fcd = fcd,
                Fbt = fbt,
                Ab = ab,


                BasisVerankeringslengte = lbRqd,

                Alpha1 = alpha1,
                Alpha2 = alpha2,
                Alpha3 = alpha3,
                Alpha4 = alpha4,
                Alpha5 = alpha5,
                ProductAlpha235 = productAlpha235,

                MinimumVerankeringslengte = lbMin,
                Verankeringslengte = lbd,

                MinimaleBuigdoornDiamStaal = buigrolDiameter,
                MinimaleBuigdoornDiameterBeton = buigrolDiameterBeton,
                MinimaleBuigstraal = buigstraal,

                ToegepasteVerankeringslengte = i.ToegepasteVerankeringslengte,
            };

            VulRegels(i, result);

            return result;
        }

        /// <summary>
        /// f~ctd~ voor de aanhechting. Bij hogere sterkte begrensd tot de waarde voor C60/75 (§8.4.2 (2)).
        /// </summary>
        private static double BepaalFctd(BetonContext beton)
        {
            if (beton.Fck <= 60.0)
                return beton.Fctd;

            var c60 = new BetonContext(BetonsterkteklasseEnum.C60_75);
            return c60.Fctd;
        }

        /// <summary>
        /// α1 – vorm van de staaf (Tabel 8.2). Voor gebogen trekstaven 0,7 mits c~d~ &gt; 3Ø.
        /// </summary>
        private static double BepaalAlpha1(bool isTrek, bool isGebogen, double diameter, double cd)
        {
            if (!isTrek || !isGebogen)
                return 1.0;

            return cd > 3.0 * diameter ? 0.7 : 1.0;
        }

        /// <summary>
        /// α2 – betondekking (Tabel 8.2). Alleen bij trekstaven, begrensd tot 0,7 ≤ α2 ≤ 1,0.
        /// </summary>
        private static double BepaalAlpha2(bool isTrek, bool isGebogen, double diameter, double cd)
        {
            if (!isTrek)
                return 1.0;

            double alpha2 = isGebogen
                ? 1.0 - 0.15 * (cd - 3.0 * diameter) / diameter   // gebogen staaf
                : 1.0 - 0.15 * (cd - diameter) / diameter;        // rechte staaf

            return Math.Clamp(alpha2, 0.7, 1.0);
        }

        /// <summary>
        /// Minimale buigroldiameter volgens Tabel 8.1N (voorkomen van beschadiging van de wapening).
        /// </summary>
        private static double BepaalMinimaleBuigrolDiameter(double diameter)
        {
            return diameter <= 16.0 ? 4.0 * diameter : 7.0 * diameter;
        }

        /// <summary>
        /// Minimale buigroldiameter om betondrukbezwijken bij de buiging te voorkomen (8.1).
        /// Retourneert 0 wanneer geen trekkracht/afstand is opgegeven.
        /// </summary>
        private static double BepaalBuigrolDiameterBeton(VerankeringslengteInput i)
        {
            if (i.Fbt <= 0 || i.Ab <= 0)
                return 0;

            double fcd = BepaalFcdBuiging(i.Beton);
            return i.Fbt * (1.0 / i.Ab + 1.0 / (2.0 * i.Diameter)) / fcd; // (8.1)
        }

        /// <summary>
        /// f~cd~ voor de buigcontrole (8.1). Begrensd tot de waarde voor C55/67 (§8.3 (3)).
        /// </summary>
        private static double BepaalFcdBuiging(BetonContext beton)
        {
            if (beton.Fck <= 55.0)
                return beton.Fcd;

            var c55 = new BetonContext(BetonsterkteklasseEnum.C55_67);
            return c55.Fcd;
        }

        private static void VulRegels(VerankeringslengteInput i, VerankeringResult r)
        {
            r.ResultRows.Add(new()
            {
                Toelichting = "Invoer",
                Artikel = "8.3 / 8.4"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Staafdiameter",
                SymboolHtml = "<i>Ø</i>",
                SymboolTex = @"Ø",
                Waarde = i.Diameter.ToString("0"),
                Eenheid = "mm",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Betonsterkteklasse",
                SymboolHtml = "<i>C</i>",
                Waarde = i.Beton.BetonSterkteKlasseGebruiksvriendelijkeNaam,
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Type staaf",
                Waarde = r.StaafType == VerankeringStaafType.Trekstaaf ? "trekstaaf" : "drukstaaf",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Vorm van de staaf",
                Waarde = r.StaafVorm == VerankeringStaafVorm.Gebogen ? "gebogen (haak/lus/buiging)" : "recht",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Rekenwaarde staafspanning",
                SymboolHtml = "<i>σ</i><sub>sd</sub>",
                SymboolTex = @"\sigma_{sd}",
                Waarde = r.RekenwaardeStaafspanning.ToString("0"),
                Eenheid = "N/mm²",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Dekkingsmaat (fig. 8.3)",
                SymboolHtml = "<i>c</i><sub>d</sub>",
                SymboolTex = @"c_d",
                Waarde = i.Cd.ToString("0"),
                Eenheid = "mm",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Berekening verankering",
                Artikel = "8.4"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Rekenwaarde treksterkte beton",
                SymboolHtml = "<i>f</i><sub>ctd</sub>",
                SymboolTex = @"f_{ctd}",
                Waarde = r.Fctd.ToString("0.00"),
                Eenheid = "N/mm²",
                Artikel = "3.1.6 (2)P",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Factor aanhechtingsomstandigheden",
                SymboolHtml = "<i>η</i><sub>1</sub>",
                SymboolTex = @"\eta_1",
                Waarde = r.Eta1.ToString("0.0"),
                Artikel = "8.4.2 (2)",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Factor staafdiameter",
                SymboolHtml = "<i>η</i><sub>2</sub>",
                SymboolTex = @"\eta_2",
                Waarde = r.Eta2.ToString("0.00"),
                FormuleTex = i.Diameter <= 32.0 ? @"\eta_2 = 1.0" : @"\eta_2 = (132-Ø)/100",
                Artikel = "8.4.2 (2)",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Rekenwaarde opneembare aanhechtspanning",
                SymboolHtml = "<i>f</i><sub>bd</sub>",
                SymboolTex = @"f_{bd}",
                Waarde = r.Fbd.ToString("0.00"),
                Eenheid = "N/mm²",
                FormuleTex = @"f_{bd}=2.25\,\eta_1\,\eta_2\,f_{ctd}",
                Artikel = "(8.2)",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Basisverankeringslengte",
                SymboolHtml = "<i>l</i><sub>b,rqd</sub>",
                SymboolTex = @"l_{b,rqd}",
                Waarde = r.BasisVerankeringslengte.ToString("0"),
                Eenheid = "mm",
                FormuleTex = @"l_{b,rqd}=\frac{Ø}{4}\cdot\frac{\sigma_{sd}}{f_{bd}}",
                Artikel = "(8.3)",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Factor vorm van de staaf",
                SymboolHtml = "<i>α</i><sub>1</sub>",
                SymboolTex = @"\alpha_1",
                Waarde = r.Alpha1.ToString("0.00"),
                Artikel = "Tabel 8.2",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Factor betondekking",
                SymboolHtml = "<i>α</i><sub>2</sub>",
                SymboolTex = @"\alpha_2",
                Waarde = r.Alpha2.ToString("0.00"),
                Artikel = "Tabel 8.2",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Factor opsluiting dwarswapening",
                SymboolHtml = "<i>α</i><sub>3</sub>",
                SymboolTex = @"\alpha_3",
                Waarde = r.Alpha3.ToString("0.00"),
                Artikel = "Tabel 8.2",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Factor gelaste dwarswapening",
                SymboolHtml = "<i>α</i><sub>4</sub>",
                SymboolTex = @"\alpha_4",
                Waarde = r.Alpha4.ToString("0.00"),
                Artikel = "Tabel 8.2",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Factor dwarsdruk",
                SymboolHtml = "<i>α</i><sub>5</sub>",
                SymboolTex = @"\alpha_5",
                Waarde = r.Alpha5.ToString("0.00"),
                Artikel = "Tabel 8.2",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Controle product factoren",
                SymboolTex = @"\alpha_2\alpha_3\alpha_5",
                Waarde = r.ProductAlpha235.ToString("0.00"),
                FormuleTex = @"\alpha_2\,\alpha_3\,\alpha_5 \geq 0.7",
                Artikel = "(8.5)",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Minimum verankeringslengte",
                SymboolHtml = "<i>l</i><sub>b,min</sub>",
                SymboolTex = @"l_{b,min}",
                Waarde = r.MinimumVerankeringslengte.ToString("0"),
                Eenheid = "mm",
                FormuleTex = r.StaafType == VerankeringStaafType.Trekstaaf
                    ? @"l_{b,min}=\max\{0.3\,l_{b,rqd};\,10Ø;\,100\}"
                    : @"l_{b,min}=\max\{0.6\,l_{b,rqd};\,10Ø;\,100\}",
                Artikel = r.StaafType == VerankeringStaafType.Trekstaaf ? "(8.6)" : "(8.7)",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Rekenwaarde verankeringslengte",
                SymboolHtml = "<i>l</i><sub>bd</sub>",
                SymboolTex = @"l_{bd}",
                Waarde = r.Verankeringslengte.ToString("0"),
                Eenheid = "mm",
                FormuleTex = @"l_{bd}=\alpha_1\alpha_2\alpha_3\alpha_4\alpha_5\,l_{b,rqd}\geq l_{b,min}",
                Artikel = "(8.4)",
            });

            if (r.IsUnityCheckUitgevoerd)
            {
                r.ResultRows.Add(new()
                {
                    Toelichting = "Controle beschikbare verankeringslengte",
                    SymboolTex = @"l_{bd}/l_{bd,prov}",
                    Waarde = $"{r.Verankeringslengte:0} ≤ {r.ToegepasteVerankeringslengte:0} (U.C. = {r.UnityCheck:0.00})",
                    Eenheid = "mm",
                    IsOk = r.IsVoldoende,
                    FormuleTex = @"l_{bd} \leq l_{bd,prov}",
                });
            }

            if (r.StaafVorm == VerankeringStaafVorm.Gebogen)
            {
                r.ResultRows.Add(new()
                {
                    Toelichting = "Buigstralen",
                    Artikel = "8.3"
                });

                r.ResultRows.Add(new()
                {
                    Toelichting = "Minimale buigroldiameter (beschadiging wapening)",
                    SymboolHtml = "<i>Ø</i><sub>m,min</sub>",
                    SymboolTex = @"Ø_{m,min}",
                    Waarde = r.MinimaleBuigdoornDiamStaal.ToString("0"),
                    Eenheid = "mm",
                    FormuleTex = i.Diameter <= 16.0 ? @"Ø_{m,min}=4Ø" : @"Ø_{m,min}=7Ø",
                    Artikel = "Tabel 8.1N",
                });

                if (r.IsBuigrolControleUitgevoerd)
                {
                    r.ResultRows.Add(new()
                    {
                        Toelichting = "Minimale buigroldiameter (betondrukbezwijken)",
                        SymboolHtml = "<i>Ø</i><sub>m,min</sub>",
                        SymboolTex = @"Ø_{m,min}",
                        Waarde = r.MinimaleBuigdoornDiameterBeton.ToString("0"),
                        Eenheid = "mm",
                        FormuleTex = @"Ø_{m,min}\geq F_{bt}\left(\frac{1}{a_b}+\frac{1}{2Ø}\right)/f_{cd}",
                        Artikel = "(8.1)",
                    });

                    r.ResultRows.Add(new()
                    {
                        Toelichting = "Maatgevende buigroldiameter",
                        SymboolHtml = "<i>Ø</i><sub>m,min</sub>",
                        SymboolTex = @"Ø_{m,min}",
                        Waarde = r.MinimaleBuigdoornDiameter.ToString("0"),
                        Eenheid = "mm",
                    });
                }

                r.ResultRows.Add(new()
                {
                    Toelichting = "Verankeringslengte",
                    SymboolTex = @"l_{b,req}",
                    Waarde = r.Verankeringslengte.ToString("0"),
                    Eenheid = "mm",

                });

                r.ResultRows.Add(new()
                {
                    Toelichting = "Minimale buigstraal (binnenzijde)",
                    SymboolHtml = "<i>r</i><sub>min</sub>",
                    SymboolTex = @"r_{min}",
                    Waarde = (r.MinimaleBuigdoornDiameter / 2.0).ToString("0"),
                    Eenheid = "mm",
                    FormuleTex = @"r_{min}=Ø_{m,min}/2",
                });

                
            }
        }
    }
}
