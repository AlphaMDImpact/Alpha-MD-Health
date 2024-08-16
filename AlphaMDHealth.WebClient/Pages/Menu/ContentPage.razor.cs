using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Xml;

namespace AlphaMDHealth.WebClient;

public partial class ContentPage : BasePage
{
    private readonly ContentPageDTO _contentData = new ContentPageDTO { RecordCount = -1 };
    private List<TabDataStructureModel> _dataFormatter;
    private List<ButtonActionModel> _popupActions;
    private string _selectedPageType;
    private bool _showPublishUnpublishConfirmation = true;
    private bool _showPreview;
    private bool _isPublishUnpublish;
    private bool _isEditable;
    private bool _controlDataReset = true;

    /// <summary>
    /// PageId parameter
    /// </summary>
    [Parameter]
    public long PageId { get; set; }

    /// <summary>
    /// IsEducationPage parameter
    /// </summary>
    [Parameter]
    public bool IsEducationPage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _contentData.Page = new ContentPageModel
        {
            PageID = PageId,
            IsEducation = AppState.RouterData.SelectedRoute.Page == AppPermissions.EducationsView.ToString() || AppState.RouterData.SelectedRoute.Page == AppPermissions.MyEducationsView.ToString()
        };
        await SendServiceRequestAsync(new ContentPageService(AppState.webEssentials).GetContentPagesAsync(_contentData), _contentData).ConfigureAwait(true);
        if (_contentData.ErrCode == ErrorCode.OK)
        {
            _isEditable = IsEducationPage
                ? LibPermissions.HasPermission(_contentData.FeaturePermissions, AppPermissions.EducationAddEdit.ToString())
                : LibPermissions.HasPermission(_contentData.FeaturePermissions, AppPermissions.StaticContentAddEdit.ToString());
            string selectedPageType = _contentData.Page.IsLink ? Convert.ToString((int)ContentType.Link, CultureInfo.InvariantCulture) : Convert.ToString((int)ContentType.Content, CultureInfo.InvariantCulture);
            SetLanguageControlFormatter(PageId == 0 ? Convert.ToString((int)ContentType.Content, CultureInfo.InvariantCulture) : selectedPageType);
            _isDataFetched = true;
            _controlDataReset = false;
        }
        else
        {
            await OnClose.InvokeAsync(_contentData.ErrCode.ToString());
        }
    }

    private void OnPageTypeChanged(object value)
    {
        if (value != null)
        {
            _selectedPageType = value as string;
            if (_controlDataReset)
            {
                RemoveControlContainsKey(ResourceConstants.R_TITLE_KEY);
                RemoveControlContainsKey(ResourceConstants.R_PAGE_DESCRIPTION_KEY);
                RemoveControlContainsKey(ResourceConstants.R_EDITOR_KEY);
                RemoveControlContainsKey(ResourceConstants.R_LINKS_KEY);
                foreach (var data in _contentData.PageDetails)
                {
                    data.PageData = string.Empty;
                }
            }
            _controlDataReset = true;
            SetLanguageControlFormatter(value as string);
        }
    }

    private void SetLanguageControlFormatter(string value)
    {
        _contentData.Page.IsLink = false;
        _contentData.Page.IsPdf = false;
        switch (value.ToEnum<ContentType>())
        {
            case ContentType.Content:
                _dataFormatter = GetContentDataFormatter();
                break;
            case ContentType.Link:
                _dataFormatter = GetLinksDataFormatter();
                _contentData.Page.IsLink = true;
                break;
            case ContentType.Pdf:
                _dataFormatter = GetPdfDataFormatter();
                _contentData.Page.IsPdf = true;
                break;
        }
    }

    private List<TabDataStructureModel> GetContentDataFormatter()
    {
        return new List<TabDataStructureModel>
        {
            new TabDataStructureModel{DataField=nameof(ContentDetailModel.PageHeading), ResourceKey=ResourceConstants.R_TITLE_KEY },
            new TabDataStructureModel{DataField=nameof(ContentDetailModel.Description), ResourceKey=ResourceConstants.R_PAGE_DESCRIPTION_KEY,IsRequired=false  },
            new TabDataStructureModel{DataField=nameof(ContentDetailModel.PageData), ResourceKey=ResourceConstants.R_EDITOR_KEY,},
        };
    }

    private List<TabDataStructureModel> GetLinksDataFormatter()
    {
        return new List<TabDataStructureModel>
        {
             new TabDataStructureModel{DataField=nameof(ContentDetailModel.PageHeading), ResourceKey=ResourceConstants.R_TITLE_KEY  },
             new TabDataStructureModel{DataField=nameof(ContentDetailModel.PageData), ResourceKey=ResourceConstants.R_LINKS_KEY, RegexPattern = LibSettings.GetSettingValueByKey(_contentData.Settings, SettingsConstants.S_PATH_REGEX_KEY)}
        };
    }

    private List<TabDataStructureModel> GetPdfDataFormatter()
    {
        return new List<TabDataStructureModel>
        {
             new TabDataStructureModel{DataField=nameof(ContentDetailModel.PageHeading), ResourceKey=ResourceConstants.R_TITLE_KEY}
        };
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
            ContentPageDTO contentPage = new ContentPageDTO
            {
                Page = _contentData.Page,
                PageDetails = _contentData.PageDetails,
            };
            if (!contentPage.Page.IsPdf)
            {
                contentPage.Page.PDFName = null;
            }
            contentPage.Page.IsEducation = IsEducationPage;
            if (IsEducationPage)
            {
                contentPage.Page.EducationCategoryID = _contentData.EducationCategory.FirstOrDefault(x => x.IsSelected).OptionID;
            }
            await SendServiceRequestAsync(new ContentPageService(AppState.webEssentials).SyncContentPageToServerAsync(contentPage, _isPublishUnpublish, new CancellationToken()), contentPage).ConfigureAwait(true);
            _contentData.ErrCode = contentPage.ErrCode;
            if (_contentData.ErrCode == ErrorCode.OK)
            {
                await OnClose.InvokeAsync(_contentData.ErrCode.ToString());
            }
            else
            {
                Error = _contentData.ErrCode.ToString();
            }
        }
    }

    private void OnPublishUnpublishClick()
    {
        if(IsValid())
        {
            _popupActions = new List<ButtonActionModel> {
                new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
                new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
            };
            _isPublishUnpublish = !_contentData.Page.IsPublished;
            _showPublishUnpublishConfirmation = false;
        }
    }

    private async Task OnPublishUnpublishActionClick(object sequenceNo)
    {
        if (sequenceNo != null)
        {
            _showPublishUnpublishConfirmation = true;
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                ContentPageDTO contentPage = new ContentPageDTO
                {
                    Page = _contentData.Page,
                };
                contentPage.Page.PageID = _contentData.Page.PageID;
                contentPage.Page.IsPublished = !_contentData.Page.IsPublished;
                await SendServiceRequestAsync(new ContentPageService(AppState.webEssentials).SyncPublilshedContentToServerAsync(_contentData, new CancellationToken()), _contentData).ConfigureAwait(true);
                _contentData.ErrCode = contentPage.ErrCode;
                if(contentPage.ErrCode == ErrorCode.OK)
                {
                    //_contentData.Page.IsPublished = contentPage.Page.IsPublished;
                    await OnClose.InvokeAsync(_contentData.ErrCode.ToString());
                }
                else
                {
                    Error = contentPage.ErrCode.ToString();
                }
            }
        }
    }

    private void OnPreviewClick()
    {
        _showPreview = true;
    }

    private void OnPreviewClosedAsync(string errorMessage)
    {
        _showPreview = false;
        _controlDataReset = false;
    }
}