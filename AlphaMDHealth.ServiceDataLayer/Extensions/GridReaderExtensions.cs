using Dapper;
using System.Data.SqlClient;

namespace AlphaMDHealth.ServiceDataLayer
{
    /// <summary>
    /// Grid reader extension
    /// </summary>
    public static class GridReaderExtensions
    {
        /// <summary>
        /// Checks if the grid reader has rows
        /// </summary>
        /// <param name="gridReader">Instance of grid reader</param>
        /// <returns>true if grid reader has rows else returns false</returns>
        public static bool HasRows(this SqlMapper.GridReader gridReader)
        {
            SqlDataReader sqlDataReader = ((SqlDataReader)typeof(SqlMapper.GridReader)
                     .GetField("reader", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     .GetValue(gridReader));
            return sqlDataReader.HasRows || sqlDataReader.FieldCount > 0;
        }
    }
}
