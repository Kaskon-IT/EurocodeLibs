using Eurocode.BetonConstructies;
using System.Globalization;
using System.Text;

namespace Eurocode.BetonConstructies
{



    public static class VerankeringMarkdownExtensions
    {

        public static string ToMarkdownText(this VerankeringResult v)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@$"De verankeringslengte $l_{{bd}}$={v.Verankeringslengte:0} mm. ");
            if (v.StaafVorm == VerankeringStaafVorm.Gebogen)
            {
                sb.Append($@"De staaf Ø{v.Diameter:0.#} wordt omgebogen (r={(v.ToegepasteBuigdoornDiameter/2.0):0} mm) na een rechte afstand van {v.AfstandTotFbt:0} mm.");
                
                //if (v.Verankeringslengte <= v.AfstandTotFbt)
                //{
                //    sb.Append(@$"$$l_{{bd}} \leq {v.AfstandTotFbt:0}$$");
                //}

                sb.Append(@$"$$ F_{{bt}} = {v.Fbt:0} N $$");
                sb.Append($@"$$ \phi_{{m,min}} \geq {v.MinimaleBuigdoornDiameter:0} mm $$");

                
                sb.Append($@"$$ \phi_m ={v.ToegepasteBuigdoornDiameter:0} mm $$");

                if (v.ToegepasteBuigdoornDiameter >= v.MinimaleBuigdoornDiameter)
                {
                    sb.Append($@"$$ \phi_{{m,prov}} \geq \phi_{{m,min}} $$ ✅");
                }
                else
                {
                    sb.Append($@"$$ \phi_{{m,prov}} \leq \phi_{{m,min}} $$ ❌");
                }
            }


            return sb.ToString();
        }

        public static string ToMarkdownTable(
            this Dictionary<WapeningGroep, List<VerankeringResult>> verankeringenPerGroep)
        {
            var verankeringen = verankeringenPerGroep
                .SelectMany(x => x.Value)
                .ToList();

            return verankeringen
            .ToMarkdownTable()
            .NumberColumn()
            .Column("Vorm", x => x.StaafVorm, alignment: MarkdownAlignment.Left)
            .Column("$F_{bt}$", x => x.Fbt, "0.0")
            .Column("$a_b$", x => x.Ab, "0.0")
            .Column("$f_{cd}$", x => x.Fcd, "0.00")
            .Column("$Ø_{m,prov}$", x => x.ToegepasteBuigdoornDiameter, "0")
            .Column("$Ø_{m,min}$", x => x.MinimaleBuigdoornDiameter, "0.0")
            .Column("$a_1$", x => x.AfstandTotFbt, "0")
            .Column("$a_{1,min}$", x => x.MinimaleRechteLengteTekst)
            .Column("OK", x => x.IsOk ? "✅" : "❌", alignment: MarkdownAlignment.Center)
            .Build();
        }


        public static string ToMarkdownLbd(
            this Dictionary<WapeningGroep, List<VerankeringResult>> verankeringenPerGroep)
        {
            var sb = new StringBuilder();
            sb.AppendLine("| Nr | Groep | $Ø_k$ | Pos |  $\\sigma_{sd}$ | $f_{bd}$ | $\\eta_1$ | $\\eta_2$ | $f_{ctd}$ | $l_{b,rqd}$ |  $\\prod_{i=1}^{5} \\alpha_i (\\alpha_2\\cdot\\alpha_3 \\alpha_5\\geq0.7)$ | $l_{{bd}}$ |");
            sb.AppendLine("|----|-------|:-----:|-----|----------------:|---------:|:---------:|:---------:|----------:|------------:|:--------------------------------------------------------------------------:|-----------:|");
            foreach (var (groep, verankeringen) in verankeringenPerGroep)
            {
                int nr = 1;
                foreach (var v in verankeringen)
                {
                    sb.AppendLine(
                        $"| {nr++} " +
                        $"| {Escape(groep.DisplayName)} " +
                        $"| Ø{F(groep.Diameter)} " +
                        $"| {Escape(v.Naam)}" +
                        $"| {F(v.RekenwaardeStaafspanning)} " +
                        $"| {F(v.Fbd, "0.##")}" +
                        $"| {F(v.Eta1, "0.##")}" +
                        $"| {F(v.Eta2, "0.##")}" +
                        $"| {F(v.Fctd, "0.##")}" +
                        $"| {F(v.BasisVerankeringslengte)}" +
                        $"| {F(v.Alpha1, "0.##")} " +
                            $"{F(v.Alpha2, "0.##")} " +
                            $"{F(v.Alpha3, "0.##")} " +
                            $"{F(v.Alpha4, "0.##")} " +
                            $"{F(v.Alpha5, "0.##")}= " +
                            $"{F((v.Alpha1 * v.ProductAlpha235 * v.Alpha4), "0.##")}$" +
                        $"| {F(v.Verankeringslengte)} |");
                }
            }
            return sb.ToString();
        }

        public static string ToMarkdown(
            this Dictionary<WapeningGroep, List<VerankeringResult>> verankeringenPerGroep)
        {
            var sb = new StringBuilder();

            sb.AppendLine("| Nr | Vorm | $F_{bt}$ | $a_b$ | $f_{cd}$ | $Ø_{m,prov}$ | $Ø_{{m,min}}$ | $a_1$ | $a_{{1,min}}$ | OK |");
            sb.AppendLine("|----|------|---------:|------:|:--------:|-------------:|--------------:|------:|--------------:|---:|");
            
            foreach (var (groep, verankeringen) in verankeringenPerGroep)
            {
                int nr = 1;
                foreach (var v in verankeringen)
                {
                    sb.AppendLine(
                        $"| {nr++} " +
                        $"| {Escape(v.StaafVorm)} " +
                        $"| {(v.StaafVorm == VerankeringStaafVorm.Gebogen? F(v.Fbt) : "nvt")} " +
                        $"| {F(v.Ab)} " +
                        $"| {F(v.Fcd)}" +
                        $"| {F(v.ToegepasteBuigdoornDiameter)} " +
                        $"| {F(v.MinimaleBuigdoornDiameter)} " +
                        $"| {F(v.AfstandTotFbt)} " +
                        $"| {v.MinimaleRechteLengteTekst}" +
                        $"| {(v.IsOk? "✅": "❌")} |");
                }
            }

            //sb.AppendLine("tabel verankering");
            //sb.AppendLine("$a_{{1,min}}$ is de minimale rechte lengte (vanaf verankering tot aan de ombuiging) bij de toegepaste buigdoorn.");



            return sb.ToString();
        }

        private static string F(double value)
            => value.ToString("0", CultureInfo.InvariantCulture);

        private static string F(double value, string format)
            => value.ToString(format, CultureInfo.InvariantCulture);

        private static string Escape(object? value)
            => value?.ToString()?.Replace("|", "\\|") ?? "";
    }


    
        

        public enum MarkdownAlignment
        {
            Left,
            Center,
            Right
        }

    public static class MarkdownTableExtensions
    {
        public static MarkdownTableBuilder<T> ToMarkdownTable<T>(
            this IEnumerable<T> items)
        {
            return new MarkdownTableBuilder<T>(items);
        }
    }

    public sealed class MarkdownTableBuilder<T>
    {
        private readonly IEnumerable<T> _items;
        private readonly List<MarkdownColumn<T>> _columns = [];

        public MarkdownTableBuilder(IEnumerable<T> items)
        {
            _items = items;
        }

        // Normale kolom
        public MarkdownTableBuilder<T> Column(
            string header,
            Func<T, object?> value,
            string? format = null,
            MarkdownAlignment alignment = MarkdownAlignment.Right)
        {
            _columns.Add(new MarkdownColumn<T>(
                header,
                (item, _) => value(item),
                format,
                alignment));

            return this;
        }

        // Kolom die ook het regelnummer kan gebruiken
        public MarkdownTableBuilder<T> Column(
            string header,
            Func<T, int, object?> value,
            string? format = null,
            MarkdownAlignment alignment = MarkdownAlignment.Right)
        {
            _columns.Add(new MarkdownColumn<T>(
                header,
                value,
                format,
                alignment));

            return this;
        }

        public MarkdownTableBuilder<T> NumberColumn(
            string header = "Nr")
        {
            _columns.Add(new MarkdownColumn<T>(
                header,
                (_, nr) => nr,
                null,
                MarkdownAlignment.Center));

            return this;
        }


        public string Build()
        {
            if (_columns.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();

            // Header
            sb.Append("| ");
            sb.Append(string.Join(" | ", _columns.Select(x => x.Header)));
            sb.AppendLine(" |");

            // Alignment
            sb.Append('|');
            sb.Append(string.Join("|", _columns.Select(x => x.Alignment switch
            {
                MarkdownAlignment.Left => ":---",
                MarkdownAlignment.Center => ":---:",
                MarkdownAlignment.Right => "---:",
                _ => "---"
            })));
            sb.AppendLine("|");

            // Regels
            var nr = 1;

            foreach (var item in _items)
            {
                var values = _columns.Select(column =>
                {
                    var value = column.Value(item, nr);

                    return FormatValue(
                        value,
                        column.Format);
                });

                sb.Append("| ");
                sb.Append(string.Join(" | ", values));
                sb.AppendLine(" |");

                nr++;
            }

            return sb.ToString();
        }

        private static string FormatValue(
            object? value,
            string? format)
        {
            if (value is null)
                return string.Empty;

            string text;

            if (value is IFormattable formattable &&
                !string.IsNullOrWhiteSpace(format))
            {
                text = formattable.ToString(
                    format,
                    CultureInfo.InvariantCulture);
            }
            else
            {
                text = value.ToString() ?? string.Empty;
            }

            return Escape(text);
        }

        private static string Escape(string value)
        {
            return value
                .Replace("|", "\\|")
                .Replace("\r\n", "<br>")
                .Replace("\n", "<br>");
        }
    }




    internal sealed record MarkdownColumn<T>(
        string Header,
        Func<T, int, object?> Value,
        string? Format,
        MarkdownAlignment Alignment);
}

    


