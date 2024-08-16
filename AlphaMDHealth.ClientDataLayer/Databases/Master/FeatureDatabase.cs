using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientDataLayer
{
    public class FeatureDatabase : BaseDatabase
    {
        /// <summary>
        /// insert or update record in the database
        /// </summary>
        /// <param name="featureData">Object with modified resources and refence for operation status</param>
        /// <returns>Operation Status</returns>
        public async Task SaveFeaturesAsync(BaseDTO featureData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (featureData.LastModifiedON == null)
                {
                    //When Last sync datetime is default datetime, clear all existing records from DB and insert new one for avoid duplicate record issue as each server will contains different ServerID(PrimaryKey) and InsertOrReplace() function update records based on PrimaryKey
                    transaction.Execute("DELETE FROM OrganizationFeaturePermissionModel");
                }
                if (GenericMethods.IsListNotEmpty(featureData.FeaturePermissions))
                {
                    foreach (OrganizationFeaturePermissionModel feature in featureData.FeaturePermissions)
                    {
                        if (feature.IsActive)
                        {
                            transaction.InsertOrReplace(feature);
                        }
                        else
                        {
                            transaction.Execute($"DELETE FROM OrganizationFeaturePermissionModel WHERE FeatureID = ?", feature.FeatureID);
                        }
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// insert or update record in the database
        /// </summary>
        /// <param name="featureData">Object with modified resources and refence for operation status</param>
        /// <returns>Operation Status</returns>
        public async Task SaveFeatureRelationsAsync(FeatureRelationDTO featureData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (var feature in featureData.FeatureRelations)
                {
                    if (transaction.FindWithQuery<FeatureRelationModel>("SELECT 1 FROM FeatureRelationModel WHERE FeatureGroupID= ? AND FeatureID=? ", feature.FeatureGroupID, feature.FeatureID) == null)
                    {
                        transaction.Execute("INSERT INTO FeatureRelationModel (FeatureGroupID, FeatureID, SequenceNo, IsActive) VALUES (?, ?, ?, ?)",
                            feature.FeatureGroupID, feature.FeatureID, feature.SequenceNo, feature.IsActive);
                    }
                    else
                    {
                        transaction.Execute("UPDATE FeatureRelationModel SET SequenceNo = ?, IsActive = ? WHERE FeatureGroupID = ? AND FeatureID = ? ",
                            feature.SequenceNo, feature.IsActive, feature.FeatureGroupID, feature.FeatureID);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves features from database for given group list
        /// </summary>
        /// <param name="featureData">Object to hold feature data with operation status</param>
        /// <param name="groupList">List of groups for which feature has to be retrieved</param>
        /// <returns>List of all features by given grouplist</returns>
        public async Task GetFeaturesAsync(BaseDTO featureData, List<string> groupList)
        {
            featureData.FeaturePermissions = await SqlConnection.QueryAsync<OrganizationFeaturePermissionModel>(
                $"SELECT * FROM OrganizationFeaturePermissionModel WHERE FeatureCode IN ('{string.Join("','", groupList)}')").ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves tab features from database for given feature id
        /// </summary>
        /// <param name="featureData">Object to hold feature data with operation status</param>
        /// <param name="featureCode">Feature code for which feature has to be retrieved</param>
        /// <returns>List of all features by given feature id</returns>
        public async Task GetTabFeaturesAsync(UserDTO featureData, string featureCode)
        {
            featureData.FeaturePermissions = await SqlConnection.QueryAsync<OrganizationFeaturePermissionModel>(
                $"SELECT OFP.* FROM FeatureRelationModel FR LEFT JOIN OrganizationFeaturePermissionModel OFP ON FR.FeatureID = OFP.FeatureID" +
                $" WHERE FR.IsActive = 1 AND FR.FeatureGroupID = (SELECT FeatureID FROM OrganizationFeaturePermissionModel WHERE FeatureCode = ?) ORDER BY FR.SequenceNo",
                featureCode).ConfigureAwait(false);
        }
    }
}
