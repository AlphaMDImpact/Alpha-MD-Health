using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public partial class ReadingDatabase : BaseDatabase
    {
        /// <summary>
        /// Gets metadata of all reading types if ReadingType is None else returns metadata of given ReadingType
        /// </summary>
        /// <param name="readingsData">Reference object to return metadata. If IsActive is true then only Dashboard enabled data will be fetched</param>
        /// <returns>List of metadata</returns>
        public async Task GetReadingRangesAsync(PatientReadingDTO readingsData, string readingIDs)
        {
            readingsData.ReadingRanges = await SqlConnection.QueryAsync<ReadingRangeModel>(
                "SELECT DISTINCT RR.ReadingID, RR.AbsoluteMinValue, RR.AbsoluteMaxValue, RR.AbsoluteBandColor, RR.NormalMinValue, " +
                "RR.NormalMaxValue, RR.NormalBandColor, RR.TargetBandColor, RR.ForGender, RR.ForAgeGroup, RR.FromAge, RR.ToAge, " +
                "(CASE WHEN RR.IsActive = 1 AND P.IsActive = 1 AND PP.IsActive = 1 THEN 1 ELSE 0 END) AS IsActive " + //B.Dob, B.GenderID AS GenderString, 
                "FROM ReadingRangeModel RR " +
                "JOIN ProgramModel P ON P.ProgramID = RR.ProgramID " +
                "JOIN PatientProgramModel PP ON PP.ProgramID = P.ProgramID AND PP.PatientID = ? " +
                $"WHERE RR.ReadingID IN ({readingIDs})"
                , readingsData.SelectedUserID
            );
        }

        private void SaveReadingRanges(PatientReadingDTO readingData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(readingData.ReadingRanges))
            {
                foreach (var range in readingData.ReadingRanges.OrderBy(x => x.IsActive))
                {
                    string saveQuery;
                    if (transaction.FindWithQuery<ReadingRangeModel>("SELECT 1 FROM ReadingRangeModel " +
                        "WHERE ProgramID=? AND ReadingID=? AND ProgramReadingID=? AND ReadingRangeID= ?",
                        range.ProgramID, range.ReadingID, range.ProgramReadingID, range.ReadingRangeID) == null)
                    {
                        saveQuery = "INSERT INTO ReadingRangeModel(IsActive, ForGender, ForAgeGroup, FromAge, ToAge, AbsoluteMinValue, " +
                            "AbsoluteMaxValue, AbsoluteBandColor, NormalMinValue, NormalMaxValue, NormalBandColor, TargetBandColor, " +
                            "ProgramID, ReadingID, ProgramReadingID, ReadingRangeID) " +
                            "VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?) ";
                    }
                    else
                    {
                        saveQuery = "UPDATE ReadingRangeModel SET IsActive=?, ForGender=?, ForAgeGroup=?, FromAge=?, ToAge=?, AbsoluteMinValue=?, " +
                            "AbsoluteMaxValue=?, AbsoluteBandColor=?, NormalMinValue=?, NormalMaxValue=?, NormalBandColor=?, TargetBandColor=? " +
                            "WHERE ProgramID=? AND ReadingID=? AND ProgramReadingID=? AND ReadingRangeID= ?";
                    }
                    transaction.Execute(saveQuery, range.IsActive, range.ForGender, range.ForAgeGroup, range.FromAge, range.ToAge, range.AbsoluteMinValue,
                        range.AbsoluteMaxValue, range.AbsoluteBandColor, range.NormalMinValue, range.NormalMaxValue, range.NormalBandColor, range.TargetBandColor,
                        range.ProgramID, range.ReadingID, range.ProgramReadingID, range.ReadingRangeID);
                }
            }
        }
    }
}
