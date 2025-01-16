using MigraDoc.DocumentObjectModel.Tables;

namespace ExportFactory.Services
{
    public class RtfFileCreator
    {

    }


    public class MigraDocCreator
    {
        public MigraDoc.DocumentObjectModel.Tables.Table CreateTable()
        {
            Table table = new();
            return table;
        }
    }
}
