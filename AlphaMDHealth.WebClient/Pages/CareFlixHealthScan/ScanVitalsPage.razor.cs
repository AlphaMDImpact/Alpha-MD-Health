using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class ScanVitalsPage : BasePage
{
    private readonly PatientReadingDTO _readingData = new PatientReadingDTO { RecordCount = -4 , ReadingCategoryID = 421};
    private ReadingService _readingService;
    private double? _height;
    private double? _weight;
    private bool _isNotScanVitalsPage;
    private object _dataForScan;

    [Parameter]
    public bool ShowScanVitalsPage { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        _readingService = new ReadingService(AppState.webEssentials);
        _readingData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        await SendServiceRequestAsync(_readingService.GetPatientReadingsAsync(_readingData), _readingData).ConfigureAwait(true);
        if (_readingData.ErrCode == ErrorCode.OK)
        {
            if (GenericMethods.IsListNotEmpty(_readingData.ChartMetaData))
            {
                _weight = _readingData.ChartMetaData?.FirstOrDefault(x => x.ReadingID == 654)?.ReadingValue;
                _height = _readingData.ChartMetaData?.FirstOrDefault(x => x.ReadingID == 655)?.ReadingValue;
            }
            _isDataFetched = true;
        }
        else
        {
            _readingData.ErrCode = ErrorCode.NoDataFoundKey;
        }
    }

    private async Task OnCancelClickedAsync()
    {
        if(!_isNotScanVitalsPage)
        {
            await OnClose.InvokeAsync(string.Empty);
            ShowDetailPage = false;
        }
        else
        {
            _isNotScanVitalsPage = false;
        }
        StateHasChanged();
    }
    private async Task OnNextlClickedAsync()
    {
        if (IsValid())
        {
            _readingData.PatientReadings = new List<PatientReadingModel>();
            for (int index = ((int)ReadingType.Weight) ; index <= ((int)ReadingType.Height); index++)
            {
                var reading = new PatientReadingModel
                {
                    PatientReadingID = _readingData.PatientReadingID == Guid.Empty
                        ? GenericMethods.GenerateGuid()
                        : _readingData.PatientReadingID,
                    ReadingID = (short)index,
                    UserID = _readingData.SelectedUserID,
                    ReadingDateTime = GenericMethods.GetUtcDateTime,
                    AddedON = GenericMethods.GetUtcDateTime,
                    LastModifiedON = GenericMethods.GetUtcDateTime,
                    AddedByID = _readingData.SelectedUserID.ToString(),
                    IsSynced = false,
                    IsActive = true,
                    ReadingSourceType = ReadingSource.Manual.ToString(),
                    ReadingValue = (index == ((int)ReadingType.Weight)) ? _weight : _height
                };
                _readingData.PatientReadings.Add(reading);
            }
            _readingData.RecordCount = -1;
            await SendServiceRequestAsync(_readingService.SavePatientReadingAsync(_readingData), _readingData).ConfigureAwait(true);
            if (_readingData.ErrCode != ErrorCode.OK)
            {
                Error = _readingData.ErrCode.ToString();
                _isNotScanVitalsPage = false;
            }
        }
        ShowScanVitalsPage = true;
        _isNotScanVitalsPage = true;
    }
    private async Task OnStartScanClickedAsync()
    {
        if (IsValid())
        {
            string dob = _readingData.User.Dob.HasValue ? _readingData.User.Dob.Value.ToString("yyyy-MM-dd") : "";

            string scanType = _readingData.ScanType.FirstOrDefault(x => x.IsSelected == true).GroupName == ResourceConstants.R_FACE_SCAN_TYPE_KEY ? "face" :
                             _readingData.ScanType.FirstOrDefault(x => x.IsSelected == true).GroupName == ResourceConstants.R_FINGER_SCAN_TYPE_KEY ? "finger" :
                             null;

            string posture = _readingData.Posture.FirstOrDefault(x => x.IsSelected == true).GroupName == ResourceConstants.R_RESTING_POSITION_KEY ? "resting" :
                              _readingData.Posture.FirstOrDefault(x => x.IsSelected == true).GroupName == ResourceConstants.R_STANDING_POSITION_KEY ? "standing" :
                              null;

            string gender = _readingData.Genders.FirstOrDefault(x => x.IsSelected == true).GroupName == ResourceConstants.R_MALE_KEY ? "male" :
                            _readingData.Genders.FirstOrDefault(x => x.IsSelected == true).GroupName == ResourceConstants.R_FEMALE_KEY ? "female" :
                            _readingData.Genders.FirstOrDefault(x => x.IsSelected == true).GroupName == ResourceConstants.R_NEUTRAL_KEY ? "neutral" :
                            null;
            _dataForScan = new
            {
                posture=posture,
                scanType= scanType,
                dob= dob,
                gender=gender,
                weight=_weight,
                height=_height,
            };
        }
    }
}