using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// view for set target page
/// </summary>
public class PatientReadingTargetView : ViewManager
{
    private readonly CustomBindablePickerControl _readingTypePicker;
    private readonly CustomEntryControl _minValueEntry;
    private readonly CustomEntryControl _maxValueEntry;
    internal readonly PatientReadingDTO _readingData;
    private string _minPlaceHolder;
    private string _maxPlaceHolder;
    private bool _isFirstCallForPlceholder = true;

    /// <summary>
    /// constructor for set target view 
    /// </summary>
    /// <param name="page"></param>
    /// <param name="readingID"></param>
    public PatientReadingTargetView(BasePage page, object readingID) : base(page, readingID)
    {
        _readingData = new PatientReadingDTO
        {
            RecordCount = -3,
            PatientReadingTarget = new ReadingTargetModel(),
            LanguageID = (byte)App._essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0),
            SelectedUserID = App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0),
        };
        ParentPage.PageService = new ReadingService(App._essentials);
        _readingTypePicker = new CustomBindablePickerControl
        {
            ControlResourceKey = ResourceConstants.R_READING_TYPE_KEY,
        };
        _minValueEntry = new CustomEntryControl
        {
            ControlResourceKey = ResourceConstants.R_READING_TARGET_MIN_VALUE_KEY,
            ControlType = FieldTypes.DecimalEntryControl,
            IsVisible = false
        };
        _maxValueEntry = new CustomEntryControl
        {
            ControlType = FieldTypes.DecimalEntryControl,
            ControlResourceKey = ResourceConstants.R_READING_TARGET_MAX_VALUE_KEY,
            IsVisible = false
        };
        ParentPage.SetPageLayoutOption(LayoutOptions.FillAndExpand, false);
        ParentPage.PageLayout.Padding = new Thickness((double)Application.Current.Resources[StyleConstants.ST_APP_PADDING]);
        ParentPage.AddRowColumnDefinition(GridLength.Auto, 3, true);
        ParentPage.PageLayout.Add(_readingTypePicker, 0, 0);
        ParentPage.PageLayout.Add(_minValueEntry, 0, 1);
        ParentPage.PageLayout.Add(_maxValueEntry, 0, 2);
    }

    /// <summary>
    /// target view LoadUIAsync
    /// </summary>
    /// <param name="isRefreshRequest">check whether need to read parameter or not</param>
    /// <returns></returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        _readingData.ReadingID = GenericMethods.MapValueType<short>(GetParameterValue(nameof(PatientReadingTargetPage.ReadingID)));
        await (ParentPage.PageService as ReadingService).GetPatientReadingsAsync(_readingData).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_readingData?.ErrCode == ErrorCode.OK)
        {
            if (_readingData.PatientReadingTargets.Count == 1)
            {
                _readingTypePicker.SelectedValue = _readingData.PatientReadingTargets[0].ReadingID;
            }
            _readingTypePicker.SelectedValuesChanged += OnReadingPickerSelectionChange;
            _readingTypePicker.PageResources = _minValueEntry.PageResources = _maxValueEntry.PageResources = ParentPage.PageData;
            _readingTypePicker.ItemSource = _readingData.ReadingOptions;
        }
        else
        {
            await ParentPage.DisplayMessagePopupAsync(_readingData.ErrCode.ToString(), false, true, false).ConfigureAwait(true);
            await ParentPage.PopPageAsyncForPhoneAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// UnloadUIAsync
    /// </summary>
    /// <returns></returns>
    public async override Task UnloadUIAsync()
    {
        _readingTypePicker.SelectedValuesChanged -= OnReadingPickerSelectionChange;
        await Task.CompletedTask;
    }

    private void OnReadingPickerSelectionChange(object sender, EventArgs e)
    {
        CustomBindablePicker picker = (CustomBindablePicker)sender;
        AssignAbsoluteRanges(_readingData, (int)_readingTypePicker.SelectedValue);
        _minValueEntry.PageResources = ParentPage.PageData;
        _maxValueEntry.PageResources = ParentPage.PageData;
        var metadata = _readingData.ChartMetaData.FirstOrDefault(x => x.ReadingID == _readingTypePicker.SelectedValue);
        _minValueEntry.DecimalPrecision = _maxValueEntry.DecimalPrecision = metadata.DigitsAfterDecimalPoint;
        _minValueEntry.Value = metadata.TargetMinValue.ToString();
        _maxValueEntry.Value = metadata.TargetMaxValue.ToString();
        _maxValueEntry.IsVisible = true;
        _minValueEntry.IsVisible = true;
    }

    public void AssignAbsoluteRanges(PatientReadingDTO targetData, int selectedValue)
    {
        if (_isFirstCallForPlceholder)
        {
            _minPlaceHolder = targetData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_READING_TARGET_MIN_VALUE_KEY)?.ResourceValue;
            _maxPlaceHolder = targetData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_READING_TARGET_MAX_VALUE_KEY)?.ResourceValue;
            _isFirstCallForPlceholder = false;
        }
        (ParentPage.PageService as ReadingService).AssignAbsoluteRanges(targetData, selectedValue, _minPlaceHolder, _maxPlaceHolder);
    }

    internal async Task<bool> OnSaveButtonClickedAsync()
    {
        if (ParentPage.IsFormValid())
        {
            _readingData.PatientReadingTarget.TargetMinValue = Convert.ToSingle(_minValueEntry.Value, CultureInfo.InvariantCulture);
            _readingData.PatientReadingTarget.TargetMaxValue = Convert.ToSingle(_maxValueEntry.Value, CultureInfo.InvariantCulture);
            if (_readingData.PatientReadingTarget.TargetMinValue > _readingData.PatientReadingTarget.TargetMaxValue)
            {
                ParentPage.DisplayOperationStatus(string.Format(CultureInfo.InvariantCulture,
                    ParentPage.GetResourceValueByKey(ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                    ParentPage.GetResourceValueByKey(ResourceConstants.R_READING_TARGET_MIN_VALUE_KEY),
                    ParentPage.GetResourceValueByKey(ResourceConstants.R_READING_TARGET_MAX_VALUE_KEY)));
                return false;
            }
            AppHelper.ShowBusyIndicator = true;
            _readingData.PatientReadingTarget.ReadingID = (short)_readingTypePicker.SelectedValue;
            await (ParentPage.PageService as ReadingService).SavePatientReadingsTargetAsync(_readingData, CancellationToken.None).ConfigureAwait(true);
            if (_readingData.ErrCode == ErrorCode.OK)
            {
                await ParentPage.SyncDataWithServerAsync(Pages.PatientReadingTargetPage, false, _readingData.SelectedUserID).ConfigureAwait(true);
                return true;
            }
            AppHelper.ShowBusyIndicator = false;
            ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(_readingData.ErrCode.ToString()));
        }
        return false;
    }
}