using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientProviderNotePopupPage : BasePopupPage
{
    private readonly PatientProviderNoteDTO _noteData = new PatientProviderNoteDTO { PatientProviderNote = new PatientProviderNoteModel(), RecordCount = -11 };
    private readonly CustomBindablePickerControl _programPicker;
    private readonly CustomBindablePickerControl _providerPicker;
    private readonly CustomBindablePickerControl _notesPicker;
    private readonly CustomDateTimeControl _dateControl;
    private readonly Grid _valuesContainer;
    private DynamicControlView dynamicControlView;
    private bool _isEnable = true;

    /// <summary>
    /// on click event of Send Button
    /// </summary>
    public event EventHandler<EventArgs> OnSaveButtonClicked;

    public PatientProviderNotePopupPage(string providerNoteID) : base(new BasePage())
    {
        _parentPage.PageService = new QuestionnaireService(App._essentials);
        if (!string.IsNullOrWhiteSpace(providerNoteID))
        {
            _noteData.PatientProviderNote.ProviderNoteID = new Guid(providerNoteID);
        }
        _programPicker = CreatePickerControl(ResourceConstants.R_SELECT_PROGRAM_KEY);
        _providerPicker = CreatePickerControl(ResourceConstants.R_SELECT_PROVIDER_KEY);
        _notesPicker = CreatePickerControl(ResourceConstants.R_PROVIDER_NOTES_KEY);
        _dateControl = new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateControl,
            ControlResourceKey = ResourceConstants.R_DATE_AND_TIME_KEY
        };
        _valuesContainer = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            VerticalOptions = LayoutOptions.StartAndExpand,
            RowSpacing = 5,
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
            }
        };
        var bodyGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowSpacing = 0,
            RowDefinitions =
            {
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
        bodyGrid.Add(_dateControl, 0, 0);
        bodyGrid.Add(_programPicker, 0, 1);
        bodyGrid.Add(_providerPicker, 0, 2);
        bodyGrid.Add(_notesPicker, 0, 3);
        bodyGrid.Add(_valuesContainer, 0, 4);
        dynamicControlView = new DynamicControlView(new BasePage(), null);
        ScrollView content = new ScrollView { Content = bodyGrid };
        _parentPage.PageLayout.Add(content, 0, 0);
    }

    protected override async void OnAppearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            AppHelper.ShowBusyIndicator = true;
            base.OnAppearing();
            await LoadUIDataAsync().ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
        }
    }

    protected override void OnDisappearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            _programPicker.SelectedValuesChanged -= ProgramValuesChanged;
            _notesPicker.SelectedValuesChanged -= NoteValuesChanged;
            OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
            OnRightHeaderClickedEvent -= OnRightHeaderClicked;
            OnBottomButtonClickedEvent -= DeleteButtonClicked;
            base.OnDisappearing();
        }
    }

    internal async Task LoadUIDataAsync()
    {
        await GetProviderNoteDataAsync(_noteData).ConfigureAwait(true);
        _parentPage.PageData = _parentPage.PageService.PageData;
        if (_noteData.ErrCode == ErrorCode.OK)
        {
            dynamicControlView.OnFileUpdated += RefreshFilesAsync;
            var resourceValue = _parentPage.GetResourceValueByKey(_noteData.PatientProviderNote.ProviderNoteID == Guid.Empty ? ResourceConstants.R_ADD_ACTION_KEY : ResourceConstants.R_EDIT_ACTION_KEY);
            SetTitle(string.Format(_parentPage.GetFeatureValueByCode(AppPermissions.PatientProviderNoteAddEdit.ToString()), resourceValue));
            await AssignControlResources();
            if (_noteData.PatientProviderNote.ProviderNoteID != Guid.Empty && _parentPage.CheckFeaturePermissionByCode(AppPermissions.PatientProviderNoteDelete.ToString()) && _isEnable)
            {
                DisplayBottomButton(ResourceConstants.R_DELETE_ACTION_KEY, FieldTypes.DeleteTransparentExButtonControl);
                OnBottomButtonClickedEvent += DeleteButtonClicked;
            }
        }
        else
        {
            await InvokeAndClosePopupAsync(_noteData.ErrCode.ToString());
        }
    }

    private async Task AssignControlResources()
    {
        _programPicker.PageResources = _parentPage.PageData;
        _providerPicker.PageResources = _parentPage.PageData;
        _notesPicker.PageResources = _parentPage.PageData;
        _dateControl.PageResources = _parentPage.PageData;
        _dateControl.PageResources = _parentPage.PageData;

        _programPicker.ItemSource = _noteData.PatientPrograms;
        _providerPicker.ItemSource = _noteData.Providers;
        _notesPicker.ItemSource = _noteData.ProgramNotes;
        if (_noteData.PatientProviderNote.ProviderNoteID != Guid.Empty)
        {
            await AssignValuesToControl();
        }
        else
        {
            await AssignDefaultValuesToControl();
        }


        _programPicker.SelectedValuesChanged += ProgramValuesChanged;
        _notesPicker.SelectedValuesChanged += NoteValuesChanged;

        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        if (_isEnable)
        {
            DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
            OnRightHeaderClickedEvent += OnRightHeaderClicked;
        }
    }

    private async Task AssignDefaultValuesToControl()
    {
        await Task.Delay(Constants.DATE_RENDER_DELAY).ConfigureAwait(true);

        _dateControl.GetSetDate = _noteData.PatientProviderNote.NoteDateTime.Value.Date;

        if (_noteData.Providers?.Any(x => x.IsSelected) ?? false)
        {
            _providerPicker.SelectedValue = _noteData.Providers.FirstOrDefault(x => x.IsSelected).OptionID;
        }
        if (_noteData?.ProgramNotes != null)
        {
            _notesPicker.ItemSource = _noteData.ProgramNotes.Where(x => x.IsDefault == true).ToList();
        }
        if (_noteData.ProgramNotes?.Any(x => x.IsSelected) ?? false)
        {
            _notesPicker.SelectedValue = _noteData.ProgramNotes.FirstOrDefault(x => x.IsSelected).OptionID;
        }
        if (_noteData.PatientPrograms?.Any(x => x.IsSelected) ?? false)
        {
            _programPicker.SelectedValue = _noteData.PatientPrograms.FirstOrDefault(x => x.IsSelected).OptionID;
        }
        if (_noteData.ProgramNotes?.Any(x => x.IsSelected) ?? false)
        {
            _notesPicker.SelectedValue = _noteData.ProgramNotes.FirstOrDefault(x => x.IsSelected).OptionID;
        }
    }

    private async Task AssignValuesToControl()
    {
        await Task.Delay(Constants.DATE_RENDER_DELAY).ConfigureAwait(true);
        _dateControl.GetSetDate = App._essentials.ConvertToLocalTime(_noteData.PatientProviderNote.NoteDateTime.Value).Date;
        if (_noteData.PatientPrograms.Any(x => x.OptionID == _noteData.PatientProviderNote.ProgramID))
        {
            _programPicker.SelectedValue = _noteData.PatientProviderNote.ProgramID;
        }
        if (_noteData.Providers.Any(x => x.OptionID == _noteData.PatientProviderNote.CareGiverID))
        {
            _providerPicker.SelectedValue = _noteData.PatientProviderNote.CareGiverID;
        }
        if (_noteData.ProgramNotes.Any(x => x.OptionID == _noteData.PatientProviderNote.ProgramNoteID))
        {
            _notesPicker.SelectedValue = _noteData.PatientProviderNote.ProgramNoteID;
        }
        if (_noteData.ProgramNotes.Find(x => x.OptionID == _noteData.PatientProviderNote.ProgramNoteID).IsDefault == false ||
          _noteData.PatientPrograms.Find(x => x.OptionID == _noteData.PatientProviderNote.ProgramID).IsDefault)
        {
            _isEnable = false;
        }

        _dateControl.IsControlEnabled = _isEnable;
        _programPicker.IsControlEnabled = _isEnable;
        _providerPicker.IsControlEnabled = _isEnable;
        _notesPicker.IsControlEnabled = _isEnable;
        await RenderControls(_noteData);
    }

    private CustomBindablePickerControl CreatePickerControl(string resourceKey)
    {
        return new CustomBindablePickerControl
        {
            ControlResourceKey = resourceKey,
            IsUnderLine = true
        };
    }

    private async void ProgramValuesChanged(object sender, EventArgs e)
    {
        RemoveControl();
        PatientProviderNoteDTO _providerNoteData = new PatientProviderNoteDTO
        {
            RecordCount = -2,
            PatientProviderNote = new PatientProviderNoteModel
            {
                ProgramID = _noteData.PatientPrograms[((sender) as CustomBindablePicker).SelectedIndex].OptionID
            }
        };
        await GetProviderNoteDataAsync(_providerNoteData).ConfigureAwait(true);
        _noteData.ProgramNotes = _providerNoteData.ProgramNotes;
        _noteData.Providers = _providerNoteData.Providers;
        _providerPicker.ItemSource = _providerNoteData.Providers;
        _notesPicker.ItemSource = _providerNoteData.ProgramNotes;
        _noteData.QuestionnaireQuestionAnswers?.ForEach(x => x.IsActive = false);
    }

    private async void NoteValuesChanged(object sender, EventArgs e)
    {
        RemoveControl();
        PatientProviderNoteDTO _providerNoteData = new PatientProviderNoteDTO
        {
            RecordCount = -3,
            PatientProviderNote = new PatientProviderNoteModel
            {
                QuestionnaireID = _noteData.ProgramNotes[((sender) as CustomBindablePicker).SelectedIndex].ParentOptionID
            }
        };
        await GetProviderNoteDataAsync(_providerNoteData).ConfigureAwait(true);
        if (_providerNoteData.ErrCode == ErrorCode.OK)
        {
            _noteData.ProviderQuestions = _providerNoteData.ProviderQuestions;
            _noteData.QuestionnaireQuestionOptions = _providerNoteData.QuestionnaireQuestionOptions;
            _noteData.QuestionnaireQuestions = _providerNoteData.QuestionnaireQuestions;
            _noteData.QuestionnaireQuestionAnswers?.ForEach(x => x.IsActive = false);
            await RenderControls(_providerNoteData);
        }
    }

    private void RemoveControl()
    {
        if (_noteData.ProviderQuestions?.Count > 0)
        {
            _valuesContainer.RowDefinitions?.Clear();
            _valuesContainer.Children?.Clear();
        }
    }

    private async Task RenderControls(PatientProviderNoteDTO _noteData)
    {
        if (GenericMethods.IsListNotEmpty(_noteData.ProviderQuestions))
        {
            var index = 0;
            dynamicControlView.ParentPage.PageData = _parentPage.PageData;
            foreach (var item in _noteData.ProviderQuestions.Select((question, i) => new { i, question }))
            {
                var question = item.question;
                QuestionnaireDTO _questionnaireData = new QuestionnaireDTO
                {
                    PhoneNumber = string.Concat(question.QuestionTypeID, question.QuestionID),
                    Question = _noteData.QuestionnaireQuestions.FirstOrDefault(x => x.QuestionID == question.QuestionID),
                    QuestionOptions = _noteData.QuestionnaireQuestionOptions.Where(x => x.QuestionID == question.QuestionID)?.ToList(),
                    QuestionDetail = _noteData.QuestionnaireQuestiosDetails?.FirstOrDefault(x => x.QuestionID == question.QuestionID),
                    QuestionnaireQuestionAnswer = _noteData.QuestionnaireQuestionAnswers?.FirstOrDefault(x => x.QuestionID == question.QuestionID) ?? new PatientQuestionnaireQuestionAnswersModel(),
                };
                if (item.question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey && GenericMethods.IsListNotEmpty(_noteData.FileDocuments) && _questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID != Guid.Empty)
                {
                    _questionnaireData.FileDocuments = _noteData.FileDocuments.Where(x => x.DocumentSourceID == _questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID.ToString())?.ToList();
                }
                //todo:find HtmlLabel replacement for this
                Label _questionContentView = new Label
                {
                    // As RTL is not working in iOS, Set HorizontalTextAlignment
                    HorizontalTextAlignment = Device.RuntimePlatform == Device.iOS && (FlowDirection)App.Current.Resources[StyleConstants.ST_FLOW_DIRECTION] == FlowDirection.RightToLeft ? TextAlignment.End : TextAlignment.Start,
                    TextColor = Color.FromArgb(StyleConstants.PRIMARY_TEXT_COLOR),
                    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) * 1.2,
                    Text = _questionnaireData.Question.IsRequired
                    ? string.Concat(_questionnaireData.Question.CaptionText.Split(new[] { Constants.HTML_RICH_TEXT_END_TAG }, StringSplitOptions.None)[0], Constants.RED_ASTRICK_HTML, Constants.HTML_RICH_TEXT_END_TAG)
                    : _questionnaireData.Question.CaptionText
                };
                _valuesContainer.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                _valuesContainer.Add(_questionContentView, 0, index);
                index++;
                _questionnaireData.LanguageID = (byte)item.i;
                View control = dynamicControlView.CreateControlValuesHandler[question.QuestionTypeID](_questionnaireData);
                control.IsEnabled = _isEnable;
                if (question.QuestionTypeID == QuestionType.DateTimeQuestionKey || question.QuestionTypeID == QuestionType.DateQuestionKey)
                {
                    await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
                    if (!string.IsNullOrWhiteSpace(_questionnaireData.QuestionnaireQuestionAnswer.AnswerValue))
                    {
                        (control as CustomDateTimeControl).GetSetDate = App._essentials.ConvertToLocalTime(Convert.ToDateTime(_questionnaireData.QuestionnaireQuestionAnswer.AnswerValue, CultureInfo.InvariantCulture)).Date;
                    }
                }
                _valuesContainer.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                control.StyleId = string.Concat(question.QuestionID, item.i);
                _valuesContainer.Add(control, 0, index);
                index++;
            }
        }
    }

    private async Task GetProviderNoteDataAsync(PatientProviderNoteDTO _noteData)
    {
        _noteData.LanguageID = (byte)App._essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
        AppHelper.ShowBusyIndicator = true;
        await (_parentPage.PageService as QuestionnaireService).GetProviderNoteAsync(_noteData).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    private async Task SaveProviderNoteAsync(bool isActive)
    {
        _noteData.PatientProviderNote.IsActive = isActive;
        _noteData.QuestionnaireQuestionAnswers?.ForEach(x => x.IsSynced = false);
        _noteData.PatientProviderNote.IsSynced = false;
        AppHelper.ShowBusyIndicator = true;
        if (isActive)
        {
            if (_parentPage.IsFormValid())
            {
                string errorMsg = MapAndSaveProviderNoteAsync();
                if (!string.IsNullOrWhiteSpace(errorMsg))
                {
                    AppHelper.ShowBusyIndicator = false;
                    _parentPage.DisplayOperationStatus(_parentPage.GetResourceValueByKey(ResourceConstants.R_QUESTIONNAIRE_MANDATORY_ANSWER_ERROR_KEY));
                    return;
                }
            }
            else
            {
                AppHelper.ShowBusyIndicator = false;
                return;
            }
        }
        else
        {
            _noteData.QuestionnaireQuestionAnswers?.ForEach(x => x.IsActive = isActive);
        }
        await (_parentPage.PageService as QuestionnaireService).SavePatientProviderAsync(_noteData).ConfigureAwait(true);
        if (_noteData.ErrCode == ErrorCode.OK)
        {
            _ = (_parentPage as BasePage).SyncDataWithServerAsync(Pages.PatientProviderNotesView, false,
                             App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)).ConfigureAwait(true);
            await InvokeAndClosePopupAsync(_noteData.ErrCode.ToString()).ConfigureAwait(true);
        }
        else
        {
            AppHelper.ShowBusyIndicator = false;
            _parentPage.DisplayOperationStatus(_parentPage.GetResourceValueByKey(_noteData.ErrCode.ToString()));
        }
    }

    private string GetAnswerValue()
    {
        if (_noteData.QuestionnaireQuestionAnswers == null)
        {
            _noteData.QuestionnaireQuestionAnswers = new List<PatientQuestionnaireQuestionAnswersModel>();
        }
        _noteData.FileDocuments = new List<FileDocumentModel>();
        foreach (var item in _noteData.ProviderQuestions.Select((question, i) => new { i, question }))
        {
            var question = item.question;
            string answerValue = string.Empty;
            if (question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey)
            {
                FileDocumentModel fileDocumentData;
                fileDocumentData = dynamicControlView.GetFileValue(string.Concat("FileList", Constants.SYMBOL_DASH, question.QuestionID, item.i))?.FirstOrDefault();
                if (fileDocumentData == null && _noteData.QuestionnaireQuestions.FirstOrDefault(x => x.QuestionID == question.QuestionID).IsRequired)
                {
                    return ResourceConstants.R_QUESTIONNAIRE_MANDATORY_ANSWER_ERROR_KEY;
                }
                else
                {
                    if (fileDocumentData != null)
                    {
                        var categoryID = (long)_noteData.QuestionnaireQuestions.FirstOrDefault(x => x.QuestionID == question.QuestionID).CategoryID;
                        new FileService(App._essentials).MapQuestionnaireFileData(categoryID, fileDocumentData);
                        answerValue = fileDocumentData.DocumentDescription;
                        _noteData.FileDocuments.Add(fileDocumentData);
                    }
                    else
                    {
                        answerValue = string.Empty;
                    }
                }
            }
            else
            {
                //todo:
                //var valueField = _valuesContainer?.Children?.FirstOrDefault(x => x.StyleId == string.Concat(question.QuestionID, item.i));
                //if (valueField != null)
                //{
                //     answerValue = dynamicControlView.GetControlValuesHandler[question.QuestionTypeID](valueField);
                //    if (question.QuestionTypeID == QuestionType.SingleSelectQuestionKey)
                //    {
                //        answerValue =  dynamicControlView.GetSingleSelectAnswer(answerValue, _noteData.QuestionnaireQuestionOptions?.Where(x => x.QuestionID == question.QuestionID)?.ToList());
                //    }
                //}
            }
            if ((question.QuestionTypeID == QuestionType.MultiSelectQuestionKey || question.QuestionTypeID == QuestionType.SingleSelectQuestionKey || question.QuestionTypeID == QuestionType.VerticalSliderQuestionKey || question.QuestionTypeID == QuestionType.HorizontalSliderQuestionKey) &&
                question.IsRequired == true && string.IsNullOrWhiteSpace(answerValue))
            {
                return ResourceConstants.R_QUESTIONNAIRE_MANDATORY_ANSWER_ERROR_KEY;
            }
            var nextQID = (item.i < (_noteData.ProviderQuestions.Count - 1)) ? _noteData.ProviderQuestions[item.i + 1] == null ? 0 : _noteData.ProviderQuestions[item.i + 1].QuestionID : 0;
            var prevQID = (item.i > 0) ? (_noteData.ProviderQuestions[item.i - 1] == null ? 0 : _noteData.ProviderQuestions[item.i - 1].QuestionID) : 0;

            if (_noteData.QuestionnaireQuestionAnswers.Any(x => x.QuestionID == question.QuestionID))
            {
                _noteData.QuestionnaireQuestionAnswers.FirstOrDefault(x => x.QuestionID == question.QuestionID).AnswerValue = answerValue;
                _noteData.QuestionnaireQuestionAnswers.FirstOrDefault(x => x.QuestionID == question.QuestionID).IsSynced = false;
                _noteData.QuestionnaireQuestionAnswers.FirstOrDefault(x => x.QuestionID == question.QuestionID).IsActive = true;
            }
            else
            {
                _noteData.QuestionnaireQuestionAnswers.Add(new PatientQuestionnaireQuestionAnswersModel
                {
                    QuestionID = question.QuestionID,
                    AnswerValue = answerValue,
                    IsActive = true,
                    NextQuestionID = nextQID,
                    PreviousQuestionID = prevQID,
                    TaskType = 2,
                    IsSynced = false,
                    PatientTaskID = _noteData.PatientProviderNote.ProviderNoteID.ToString(),
                });
            }
        }
        if (GenericMethods.IsListNotEmpty(dynamicControlView._removedDocuments))
        {
            _noteData.FileDocuments.AddRange(dynamicControlView._removedDocuments);
        }
        return string.Empty;
    }

    private string MapAndSaveProviderNoteAsync()
    {
        _noteData.PatientProviderNote.NoteDateTime = _dateControl.GetSetDate.Value.ToUniversalTime();
        _noteData.PatientProviderNote.ProgramID = _programPicker.SelectedValue;
        _noteData.PatientProviderNote.CareGiverID = _providerPicker.SelectedValue;
        _noteData.PatientProviderNote.ProgramNoteID = _notesPicker.SelectedValue;
        _noteData.PatientProviderNote.PatientID = _noteData.SelectedUserID;
        if (GenericMethods.IsListNotEmpty(_noteData.ProviderQuestions))
        {
            return GetAnswerValue();
        }
        else
        {
            return string.Empty;
        }
    }

    private async void DeleteButtonClicked(object sender, EventArgs e)
    {
        await _parentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnMessgeViewActionClicked, true, true, false).ConfigureAwait(true);
    }

    private async void OnMessgeViewActionClicked(object sender, int e)
    {
        OnBottomButtonClickedEvent -= DeleteButtonClicked;
        _parentPage.OnClosePupupAction(sender, e);
        if (e == 1)
        {
            await SaveProviderNoteAsync(false).ConfigureAwait(true);
        }
        OnBottomButtonClickedEvent += DeleteButtonClicked;
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        await SaveProviderNoteAsync(true).ConfigureAwait(true);
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        await InvokeAndClosePopupAsync(default);
    }

    private async Task InvokeAndClosePopupAsync(string errorCode)
    {
        AppHelper.ShowBusyIndicator = false;
        OnSaveButtonClicked?.Invoke(errorCode, new EventArgs());
        //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
    }

    private void RefreshFilesAsync(object sender, EventArgs e)
    {
        string uniqueKey = sender.ToString();
        List<FileDocumentModel> documents = dynamicControlView.GetFileValue(uniqueKey)?.ToList();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (GenericMethods.IsListNotEmpty(documents))
            {
                //todo: ((_valuesContainer.Children.FirstOrDefault(x => x.StyleId == uniqueKey.Split(Constants.SYMBOL_DASH)[1]) as Grid).Children.FirstOrDefault(x => x.StyleId == uniqueKey) as CustomListView).Header = null;
                //todo: ((_valuesContainer.Children.FirstOrDefault(x => x.StyleId == uniqueKey.Split(Constants.SYMBOL_DASH)[1]) as Grid).Children.FirstOrDefault(x => x.StyleId == uniqueKey) as CustomListView).ItemsSource = documents;
            }
            else
            {
                CustomMessageControl _emptyListView = new CustomMessageControl(false)
                {
                    ControlResourceKey = ResourceConstants.R_DOCUMENT_FILE_EMPTY_VIEW_KEY,
                    PageResources = _parentPage.PageData,
                };
                //todo: ((_valuesContainer.Children.FirstOrDefault(x => x.StyleId == uniqueKey.Split(Constants.SYMBOL_DASH)[1]) as Grid).Children.FirstOrDefault(x => x.StyleId == uniqueKey) as CustomListView).Header = _emptyListView;
                //todo: ((_valuesContainer.Children.FirstOrDefault(x => x.StyleId == uniqueKey.Split(Constants.SYMBOL_DASH)[1]) as Grid).Children.FirstOrDefault(x => x.StyleId == uniqueKey) as CustomListView).HeightRequest = Convert.ToDouble(Application.Current.Resources[LibStyleConstants.ST_DOUBLE_ROW_HEIGHT], CultureInfo.InvariantCulture);
                //todo: ((_valuesContainer.Children.FirstOrDefault(x => x.StyleId == uniqueKey.Split(Constants.SYMBOL_DASH)[1]) as Grid).Children.FirstOrDefault(x => x.StyleId == uniqueKey) as CustomListView).ItemsSource = null;
            }
        });
    }
}