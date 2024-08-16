using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class SubscalesScorePage : BasePage
{
    private readonly QuestionnaireDTO _questionnaireData = new QuestionnaireDTO { QuestionnaireSubscaleRange = new QuestionnaireSubscaleRangesModel(), RecordCount = -1 };
    private IList<TabDataStructureModel> _dataFormatter;
    private List<ButtonActionModel> _actionData;
    private bool _hideConfirmationPopup = true;
    private double? _minValue;
    private double? _maxValue;
    private bool _isEditable;
    private bool _showAddEditPage;

    /// <summary>
    /// Id of the questionnaire
    /// </summary>
    [Parameter]
    public long QuestionnaireID { get; set; }

    /// <summary>
    /// Subscale Range ID parameter
    /// </summary>
    [Parameter]
    public long SubscaleRangeID { get; set; }

    /// <summary>
    /// QuestionnaireData paramter
    /// </summary>
    [Parameter]
    public QuestionnaireDTO QuestionnaireData { get; set; }

    /// <summary>
    /// QuestionnaireSubscaleRanges ID parameter
    /// </summary>
    [Parameter]
    public List<QuestionnaireSubscaleRangesModel> QuestionnaireSubscaleRanges { get; set; }
    protected override async Task OnInitializedAsync()
    {
        await GetDatAsync();
        await base.OnInitializedAsync();
    }

    private async Task GetDatAsync()
    {
        _questionnaireData.Questionnaire = new QuestionnaireModel { QuestionnaireID = QuestionnaireID };
        _questionnaireData.QuestionnaireSubscaleRange.SubScaleRangeID = SubscaleRangeID;
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionnaireSubScaleFromServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
        if (_questionnaireData.ErrCode == ErrorCode.OK)
        {
            _dataFormatter = SetDataFormatter();
            _questionnaireData.Questionnaire = new QuestionnaireModel { QuestionnaireID = QuestionnaireID };
            if (SubscaleRangeID != 0)
            {
                _minValue = Math.Round(_questionnaireData.QuestionnaireSubscaleRange.MinValue, 2);
                _maxValue = Math.Round(_questionnaireData.QuestionnaireSubscaleRange.MaxValue, 2);
            }
        }
        else
        {
            await OnClose.InvokeAsync(_questionnaireData.ErrCode.ToString());
        }
        _isEditable = LibPermissions.HasPermission(_questionnaireData.FeaturePermissions, AppPermissions.SubscaleDelete.ToString());
        _isDataFetched = true;
    }

    private List<TabDataStructureModel> SetDataFormatter()
    {
        return new List<TabDataStructureModel>
        {
            new TabDataStructureModel{DataField=nameof(ContentDetailModel.PageHeading),  ResourceKey=ResourceConstants.R_RECOMMENDATION_TEXT_KEY, IsRequired = false},
            new TabDataStructureModel{DataField=nameof(ContentDetailModel.PageData), Height=Constants.SMALL_TEXT_BOX_HEIGHT, ResourceKey=ResourceConstants.R_DESCRIPTION_TEXT_KEY, IsRequired= false},
        };
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private async Task OnSaveClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsDataValid())
        {
            _questionnaireData.IsActive = true;
            await SaveSubscaleRangeAsync();
        }
    }

    private bool IsDataValid()
    {
        if (IsValid())
        {
            _questionnaireData.QuestionnaireSubscaleRange.MinValue = Convert.ToSingle(_minValue);
            _questionnaireData.QuestionnaireSubscaleRange.MaxValue = Convert.ToSingle(_maxValue);
            if (_questionnaireData.QuestionnaireSubscaleRange.MinValue > _questionnaireData.QuestionnaireSubscaleRange.MaxValue)
            {
                Error = string.Format(CultureInfo.InvariantCulture, LibResources.GetResourceValueByKey(_questionnaireData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY)
                    , LibResources.GetResourceValueByKey(_questionnaireData.Resources, ResourceConstants.R_MIN_TEXT_KEY),
                    LibResources.GetResourceValueByKey(_questionnaireData.Resources, ResourceConstants.R_MAX_TEXT_KEY));
                return false;
            }
            if (IsDuplicateRange())
            {
                Error = LibResources.GetResourceValueByKey(_questionnaireData.Resources, ResourceConstants.R_DUPLICATE_DATA_KEY);
                return false;
            }
            if (IsRangeOverlapping())
            {
                Error = LibResources.GetResourceValueByKey(_questionnaireData.Resources, ResourceConstants.R_OVERLAP_RANGE_KEY);
                return false;
            }
            return true;
        }
        return false;
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel {  ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel {  ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task PopUpCallbackAsync(object id)
    {
        if (id != null)
        {
            _hideConfirmationPopup = true;
            Success = Error = string.Empty;
            if (Convert.ToInt64(id) == 1)
            {
                _questionnaireData.IsActive = false;
                await SaveSubscaleRangeAsync();
            }
        }
    }

    public async Task SaveSubscaleRangeAsync()
    {
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionnaireSubscaleRangesToServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
        if (_questionnaireData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_questionnaireData.ErrCode.ToString());
        }
        else
        {
            Error = _questionnaireData.ErrCode.ToString();
        }
    }

    private bool IsDuplicateRange()
    {
        return QuestionnaireData.QuestionnaireSubscaleRanges.Any(range => range.IsActive && range.SubScaleRangeID != _questionnaireData.QuestionnaireSubscaleRange.SubScaleRangeID
        && range.MinValue.Equals(_questionnaireData.QuestionnaireSubscaleRange.MinValue) && range.MaxValue.Equals(_questionnaireData.QuestionnaireSubscaleRange.MaxValue));
    }

    private bool IsRangeOverlapping()
    {
        return QuestionnaireData.QuestionnaireSubscaleRanges.Any(range => range.IsActive && range.SubScaleRangeID != _questionnaireData.QuestionnaireSubscaleRange.SubScaleRangeID
        && Math.Max(range.MinValue, _questionnaireData.QuestionnaireSubscaleRange.MinValue) - Math.Min(range.MaxValue, _questionnaireData.QuestionnaireSubscaleRange.MaxValue) <= 0);
    }
}