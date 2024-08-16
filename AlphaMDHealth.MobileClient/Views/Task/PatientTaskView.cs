using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientTaskView : ViewManager
{
    internal readonly ProgramDTO _programDTO = new ProgramDTO { Task = new TaskModel(), RecordCount = -1 };
    private readonly CustomLabelControl _taskType;
    private readonly CustomLabelControl _taskTypeValue;
    private readonly CustomLabelControl _item;
    private readonly CustomLabelControl _itemValue;
    private readonly CustomLabelControl _program;
    private readonly CustomLabelControl _programValue;
    private readonly CustomLabelControl _startEndDate;
    private readonly CustomLabelControl _startEndDateValue;
    private readonly CustomLabelControl _status;
    private readonly CustomLabelControl _statusValue;
    private readonly CustomLabelControl _lastActionBy;
    private readonly CustomLabelControl _lastActionByValue;
    private readonly CustomLabelControl _completionDate;
    private readonly CustomLabelControl _completionDateValue;
    private readonly CustomLabelControl _recommendation;
    private readonly CustomLabelControl _recommendationValue;
    private readonly CustomLabelControl _result;
    private readonly CustomLabelControl _resultValue;
    private readonly Grid _bodyGrid;
    private readonly CustomWebView _costumWebView;
    //  private readonly Grid _valuesContainer;
    readonly HtmlWebViewSource _instructionWebViewSource;

    public PatientTaskView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new PatientTaskService(App._essentials);
        _taskType = CreateTitleLabel();
        _item = CreateTitleLabel();
        _program = CreateTitleLabel();
        _startEndDate = CreateTitleLabel();
        _status = CreateTitleLabel();
        _lastActionBy = CreateTitleLabel();
        _completionDate = CreateTitleLabel();
        _recommendation = CreateTitleLabel();
        _result = CreateTitleLabel();
        _taskTypeValue = CreateValueLabel();
        _itemValue = CreateValueLabel();
        _programValue = CreateValueLabel();
        _startEndDateValue = CreateValueLabel();
        _statusValue = CreateValueLabel();
        _lastActionByValue = CreateValueLabel();
        _completionDateValue = CreateValueLabel();
        _recommendationValue = CreateValueLabel();
        _recommendationValue.IsHtmlLabel = true;
        _resultValue = CreateValueLabel();

        _instructionWebViewSource = new HtmlWebViewSource();

        //_valuesContainer = new Grid
        //{
        //    Style = (Style)Application.Current.Resources[LibStyleConstants.ST_END_TO_END_GRID_STYLE],
        //    VerticalOptions = LayoutOptions.Fill, 
        //    ColumnDefinitions =
        //    {
        //        new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
        //    }
        //};

        _costumWebView = new CustomWebView
        {
            VerticalOptions = LayoutOptions.Fill,
            IsAutoIncreaseHeight = true,
            ShowBusyIndicator = false,
            Source = _instructionWebViewSource,
            // Margin = new Thickness(0,-10,0, Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture))
        };

        _bodyGrid = new Grid
        {
            Padding = new Thickness(0),
            Style = IsTaskView() && DeviceInfo.Idiom == DeviceIdiom.Tablet ? (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE] : (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
            }
        };
        _bodyGrid.Add(_taskType, 0, 0);
        _bodyGrid.Add(_taskTypeValue, 0, 1);
        _bodyGrid.Add(_item, 0, 2);
        _bodyGrid.Add(_itemValue, 0, 3);
        _bodyGrid.Add(_program, 0, 4);
        _bodyGrid.Add(_programValue, 0, 5);
        _bodyGrid.Add(_startEndDate, 0, 6);
        _bodyGrid.Add(_startEndDateValue, 0, 7);
        _bodyGrid.Add(_status, 0, 8);
        _bodyGrid.Add(_statusValue, 0, 9);
        _bodyGrid.Add(_lastActionBy, 0, 10);
        _bodyGrid.Add(_lastActionByValue, 0, 11);
        _bodyGrid.Add(_completionDate, 0, 12);
        _bodyGrid.Add(_completionDateValue, 0, 13);

        ScrollView content = new ScrollView { Content = _bodyGrid };
        content.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
        content.HorizontalScrollBarVisibility = ScrollBarVisibility.Never;
        ParentPage.PageLayout.Add(content, 0, 0);
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        if (isPatientData)
        {
            SetPageContent(ParentPage.PageLayout);
        }
    }

    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        _programDTO.Task.PatientTaskID = Convert.ToInt64(GenericMethods.MapValueType<long>(GetParameterValue(nameof(TaskModel.PatientTaskID))));
        await (ParentPage.PageService as PatientTaskService).GetPatientTaskAsync(_programDTO).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_programDTO.ErrCode == ErrorCode.OK)
        {
            AssignControlResources();
            AddCompletedTaskFields();
        }
    }

    private void AssignControlResources()
    {
        _taskType.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_TASK_TYPE_KEY);
        _item.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ITEM_KEY);
        _program.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_PROGRAM_TITLE_KEY);
        _startEndDate.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_START_END_DATE_KEY);
        _status.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_STATUS_KEY);
        _lastActionBy.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_LAST_ACTION_BY_KEY);
        _completionDate.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_COMPLETION_DATE_KEY);
        _recommendation.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_RECOMMENDATION_TEXT_KEY);
        _result.Text = ParentPage.GetResourceValueByKey(_programDTO.Task.TaskType == ResourceConstants.R_QUESTIONNAIRE_KEY ? ResourceConstants.R_SCORE_KEY : ResourceConstants.R_VALUE_KEY);
        _taskTypeValue.Text = _programDTO.Task.TaskType;
        _itemValue.Text = _programDTO.Task.SelectedItemName;
        _programValue.Text = string.IsNullOrWhiteSpace(_programDTO.Task.Name) ? Constants.SYMBOL_DOUBLE_HYPHEN : _programDTO.Task.Name;
        _startEndDateValue.Text = _programDTO.Task.FromDateString;
        _statusValue.Text = _programDTO.Task.StatusValue;
        _lastActionByValue.Text = _programDTO.Task.LastActionBy;
        _completionDateValue.Text = _programDTO.Task.CompletionDateString;
        _recommendationValue.Text = string.IsNullOrWhiteSpace(_programDTO.Task.Recommendation) ? Constants.SYMBOL_DOUBLE_HYPHEN : _programDTO.Task.Recommendation;
        _resultValue.Text = string.IsNullOrWhiteSpace(_programDTO.Task.ResultValue) ? Constants.SYMBOL_DOUBLE_HYPHEN : _programDTO.Task.ResultValue;
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        if (!isPatientData)
        {
            if (GenericMethods.IsListNotEmpty(_programDTO.Items))
            {
                _instructionWebViewSource.Html = ParentPage.GetSettingsValueByKey(SettingsConstants.S_HTML_WRAPPER_KEY).Replace(Constants.STRING_FROMAT, _programDTO.Items.FirstOrDefault().OptionText);
            }
        }

    }

    private void AddCompletedTaskFields()
    {
        if (_programDTO.Task.Status == ResourceConstants.R_COMPLETED_STATUS_KEY)
        {
            if (_programDTO.Task.TaskType == ParentPage.GetResourceValueByKey(ResourceConstants.R_QUESTIONNAIRE_KEY))
            {
                _bodyGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                _bodyGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                _bodyGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                _bodyGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                _bodyGrid.Add(_result, 0, 14);
                _bodyGrid.Add(_resultValue, 0, 15);
                _bodyGrid.Add(_recommendation, 0, 16);
                _bodyGrid.Add(_recommendationValue, 0, 17);
                var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
                bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
                if (!isPatientData)
                {
                    //_valuesContainer.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    //_valuesContainer.Add(_costumWebView, 0, 0);
                    _bodyGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    _bodyGrid.Add(_costumWebView, 0, 18);
                    _costumWebView.IsEnabled = false;
                }
            }
            else if (_programDTO.Task.TaskType == ParentPage.GetResourceValueByKey(ResourceConstants.R_MEASUREMENT_KEY))
            {
                _bodyGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                _bodyGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                _bodyGrid.Add(_result, 0, 14);
                _bodyGrid.Add(_resultValue, 0, 15);
            }
            else
            {
                ////Do nothing
            }
        }
    }
    public override async Task UnloadUIAsync()
    {
        await Task.CompletedTask;
    }

    private CustomLabelControl CreateTitleLabel()
    {
        return new CustomLabelControl(LabelType.SecondrySmallLeft);
    }

    private CustomLabelControl CreateValueLabel()
    {
        return new CustomLabelControl(LabelType.PrimarySmallLeft) { Margin = new Thickness(0, 10, 0, Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture)) };
    }

    private bool IsTaskView()
    {
        return ShellMasterPage.CurrentShell.CurrentPage is PatientTasksPage || IsPatientPage();
    }
}
