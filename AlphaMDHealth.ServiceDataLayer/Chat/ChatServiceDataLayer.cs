using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class ChatServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get Chats and chat details 
        /// </summary>
        /// <param name="chatData">Reference object of chat data to get</param>
        /// <returns>List of chats and and chat details</returns>
        public async Task GetChatsAsync(ChatDTO chatData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), chatData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ChatModel.ToID), chatData.Chat.ToID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ChatModel.ChatID), chatData.Chat.ChatID, DbType.Guid, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), default(DateTimeOffset), DbType.DateTimeOffset, ParameterDirection.Input);
            if (chatData.RecordCount == -1)
            {
                // AddNewChat and Chat Detail Page
                MapCommonSPParameters(chatData, parameters, GetPermissionsToCheck(chatData), chatData.Chat.ToID > 0
                    ? $"{AppPermissions.ChatDelete},{AppPermissions.ChatAddEdit}"
                    : AppPermissions.ChatView.ToString()
                );
            }
            else
            {
                MapCommonSPParameters(chatData, parameters, AppPermissions.ChatsView.ToString(), $"{AppPermissions.ChatView},{AppPermissions.NewChatView}");
            }
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_CHATS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                await MapChatsDataAsync(chatData, result).ConfigureAwait(false);
            }
            chatData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        private string GetPermissionsToCheck(ChatDTO chatData)
        {
            if (chatData.Chat.ToID != -1)
            {
                return chatData.Chat.ToID > 0 ? AppPermissions.ChatView.ToString() : string.Empty;
            }
            return AppPermissions.NewChatView.ToString();
        }

        /// <summary>
        /// Save Chat to database
        /// </summary>
        /// <param name="chatData">Chats data to be saved</param>
        /// <returns>Result of operation</returns>
        public async Task SaveChatAsync(ChatDTO chatData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), chatData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), MapChatsToTable(chatData.Chats).AsTableValuedParameter());
            parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS_2), MapChatDetailsToTable(chatData.ChatDetails).AsTableValuedParameter());
            parameter.Add(ConcateAt(nameof(UserDTO.SelectedUserID)), chatData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(chatData, parameter, CheckPermission(chatData.ChatDetails));
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_CHATS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                chatData.SaveChats = (await result.ReadAsync<SaveResultModel>().ConfigureAwait(false))?.ToList();
                if (!result.IsConsumed)
                {
                    chatData.SaveChatDetails = (await result.ReadAsync<SaveResultModel>().ConfigureAwait(false))?.ToList();
                }
            }
            chatData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        private async Task MapChatsDataAsync(ChatDTO chatData, SqlMapper.GridReader result)
        {
            chatData.Chats = (await result.ReadAsync<ChatModel>().ConfigureAwait(false))?.ToList();
            if (chatData.RecordCount == -1 && chatData.Chat.ToID > 0)
            {
                chatData.ChatDetails = (await result.ReadAsync<ChatDetailModel>().ConfigureAwait(false))?.ToList();
            }
            await MapReturnPermissionsAsync(chatData, result).ConfigureAwait(false);
        }

        private string CheckPermission(List<ChatDetailModel> chatDetails)
        {
            var editCheck = chatDetails.Any(x => x.IsActive);
            var deleteCheck = chatDetails.Any(x => !x.IsActive);
            if (editCheck && deleteCheck)
            {
                return $"{AppPermissions.ChatAddEdit},{AppPermissions.ChatDelete}";
            }
            else if (editCheck)
            {
                return AppPermissions.ChatAddEdit.ToString();
            }
            else if (deleteCheck)
            {
                return AppPermissions.ChatDelete.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        private DataTable MapChatsToTable(List<ChatModel> chats)
        {
            DataTable dataTable = CreateChatTable();
            short count = 1;
            foreach (ChatModel record in chats)
            {
                dataTable.Rows.Add(record.ChatID, record.FromID, record.ToID, record.AddedOn, count++);
            }
            return dataTable;
        }

        private DataTable MapChatDetailsToTable(List<ChatDetailModel> chatDetails)
        {
            DataTable dataTable = CreateChatDetailsTable();
            short count = 1;
            foreach (ChatDetailModel record in chatDetails)
            {
                dataTable.Rows.Add(
                    record.ChatID, record.ChatDetailID, record.ChatText, record.FileName,
                    string.IsNullOrWhiteSpace(record.FileName) ? AppFileExtensions.none.ToString() : record.FileType.ToString(),
                    record.IsActive, record.AddedOn, record.FromID, record.IsDataDownloaded, count++);
            }
            return dataTable;
        }

        private DataTable CreateChatTable()
        {
            return new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(nameof(ChatDetailModel.ChatID), typeof(Guid)),
                    new DataColumn(nameof(ChatDetailModel.FromID), typeof(long)),
                    new DataColumn(nameof(ChatDetailModel.ToID), typeof(long)),
                    new DataColumn(nameof(ChatDetailModel.AddedOn), typeof(DateTimeOffset)),
                    new DataColumn(SPFieldConstants.FIELD_TEMP_ID, typeof(short)),
                }
            };
        }

        private DataTable CreateChatDetailsTable()
        {
            return new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(nameof(ChatDetailModel.ChatID), typeof(Guid)),
                    new DataColumn(nameof(ChatDetailModel.ChatDetailID), typeof(Guid)),
                    new DataColumn(nameof(ChatDetailModel.ChatText), typeof(string)),
                    new DataColumn(nameof(ChatDetailModel.FileName), typeof(string)),
                    new DataColumn(nameof(ChatDetailModel.FileType), typeof(string)),
                    new DataColumn(nameof(ChatDetailModel.IsActive), typeof(bool)),
                    new DataColumn(nameof(ChatDetailModel.AddedOn), typeof(DateTimeOffset)),
                    new DataColumn(nameof(ChatDetailModel.FromID), typeof(long)),
                    new DataColumn(nameof(ChatDetailModel.IsDataDownloaded), typeof(bool)),
                    new DataColumn(SPFieldConstants.FIELD_TEMP_ID, typeof(short)),
                }
            };
        }
    }
}