using System.Data.SqlTypes;
using System.IO;

namespace SQLiteRepository
{
    /// <summary>	Connection options for a sqlite database. </summary>
    public class SQLiteDbOptions
    {
        public string DatabaseFilename { get; set; }

        public string DatabaseLocation { get; set; }

        public SQLite.SQLiteOpenFlags Flags { get; set; } =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;
    }
}
