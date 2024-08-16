using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer;

public class AppIntroServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Get App intro from database
    /// </summary>
    /// <param name="appIntroData">object to hold app intro data</param>
    /// <returns>App intro data and operation status</returns>
    public async Task GetAppIntrosAsync(AppIntroDTO appIntroData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), appIntroData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AppIntroModel.IntroSlideID), appIntroData.AppIntro.IntroSlideID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.RecordCount), appIntroData.RecordCount, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.LanguageID)), appIntroData.LanguageID, DbType.Byte, direction: ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), appIntroData.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
        MapCommonSPParameters(appIntroData, parameter
            , appIntroData.RecordCount == -3 ? AppPermissions.UserWelcomeScreensView.ToString() : AppPermissions.WelcomeScreensView.ToString()
            , appIntroData.RecordCount == -3 ? "" : $"{AppPermissions.WelcomeScreenDelete},{AppPermissions.WelcomeScreenAddEdit}"
        );
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_ORGANISATION_APP_INTRO_SLIDES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            if (appIntroData.RecordCount == -1)
            {
                appIntroData.AppIntro = (await result.ReadAsync<AppIntroModel>().ConfigureAwait(false))?.FirstOrDefault();
            }
            appIntroData.AppIntros = (await result.ReadAsync<AppIntroModel>().ConfigureAwait(false))?.ToList();
            await MapReturnPermissionsAsync(appIntroData, result).ConfigureAwait(false);
        }
        appIntroData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
    }

    /// <summary>
    /// Save AppIntros data in database
    /// </summary>
    /// <param name="appIntroData">object to save data</param>
    /// <returns>Operation status</returns>
    public async Task SaveAppIntroAsync(AppIntroDTO appIntroData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), appIntroData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AppIntroModel.IntroSlideID), appIntroData.AppIntro.IntroSlideID, DbType.Int64, ParameterDirection.InputOutput);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AppIntroModel.SequenceNo), appIntroData.AppIntro.SequenceNo, DbType.Int32, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AppIntroModel.ImageName), appIntroData.AppIntro.ImageName, DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, MapAppIntrosToTable(appIntroData.AppIntros).AsTableValuedParameter());
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AppIntroModel.IsActive), appIntroData.AppIntro.IsActive, DbType.Boolean, ParameterDirection.Input);
        MapCommonSPParameters(appIntroData, parameter, string.Empty);
        await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_ORGANISATION_APP_INTRO_SLIDES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        appIntroData.AppIntro.IntroSlideID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(appIntroData.AppIntro.IntroSlideID));
        appIntroData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
    }

    private DataTable MapAppIntrosToTable(List<AppIntroModel> appIntros)
    {
        DataTable dataTable = CreateGenericTypeTable();
        if (GenericMethods.IsListNotEmpty(appIntros))
        {
            foreach (AppIntroModel record in appIntros)
            {
                dataTable.Rows.Add(record.IntroSlideID, Guid.Empty, record.LanguageID, record.HeaderText, record.SubHeaderText, string.Empty);
            }
        }
        return dataTable;
    }
}
