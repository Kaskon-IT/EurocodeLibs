using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLibrary.Models
{
    /// <summary>
    /// Eén genummerde stap van een rekenvoorbeeld. Kan buiten de
    /// <see cref="RekenvoorbeeldBuilder"/> worden opgebouwd (bijv. conditioneel,
    /// afhankelijk van het verloop van de berekening) en daarna worden aangeleverd
    /// via <see cref="RekenvoorbeeldBuilder.Stappen"/> of de constructor-overload.
    /// </summary>
    /// <param name="Key">Unieke sleutel om de stap te kunnen overslaan/vervangen.</param>
    /// <param name="Titel">Titel achter het stapnummer.</param>
    /// <param name="Inhoud">Markdown-inhoud (mag LaTeX bevatten).</param>
    /// <param name="Overslaan">Indien true wordt de stap niet gerenderd.</param>
    public sealed record RekenvoorbeeldStap(string Key, string Titel, string Inhoud, bool Overslaan = false);


    /// <summary>
    /// Schrijver voor de inhoud van één stap, voor gebruik met de
    /// lambda-overload van <see cref="RekenvoorbeeldBuilder.Stap(string, string, Action{StapSchrijver})"/>.
    /// Verzamelt regels en formules en voegt ze samen tot markdown.
    /// </summary>
    public sealed class StapSchrijver
    {
        private readonly StringBuilder _sb = new();

        /// <summary>Voegt een markdown-regel toe (mag inline LaTeX bevatten), gevolgd door een lege regel.</summary>
        public StapSchrijver Regel(string markdown)
        {
            _sb.AppendLine(markdown);
            _sb.AppendLine();
            return this;
        }

        /// <summary>Voegt een display-formule toe, verpakt in $$ … $$.</summary>
        public StapSchrijver Formule(string latex)
        {
            _sb.AppendLine("$$");
            _sb.AppendLine(latex);
            _sb.AppendLine("$$");
            _sb.AppendLine();
            return this;
        }

        /// <summary>Voegt de regel alleen toe als aan de voorwaarde is voldaan.</summary>
        public StapSchrijver RegelAls(bool voorwaarde, string markdown) =>
            voorwaarde ? Regel(markdown) : this;

        /// <summary>Voegt een geslaagd/gefaald-regel toe (✔️/⚠️).</summary>
        public StapSchrijver Toets(bool voldoet, string omschrijvingVoldoet, string omschrijvingFaalt) =>
            Regel(voldoet ? $"✔️ {omschrijvingVoldoet}" : $"⚠️ {omschrijvingFaalt}");

        internal string BuildInhoud() => _sb.ToString().TrimEnd();
    }



    /// <summary>
    /// Generieke builder voor markdown-rekenvoorbeelden (met LaTeX-formules).
    /// <para>
    /// Stappen worden op volgorde toegevoegd met een unieke sleutel; de stapnummers
    /// lopen automatisch op bij het genereren. Stappen kunnen achteraf worden
    /// overgeslagen (<see cref="Skip"/>), vervangen (<see cref="Replace"/>) of
    /// ingevoegd (<see cref="InsertAfter"/>), zonder dat de nummering handmatig
    /// bijgewerkt hoeft te worden.
    /// </para>
    /// <para>
    /// De builder is model-onafhankelijk en daarmee bruikbaar voor alle
    /// Input/Result-combinaties (BaseInput-afgeleiden e.d.).
    /// </para>
    /// </summary>
    public sealed class RekenvoorbeeldBuilder
    {
        private sealed class StapCtx
        {
            public required string Key { get; init; }
            public required string Titel { get; set; }
            public required string Inhoud { get; set; }
            public bool Overslaan { get; set; }
        }

        private readonly string _titel;
        private string? _intro;   // bijv. uitgangspunten-tabel (zonder stapnummer)
        private string? _slot;    // bijv. samenvatting (zonder stapnummer)
        private readonly List<StapCtx> _stappen = new();

        public RekenvoorbeeldBuilder(string titel) => _titel = titel;

        /// <summary>
        /// Maakt een builder met extern opgebouwde intro, stappen en slot.
        /// Handig wanneer de opzet dynamisch is (bijv. berekening die anders
        /// verloopt of vroegtijdig wordt afgebroken): stel de lijst zelf samen
        /// en geef hem in één keer door. Skip/Replace/InsertAfter blijven daarna
        /// gewoon bruikbaar.
        /// </summary>
        public RekenvoorbeeldBuilder(string titel, IEnumerable<RekenvoorbeeldStap> stappen,
            string? intro = null, string? slot = null)
            : this(titel)
        {
            _intro = intro;
            _slot = slot;
            Stappen(stappen);
        }

        /// <summary>Voegt meerdere extern opgebouwde stappen toe (op volgorde).</summary>
        public RekenvoorbeeldBuilder Stappen(IEnumerable<RekenvoorbeeldStap> stappen)
        {
            foreach (var s in stappen)
            {
                Stap(s.Key, s.Titel, s.Inhoud);
                if (s.Overslaan) Skip(s.Key);
            }
            return this;
        }

        /// <summary>Blok vóór de stappen, zonder nummering (bijv. uitgangspunten).</summary>
        public RekenvoorbeeldBuilder Intro(string markdown)
        {
            _intro = markdown;
            return this;
        }

        /// <summary>Voegt een genummerde stap toe. De sleutel moet uniek zijn.</summary>
        public RekenvoorbeeldBuilder Stap(string key, string titel, string inhoud)
        {
            if (_stappen.Any(s => s.Key == key))
                throw new ArgumentException($"Stap met sleutel '{key}' bestaat al.", nameof(key));

            _stappen.Add(new StapCtx { Key = key, Titel = titel, Inhoud = inhoud });
            return this;
        }

        /// <summary>
        /// Voegt een genummerde stap toe waarvan de inhoud via een
        /// <see cref="StapSchrijver"/>-lambda wordt opgebouwd.
        /// </summary>
        public RekenvoorbeeldBuilder Stap(string key, string titel, Action<StapSchrijver> inhoud)
        {
            var s = new StapSchrijver();
            inhoud(s);
            return Stap(key, titel, s.BuildInhoud());
        }

        /// <summary>Voegt een nieuwe stap in direct na een bestaande stap (lambda-variant).</summary>
        public RekenvoorbeeldBuilder InsertAfter(string afterKey, string key, string titel, Action<StapSchrijver> inhoud)
        {
            var s = new StapSchrijver();
            inhoud(s);
            return InsertAfter(afterKey, key, titel, s.BuildInhoud());
        }


        /// <summary>Slaat een stap over; volgende stappen schuiven automatisch op.</summary>
        public RekenvoorbeeldBuilder Skip(string key)
        {
            Vind(key).Overslaan = true;
            return this;
        }

        /// <summary>Vervangt titel en/of inhoud van een bestaande stap.</summary>
        public RekenvoorbeeldBuilder Replace(string key, string? titel = null, string? inhoud = null)
        {
            var stap = Vind(key);
            if (titel is not null) stap.Titel = titel;
            if (inhoud is not null) stap.Inhoud = inhoud;
            return this;
        }

        /// <summary>Voegt een nieuwe stap in direct na een bestaande stap.</summary>
        public RekenvoorbeeldBuilder InsertAfter(string afterKey, string key, string titel, string inhoud)
        {
            var index = _stappen.FindIndex(s => s.Key == afterKey);
            if (index < 0)
                throw new ArgumentException($"Stap met sleutel '{afterKey}' niet gevonden.", nameof(afterKey));

            _stappen.Insert(index + 1, new StapCtx { Key = key, Titel = titel, Inhoud = inhoud });
            return this;
        }

        /// <summary>Blok ná de stappen, zonder nummering (bijv. samenvatting).</summary>
        public RekenvoorbeeldBuilder Slot(string markdown)
        {
            _slot = markdown;
            return this;
        }

        /// <summary>Genereert de markdown; stapnummers lopen automatisch op.</summary>
        public string Build()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {_titel}");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(_intro))
            {
                sb.AppendLine(_intro.Trim());
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();
            }

            var nr = 0;
            foreach (var stap in _stappen.Where(s => !s.Overslaan))
            {
                nr++;
                sb.AppendLine($"## Stap {nr} -- {stap.Titel}");
                sb.AppendLine();
                sb.AppendLine(stap.Inhoud.Trim());
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(_slot))
            {
                sb.AppendLine(_slot.Trim());
            }

            return sb.ToString();
        }

        private StapCtx Vind(string key) =>
            _stappen.FirstOrDefault(s => s.Key == key)
            ?? throw new ArgumentException($"Stap met sleutel '{key}' niet gevonden.", nameof(key));
    }
}
