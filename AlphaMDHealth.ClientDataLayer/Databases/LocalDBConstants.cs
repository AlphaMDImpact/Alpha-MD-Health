namespace AlphaMDHealth.ClientDataLayer
{
    internal class LocalDBConstants
    {
        private const string SQLITE_DB_NAME = "app.db2";

        internal const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            //SQLite.SQLiteOpenFlags.SharedCache |
            SQLite.SQLiteOpenFlags.FullMutex;

        internal static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, SQLITE_DB_NAME);
    }
}
