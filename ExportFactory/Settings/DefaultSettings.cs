using MigraDoc.DocumentObjectModel;

namespace ExportFactory.Settings
{
    public class DefaultSettings
    {
        public Font DefaultFont { get; set; } = new Font("Comic Sans", Unit.FromPoint(9.00));
        public MigraDoc.DocumentObjectModel.Color DefaultColor { get; set; } = new Color(255, 40, 50, 60);

    }
}
