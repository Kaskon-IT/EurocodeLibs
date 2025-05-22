using MigraDoc.DocumentObjectModel;

namespace ExportFactory.MigraDocContentModels
{
    /// <summary>
    /// Universele class voor Paragraph, Chart, Image, Table of TextFrame van MigraDoc rechtstreeks te plaatsen in de document bouwstenen.
    /// </summary>
    public class MigraDocElement : SectionElement
    {
        public MigraDoc.DocumentObjectModel.DocumentObject? DocumentObject { get; set; }

        public MigraDocElement()
        {

        }

        public MigraDocElement(DocumentObject documentObject)
        {
            DocumentObject = documentObject;
        }

    }



}
