using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class InstructionService : BaseService
    {
        public InstructionService(IEssentials essentials) : base(essentials)
        {
            
        }
        /// <summary>
        /// Map and Save Instructions From Server
        /// </summary>
        /// <param name="result">Data Sync Result</param>
        /// <param name="data">Jtoken Data From Sync call</param>
        /// <returns>Operation Status and No of records saved</returns>
        internal async Task MapAndSaveInstructionsAsync(DataSyncModel result, JToken data)
        {
            try
            {
                ProgramDTO instructionData = new ProgramDTO()
                {
                    Instructions = MapInstructions(data, nameof(DataSyncDTO.Instructions))
                };
                if (GenericMethods.IsListNotEmpty(instructionData.Instructions))
                {
                    await new InstructionDatabase().SaveInstructionsAsync(instructionData).ConfigureAwait(false);
                }
                result.RecordCount = instructionData.Instructions?.Count ?? 0;
            }
            catch (Exception ex)
            {
                result.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Map and Save Instructions Data Based On Language From Server
        /// </summary>
        /// <param name="result">Data Sync Result</param>
        /// <param name="data">Jtoken Data From Sync call</param>
        /// <returns>Operation Status and No of records saved</returns>
        internal async Task MapAndSaveInstructionI18NAsync(DataSyncModel result, JToken data)
        {
            try
            {
                ProgramDTO instructionData = new ProgramDTO()
                {
                    InstructionsI18N = MapInstructionI18N(data, nameof(DataSyncDTO.InstructionI18N))
                };

                if (GenericMethods.IsListNotEmpty(instructionData.InstructionsI18N))
                {
                    await new InstructionDatabase().SaveInstructionI18NAsync(instructionData).ConfigureAwait(false);
                }
                result.RecordCount = instructionData.InstructionsI18N?.Count ?? 0;
            }
            catch (Exception ex)
            {
                result.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Gets Instruction(s) and task status
        /// </summary>
        /// <param name="instructionData">Object to receive transfer filter conditions and return instruction data</param>
        /// <returns>Instruction Data with Operation Status</returns>
        public async Task GetInstructionAsync(ProgramDTO instructionData)
        {
            try
            {
                instructionData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
                await Task.WhenAll(GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_TASK_GROUP),
                                 new InstructionDatabase().GetInstructionAsync(instructionData)).ConfigureAwait(false);
                GetDataForWebView(instructionData);
            }
            catch (Exception ex)
            {
                instructionData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void GetDataForWebView(ProgramDTO instructionData)
        {
            if (instructionData.InstructionI18N != null)
            {
                ResourceModel webViewResource = new ResourceModel
                {
                    ResourceKey = instructionData.InstructionI18N.InstructionID.ToString(),
                    InfoValue = instructionData.InstructionI18N.Description.ToString(),
                };
                PageData.Resources.Add(webViewResource);
            }
        }

        private List<InstructionModel> MapInstructions(JToken data, string nameOfToken)
        {
            return data[nameOfToken].Any() ?
                    (from dataItem in data[nameOfToken]
                     select new InstructionModel
                     {
                         InstructionID = GetDataItem<long>(dataItem, nameof(InstructionModel.InstructionID)),
                         IsActive = GetDataItem<bool>(dataItem, nameof(InstructionModel.IsActive)),
                     }).ToList() : new List<InstructionModel>();
        }

        private List<InstructionI18NModel> MapInstructionI18N(JToken data, string nameOfToken)
        {
            return data[nameOfToken].Any() ?
                    (from dataItem in data[nameOfToken]
                     select new InstructionI18NModel
                     {
                         InstructionID = GetDataItem<long>(dataItem, nameof(InstructionI18NModel.InstructionID)),
                         LanguageID = GetDataItem<byte>(dataItem, nameof(InstructionI18NModel.LanguageID)),
                         Name = GetDataItem<string>(dataItem, nameof(InstructionI18NModel.Name)),
                         Description = GetDataItem<string>(dataItem, nameof(InstructionI18NModel.Description)),
                         IsActive = GetDataItem<bool>(dataItem, nameof(InstructionI18NModel.IsActive)),
                     }).ToList() : new List<InstructionI18NModel>();
        }
    }
}
