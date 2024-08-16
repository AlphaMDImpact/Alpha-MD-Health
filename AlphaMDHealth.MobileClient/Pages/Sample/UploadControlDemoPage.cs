using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Collections.ObjectModel;

namespace AlphaMDHealth.MobileClient;

public class UploadControlDemoPage : BasePage
{
    private readonly AmhUploadControl _uploadControl;
    private readonly AmhUploadControl _uploadControl1;
    private readonly AmhUploadControl _uploadControl2;
    private readonly AmhUploadControl _uploadControl3;
    private readonly AmhUploadControl _uploadControl4;
    private readonly AmhUploadControl _uploadControl5;
    private AmhButtonControl _primaryButton;

    public UploadControlDemoPage() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.FillAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);
        _uploadControl = new AmhUploadControl(FieldTypes.UploadControl)
        {
            ResourceKey = "SingleUploadControl",
        };
        AddView(_uploadControl);

        _uploadControl1 = new AmhUploadControl(FieldTypes.UploadControl)
        {
            ResourceKey = "SingleUploadControl",

        };
        AddView(_uploadControl1);

        _uploadControl2 = new AmhUploadControl(FieldTypes.UploadControl)
        {
            ResourceKey = "SingleUploadControl",
        };
        AddView(_uploadControl2);

        _uploadControl3 = new AmhUploadControl(FieldTypes.UploadControl)
        {
            ResourceKey = "MultipleUploadControl",
        };
        AddView(_uploadControl3);

        _uploadControl4 = new AmhUploadControl(FieldTypes.UploadControl)
        {
            ResourceKey = "MultipleUploadControl",
        };
        AddView(_uploadControl4);

        _uploadControl5 = new AmhUploadControl(FieldTypes.UploadControl)
        {
            ResourceKey = "MultipleUploadControl",
        };
        AddView(_uploadControl5);

        _primaryButton = new AmhButtonControl(FieldTypes.PrimaryButtonControl)
        {
            Icon = ImageConstants.I_USER_ID_PNG,
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_primaryButton);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await new AuthService(App._essentials).GetAccountDataAsync(PageData).ConfigureAwait(true);

        var demoService = new ControlDemoService(App._essentials);
        demoService.GetControlDemoPageResources(PageData);

        _uploadControl.PageResources = _primaryButton.PageResources = _uploadControl1.PageResources = _uploadControl2.PageResources = _uploadControl3.PageResources = _uploadControl4.PageResources = _uploadControl5.PageResources = PageData;
        //single upload
        _uploadControl1.Value = new ObservableCollection<AttachmentModel>
        {
            new AttachmentModel
            {
                FileID = GenericMethods.GenerateGuid(),FileName = "jayhind.doc",IsActive = true,FileExtension = "doc"
            },
        };
        _uploadControl2.IsControlEnabled = false;
        _uploadControl4.Value = new ObservableCollection<AttachmentModel>
        {
            new AttachmentModel
            {
                FileID = GenericMethods.GenerateGuid(),FileName = "document.pdf",IsActive = true,FileIcon = ImageConstants.I_PDF_ICON,FileExtension = "pdf"
            },
        };
        _uploadControl5.IsControlEnabled = false;
        _uploadControl5.Value = new ObservableCollection<AttachmentModel>
        {
            new AttachmentModel
            {
                FileID = GenericMethods.GenerateGuid(),FileName = "jayhind.pdf",IsActive = true,FileIcon = ImageConstants.I_PDF_ICON,FileExtension = "pdf"
            },
            new AttachmentModel
            {
                FileID = GenericMethods.GenerateGuid(),FileExtension = Constants.DOC_FILE_TYPE,IsActive = true,FileIcon = ImageConstants.I_DOC_ICON_PNG,FileName = "abcd.doc"
            },
        };
        _primaryButton.OnValueChanged += OnSignInButtonClicked;
        AppHelper.ShowBusyIndicator = false;
    }
    List<string> files = new List<string>();
    private async void OnSignInButtonClicked(object sender, EventArgs e)
    {
        var attachment = _uploadControl.Value;
        var attachment1 = _uploadControl1.Value;
        var attachment2 = _uploadControl2.Value;
        var attachment3 = _uploadControl3.Value;
        var attachment4 = _uploadControl4.Value;
        var attachment5 = _uploadControl5.Value;

        if (IsFormValid())
        {
            AppHelper.ShowBusyIndicator = true;
            await ShellMasterPage.CurrentShell.PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
        }
        //var file = await FilePicker.Default.PickAsync();
        //if (file != null)
        //{
        //    using(MemoryStream ms = new MemoryStream()) 
        //    {
        //        var stream = await file.OpenReadAsync();
        //            await stream.CopyToAsync(ms);
        //        var base64 = GenericMethods.GetBase64MetaDataPrefix(file.FileName) + Convert.ToBase64String(ms.ToArray());
        //        files.Add(base64);
        //    }
        //}
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _primaryButton.OnValueChanged -= OnSignInButtonClicked;
    }
    private void AddView(View view)
    {
        AddRowColumnDefinition(new GridLength(1, GridUnitType.Auto), 1, true);
        int index = PageLayout.Children?.Count() ?? 0;
        PageLayout.Add(view, 0, index);
    }
}