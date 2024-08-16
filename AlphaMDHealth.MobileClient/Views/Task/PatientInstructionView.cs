using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientInstructionView : DynamicControlView
{
    private readonly ProgramDTO _instructionTaskData = new ProgramDTO() { Task = new TaskModel() };
    private readonly CustomButtonControl _doneButton;
    private readonly CustomMessageControl _customMessageControl;

    public PatientInstructionView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new InstructionService(App._essentials);
        _doneButton = new CustomButtonControl(ButtonType.PrimaryWithMargin) { VerticalOptions = LayoutOptions.End };
        BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
        _customMessageControl = new CustomMessageControl(false, false) { Margin = new Thickness(10, 10), WebViewHeightAutoIncrease = false, MessageType = MessageType.PageDetails };
        Grid mainLayout = new Grid
        {
            Style = (Style)App.Current.Resources[MobileConstants.IsTablet ? StyleConstants.ST_END_TO_END_GRID_STYLE : StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowSpacing = Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto},
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star) },
            }
        };
        mainLayout.Add(_customMessageControl, 0, 0);
        mainLayout.Add(_doneButton, 0, 1);
        Content = mainLayout;
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        _instructionTaskData.Task.PatientTaskID = Convert.ToInt64(GenericMethods.MapValueType<long>(GetParameterValue(nameof(TaskModel.PatientTaskID)))); //Patient task ID
        _instructionTaskData.Task.ItemID = Convert.ToInt64(GenericMethods.MapValueType<long>(GetParameterValue(nameof(TaskModel.ItemID))));
        await (ParentPage.PageService as InstructionService).GetInstructionAsync(_instructionTaskData).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_instructionTaskData.ErrCode == ErrorCode.OK && _instructionTaskData.InstructionI18N != null)
        {
            await SetTitleViewAsync().ConfigureAwait(true);
            _customMessageControl.PageResources = ParentPage.PageData;
            _customMessageControl.ControlResourceKey = _instructionTaskData.InstructionI18N.InstructionID.ToString();

            _doneButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_DONE_TEXT_KEY);
            _doneButton.Clicked += FinishButton_Clicked;

            if (string.IsNullOrWhiteSpace(_instructionTaskData.AddedBy) || _instructionTaskData.AddedBy == ResourceConstants.R_NEW_STATUS_KEY)
            {
                await UpdateTaskStatusAsync(ResourceConstants.R_INPROGRESS_STATUS_KEY).ConfigureAwait(true);
            }
        }
        else
        {
            await ParentPage.DisplayMessagePopupAsync(_instructionTaskData.ErrCode != ErrorCode.OK ? _instructionTaskData.ErrCode.ToString() : ResourceConstants.R_NO_DATA_FOUND_KEY, OnPopupActionClicked).ConfigureAwait(true);
        }
        AppHelper.ShowBusyIndicator = false;
    }
    private async void OnMessgeViewActionClicked(object sender, int e)
    {
        await FinishTaskPopupCallAsync().ConfigureAwait(true);
    }
    private async Task SetTitleViewAsync()
    {
        if (!IsPatientPage())
        {
            MenuView titleView = new MenuView(MenuLocation.Header, _instructionTaskData.InstructionI18N.Name, ShellMasterPage.CurrentShell.CurrentPage.IsAddEditPage);
            if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            {
                await ParentPage.OverrideTitleViewAsync(titleView).ConfigureAwait(true);
            }
        }
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        _customMessageControl.OnActionClicked -= OnMessgeViewActionClicked;
        await Task.CompletedTask;
    }

    private async Task UpdateTaskStatusAsync(string statusKey)
    {
        _instructionTaskData.ErrCode = await new PatientTaskService(App._essentials).UpdateTaskStatusAsync(_instructionTaskData.Task.PatientTaskID, statusKey).ConfigureAwait(true);
        if (_instructionTaskData.ErrCode == ErrorCode.OK)
        {
            await (ParentPage as BasePage).SyncDataWithServerAsync(Pages.PatientInstructionPage, false, default).ConfigureAwait(true);
        }
    }

    private async void FinishButton_Clicked(object sender, EventArgs e)
    {
        await FinishTaskPopupCallAsync().ConfigureAwait(true);
    }

    private async void OnPopupActionClicked(object sender, int e)
    {
        //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
    }

    private async Task FinishTaskPopupCallAsync()
    {
        _customMessageControl.OnActionClicked -= OnMessgeViewActionClicked;
        await ParentPage.DisplayMessagePopupAsync(ResourceConstants.R_FINISH_TASK_POP_UP_KEY, OnTaskFinishActionClicked, true, true, false).ConfigureAwait(true);
        _customMessageControl.OnActionClicked += OnMessgeViewActionClicked;
    }

    private async void OnTaskFinishActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 1:
                ParentPage.MessagePopup.PopCustomMessageControlAsync();
                ParentPage.MessagePopup.IsVisible = false;
                await FinishTaskAsync().ConfigureAwait(true);
                break;
            case 2:
                ParentPage.MessagePopup.PopCustomMessageControlAsync();
                ParentPage.MessagePopup.IsVisible = false;
                break;
            default:// to do
                break;
        }
    }

    private async Task FinishTaskAsync()
    {
        await UpdateTaskStatusAsync(ResourceConstants.R_COMPLETED_STATUS_KEY).ConfigureAwait(true);
        if (_instructionTaskData.ErrCode == ErrorCode.OK)
        {
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                InvokeListRefresh(_instructionTaskData.ErrCode.ToString(), new EventArgs());
            }
            else
            {
                await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
            }
        }
        else
        {
            ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(_instructionTaskData.ErrCode.ToString()), false);
        }
    }
}