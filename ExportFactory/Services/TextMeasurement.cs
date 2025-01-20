using MigraDoc.DocumentObjectModel;
using PdfSharp.Drawing;
using System.ComponentModel;
using System.Diagnostics;

namespace ExportFactory.Services
{
    /// <summary>
    /// Provides functionality to measure the width of text during document design time.
    /// </summary>
    public sealed class TextMeasurement
    {
        Font _font;
        XFont? _gdiFont;
        XGraphics? _graphics;



        /// <summary>
        /// Initializes a new instance of the TextMeasurement class with the specified font.
        /// </summary>
        public TextMeasurement(Font font)
        {
            ArgumentNullException.ThrowIfNull(font);

            _font = font;
        }

        /// <summary>
        /// Returns the size of the bounding box of the specified text.
        /// </summary>
        public XSize MeasureString(string text, UnitType unitType)
        {
            ArgumentNullException.ThrowIfNull(text);

            if (!Enum.IsDefined(unitType))
                throw new InvalidEnumArgumentException();

            XGraphics graphics = Realize();

            XSize size = graphics.MeasureString(text, _gdiFont/*, new XPoint(0, 0), StringFormat.GenericTypographic*/);
            switch (unitType)
            {
                case UnitType.Point:
                    break;

                case UnitType.Centimeter:
                    size.Width = (float)(size.Width * 2.54 / 72);
                    size.Height = (float)(size.Height * 2.54 / 72);
                    break;

                case UnitType.Inch:
                    size.Width = size.Width / 72;
                    size.Height = size.Height / 72;
                    break;

                case UnitType.Millimeter:
                    size.Width = (float)(size.Width * 25.4 / 72);
                    size.Height = (float)(size.Height * 25.4 / 72);
                    break;

                case UnitType.Pica:
                    size.Width = size.Width / 12;
                    size.Height = size.Height / 12;
                    break;

                default:
                    Debug.Assert(false, "Missing unit type");
                    break;
            }
            return size;
        }

        /// <summary>
        /// Returns the size of the bounding box of the specified text in point.
        /// </summary>
        public XSize MeasureString(string text)
        {
            return MeasureString(text, UnitType.Point);
        }

        /// <summary>
        /// Gets or sets the font used for measurement.
        /// </summary>
        public Font Font
        {
            get { return _font; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                if (_font != value)
                {
                    _font = value;
                    _gdiFont = null;
                }
            }
        }


        /// <summary>
        /// Initializes appropriate GDI+ objects.
        /// </summary>
        XGraphics Realize()
        {
            _graphics ??= XGraphics.CreateMeasureContext(new XSize(2000, 2000), XGraphicsUnit.Point, XPageDirection.Downwards);

            //this.graphics.PageUnit = GraphicsUnit.Point;

            if (_gdiFont == null)
            {
                XFontStyle style = XFontStyle.Regular;
                if (_font.Bold)
                    style |= XFontStyle.Bold;
                if (_font.Italic)
                    style |= XFontStyle.Italic;

                _gdiFont = new XFont(_font.Name, _font.Size, style);
            }
            return _graphics;
        }


    }
}
