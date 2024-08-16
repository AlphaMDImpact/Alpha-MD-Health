using System.Globalization;
using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class WelcomeScreenPage : BasePage
{
    private readonly AppIntroDTO _appIntroData = new() { RecordCount = -1, AppIntro = new AppIntroModel() };
    private List<ButtonActionModel> _messageButtonActions;
    private AppIntroService _service;
    private bool _showDeletedConfirmationPopup = true;
    private double? _sequenceNo;
    private bool _isEditable;

    /// <summary>
    /// App Intro ID param
    /// </summary>
    [Parameter]
    public long AppIntroID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _service = new AppIntroService(AppState.webEssentials);
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _appIntroData.AppIntro.IntroSlideID = AppIntroID;
        await SendServiceRequestAsync(_service.GetAppIntrosAsync(_appIntroData), _appIntroData).ConfigureAwait(true);
        if (_appIntroData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_appIntroData.FeaturePermissions, AppPermissions.WelcomeScreenAddEdit.ToString());
            _sequenceNo = _appIntroData.AppIntro.SequenceNo == 0 ? null : _appIntroData.AppIntro.SequenceNo;
            _appIntroData.AppIntro.ImageName = _appIntroData.AppIntro.ImageName;
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_appIntroData.ErrCode.ToString());
        }
    }

    private void OnImageChanged(object file)
    {
        _appIntroData.AppIntro.ImageName = file != null && file is AttachmentModel && (file as AttachmentModel).IsActive
              ? (file as AttachmentModel).FileValue
              : string.Empty;
    }

    private readonly List<TabDataStructureModel> DataFormatter = new()
    {
        new TabDataStructureModel
        {
            DataField = nameof(AppIntroModel.HeaderText),
            ResourceKey = ResourceConstants.R_HEADER_TEXT_KEY,
            //AllowImage = false,
            //Height = Constants.SMALL_TEXT_BOX_HEIGHT,
            //IsRequired = true
        },
        new TabDataStructureModel
        {
            DataField = nameof(AppIntroModel.SubHeaderText),
            ResourceKey = ResourceConstants.R_BODY_TEXT_KEY,
            //AllowImage = false,
            //Height = Constants.SMALL_TEXT_BOX_HEIGHT,
            //IsRequired = true
        },
    };

    private void OnDeleteClick()
    {
        _messageButtonActions = new List<ButtonActionModel>
        {
            new ButtonActionModel{ ButtonID = Constants.NUMBER_ONE, ButtonResourceKey =ResourceConstants.R_OK_ACTION_KEY  },
            new ButtonActionModel{ ButtonID = Constants.NUMBER_TWO, ButtonResourceKey =ResourceConstants.R_CANCEL_ACTION_KEY  },
        };
        _showDeletedConfirmationPopup = false;
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            _appIntroData.AppIntro.ImageName = _appIntroData.AppIntro.ImageName;
            _appIntroData.AppIntro.SequenceNo = Convert.ToInt32(_sequenceNo, CultureInfo.InvariantCulture);
            _appIntroData.AppIntro.IsActive = true;
            AppIntroDTO appIntroData = new()
            {
                AppIntro = _appIntroData.AppIntro,
                AppIntros = _appIntroData.AppIntros
            };
            await SaveAppIntroAsync(appIntroData).ConfigureAwait(true);
        }
    }

    private async Task OnActionClickAsync(object sequenceNo)
    {
        _showDeletedConfirmationPopup = true;
        Success = Error = string.Empty;
        if (Convert.ToInt64(sequenceNo) == 1)
        {
            _appIntroData.AppIntro.IsActive = false;
            AppIntroDTO appIntroData = new()
            {
                AppIntro = _appIntroData.AppIntro,
                AppIntros = _appIntroData.AppIntros
            };
            await SaveAppIntroAsync(appIntroData).ConfigureAwait(true);
        }
    }

    private async Task SaveAppIntroAsync(AppIntroDTO appIntroData)
    {
        await SendServiceRequestAsync(_service.SyncAppIntroToServerAsync(appIntroData, CancellationToken.None), appIntroData).ConfigureAwait(true);
        if (appIntroData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(appIntroData.ErrCode.ToString());
        }
        else
        {
            Error = appIntroData.ErrCode.ToString();
        }
    }
}