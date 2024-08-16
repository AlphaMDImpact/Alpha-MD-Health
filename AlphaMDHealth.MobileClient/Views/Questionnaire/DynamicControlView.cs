using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Utils.Filtering;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class DynamicControlView : BaseAttachmentView
{
    public Dictionary<QuestionType, Func<object, string>> GetControlValuesHandler { get; } = new Dictionary<QuestionType, Func<object, string>>();
    public Dictionary<QuestionType, Func<QuestionnaireDTO, View>> CreateControlValuesHandler { get; } = new Dictionary<QuestionType, Func<QuestionnaireDTO, View>>();
    private readonly FileDTO _documentData = new FileDTO { File = new FileModel() };
    private bool _isAttachmentDeleteClicked;
    private string _selectedID;
    public List<FileDocumentModel> _removedDocuments;
    public ViewManager _readingDetailView;

    /// <summary>
    /// Callback method to get value after selection changed in single select case
    /// </summary>
    public event EventHandler<EventArgs> OnFileUpdated;

    public DynamicControlView(BasePage page, object parameters) : base(page, parameters)
    {
        AddGetValueEvents();
        AddCreateControlEvents();
    }

    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        await Task.Run(() =>
        {
            //load ui events
        }).ConfigureAwait(true);
    }

    public override async Task UnloadUIAsync()
    {

        await Task.Run(() =>
        {
            //Unload ui events
        }).ConfigureAwait(true);

    }

    private void AddGetValueEvents()
    {
        GetControlValuesHandler.Add(QuestionType.SingleSelectQuestionKey, GetOptionValue);
        GetControlValuesHandler.Add(QuestionType.DropDownQuestionKey, GetDropDownValue);
        GetControlValuesHandler.Add(QuestionType.TextQuestionKey, GetEntryTextValue);
        GetControlValuesHandler.Add(QuestionType.NumericQuestionKey, GetEntryTextValue);
        GetControlValuesHandler.Add(QuestionType.MultiSelectQuestionKey, GetMultipleOptionValue);
        GetControlValuesHandler.Add(QuestionType.DateQuestionKey, GetDateValue);
        GetControlValuesHandler.Add(QuestionType.TimeQuestionKey, GetDateValue);
        GetControlValuesHandler.Add(QuestionType.DateTimeQuestionKey, GetDateValue);
        GetControlValuesHandler.Add(QuestionType.HorizontalSliderQuestionKey, GetSliderValue);
        GetControlValuesHandler.Add(QuestionType.VerticalSliderQuestionKey, GetSliderValue);
        GetControlValuesHandler.Add(QuestionType.MultilineTextQuestionKey, GetMultiLineEntryTextValue);
        GetControlValuesHandler.Add(QuestionType.RichTextQuestionKey, GetRichTextValue);
    }

    private void AddCreateControlEvents()
    {
        CreateControlValuesHandler.Add(QuestionType.TextQuestionKey, CreateDynamicEntry);
        CreateControlValuesHandler.Add(QuestionType.NumericQuestionKey, CreateDynamicNumericEntry);
        CreateControlValuesHandler.Add(QuestionType.DropDownQuestionKey, CreateDropDown);
        CreateControlValuesHandler.Add(QuestionType.SingleSelectQuestionKey, CreateDynamicRadioList);
        CreateControlValuesHandler.Add(QuestionType.MultiSelectQuestionKey, CreateDynamicMultiSelectList);
        CreateControlValuesHandler.Add(QuestionType.DateQuestionKey, CreateDynamicDate);
        CreateControlValuesHandler.Add(QuestionType.TimeQuestionKey, CreateDynamicTime);
        CreateControlValuesHandler.Add(QuestionType.DateTimeQuestionKey, CreateDynamicDateTime);
        CreateControlValuesHandler.Add(QuestionType.HorizontalSliderQuestionKey, CreateDynamicSliderBar);
        CreateControlValuesHandler.Add(QuestionType.VerticalSliderQuestionKey, CreateDynamicSliderBar);
        CreateControlValuesHandler.Add(QuestionType.MultilineTextQuestionKey, CreateDynamicMultiLineEntry);
        CreateControlValuesHandler.Add(QuestionType.RichTextQuestionKey, CreateMessageView);
        CreateControlValuesHandler.Add(QuestionType.FilesAndDocumentQuestionKey, CreateUploadView);
        CreateControlValuesHandler.Add(QuestionType.MeasurementQuestionKey, CreateReadingView);
    }

    #region Get Control Values

    public IEnumerable<FileDocumentModel> GetFileValue(string uniqueKey)
    {
        return _documentData.FileDocuments?.Where(x => x.UniqueKey == uniqueKey && x.IsActive)?.ToList() ?? Enumerable.Empty<FileDocumentModel>();
    }

    private string GetDateValue(object view)
    {
        var date = (view as CustomDateTimeControl).GetSetDate;
        return date == null ? string.Empty : ((DateTimeOffset)date.Value).ToUniversalTime().ToString(CultureInfo.InvariantCulture);
    }

    private string GetDropDownValue(object view)
    {
        var anserValue = Convert.ToString((view as CustomBindablePickerControl).SelectedValue, CultureInfo.InvariantCulture);
        if (anserValue == Constants.CONSTANT_ZERO || anserValue == Constants.CONSTANT_NEG_ONE)
        {
            return string.Empty;
        }
        return anserValue;
    }

    private string GetEntryTextValue(object view)
    {
        return string.IsNullOrWhiteSpace((view as CustomEntryControl).Value) ? string.Empty : (view as CustomEntryControl).Value;
    }

    private string GetOptionValue(object view)
    {
        return (view as CustomRadioTextList).SelectedIndex.ToString();
    }

    private string GetMultipleOptionValue(object view)
    {
        var selectesValues = (view as CustomCheckBoxListControl).SelectedIndexValues;
        if (selectesValues != null)
        {
            return string.Join(Constants.SYMBOL_PIPE_SEPERATOR.ToString(CultureInfo.InvariantCulture), selectesValues);
        }
        return string.Empty;
    }

    private string GetSliderValue(object view)
    {
        return (view as CustomSliderControl).GetSliderValue();
    }

    private string GetMultiLineEntryTextValue(object view)
    {
        return string.IsNullOrWhiteSpace((view as CustomMultiLineEntryControl).Value) ? string.Empty : (view as CustomMultiLineEntryControl).Value;
    }

    private string GetRichTextValue(object view)
    {
        return string.Empty;
    }

    public string GetSingleSelectAnswer(string selectedIndex, List<QuestionnaireQuestionOptionModel> questionOptions)
    {
        int SelectedIndex = Convert.ToInt32(selectedIndex);
        return SelectedIndex > -1 ? questionOptions.ElementAt(SelectedIndex)?.QuestionOptionID.ToString() : string.Empty;
    }

    #endregion

    #region create controls
    private View CreateReadingView(QuestionnaireDTO questionnaire)
    {
        _readingDetailView = new PatientReadingView(new BasePage(),
            ParentPage.AddParameters(
               ParentPage.CreateParameter(nameof(PatientReadingUIModel.ReadingID), questionnaire.Question.CategoryID?.ToString(CultureInfo.InvariantCulture)),
               ParentPage.CreateParameter(nameof(PatientReadingModel.PatientTaskID), questionnaire.PatientTaskID.ToString(CultureInfo.InvariantCulture)),
               ParentPage.CreateParameter(nameof(PatientReadingUIModel.PatientReadingID), string.IsNullOrEmpty(questionnaire.QuestionnaireQuestionAnswer.AnswerValue)
                   ? Guid.Empty.ToString()
                   : questionnaire.QuestionnaireQuestionAnswer.AnswerValue),
               ParentPage.CreateParameter(nameof(BaseDTO.SelectedUserID), questionnaire.SelectedUserID.ToString(CultureInfo.InvariantCulture)),
               ParentPage.CreateParameter(Constants.IS_COMMING_FROM_QUESTIONNAIRE_VIEW, true.ToString()),
               ParentPage.CreateParameter(nameof(QuestionnaireQuestionModel.IsRequired), (questionnaire.Question.IsRequired).ToString())
            ));
        return _readingDetailView;
    }

    private View CreateUploadView(QuestionnaireDTO questionnaire)
    {
        CustomButtonControl customButtonControl = new CustomButtonControl(ButtonType.PrimaryWithMargin)
        {
            Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_UPLOAD_DOCUMENT_TEXT_KEY),
            StyleId = string.Concat(Constants.BUTTON, Constants.SYMBOL_DASH, questionnaire.Question.QuestionID, questionnaire.LanguageID),
        };
        customButtonControl.Clicked += UploadNewClicked;
        CustomCellModel filesModel = new CustomCellModel
        {
            CellID = nameof(FileDocumentModel.ClientFileDocumentID),
            CellHeader = nameof(FileDocumentModel.DocumentDescription),
            CellDescription = nameof(FileDocumentModel.FormattedDate),
            //todo:CellLeftSourceIcon = nameof(FileDocumentModel.ImageSource),
            CellLeftDefaultIcon = nameof(FileDocumentModel.DocumentImage),
            ShowUnreadBadge = nameof(FileDocumentModel.ShowUnreadBadge),
            IsUnreadHeader = nameof(FileDocumentModel.IsUnreadHeader),
            CellRightContentHeader = nameof(FileDocumentModel.ShowRemoveButtonText),
            ShowRemoveButton = nameof(FileDocumentModel.ShowRemoveButton),
        };

        CustomListView filesList = new CustomListView(ListViewCachingStrategy.RetainElement)
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_CustomListView_2_ROW_HEIGHT_STYLE],
            Margin = new Thickness(0, 0, 0, 15),
            SelectionMode = ListViewSelectionMode.Single,
            StyleId = string.Concat(Constants.FILE_LIST, Constants.SYMBOL_DASH, questionnaire.Question.QuestionID, questionnaire.LanguageID),
            ItemTemplate = new DataTemplate(() =>
            {
                ResponsiveView view = new ResponsiveView(filesModel) { Margin = new Thickness(0, 0, 0, new OnIdiom<double> { Phone = 10, Tablet = 0 }) };
                view.OnItemClicked += DeleteDocumentClicked;
                return new ViewCell { View = view };
            })
        };
        filesList.HeightRequest = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_DEFAULT_CARD_HEIGHT_STYLE], CultureInfo.InvariantCulture);
        Grid bodyGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            ColumnSpacing = 1,
            StyleId = string.Concat(questionnaire.Question.QuestionID, questionnaire.LanguageID),
            Padding = new Thickness(1, 0),
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition{ Height = GridLength.Auto },
                new RowDefinition{ Height = GridLength.Auto }
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
               new ColumnDefinition { Width = GridLength.Star },
            },
        };
        bodyGrid.Add(customButtonControl, 0, 0);
        bodyGrid.Add(filesList, 0, 1);
        _removedDocuments = null;
        if (_documentData.FileDocuments == null || questionnaire.PhoneNumber == ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY)
        {
            _documentData.FileDocuments = new List<FileDocumentModel>();
        }
        if (GenericMethods.IsListNotEmpty(questionnaire.FileDocuments))
        {
            questionnaire.FileDocuments[0].UniqueKey = filesList.StyleId;
            questionnaire.FileDocuments[0].IsActive = true;
            _documentData.FileDocuments.AddRange(questionnaire.FileDocuments);
            filesList.Header = string.Empty;
            filesList.ItemsSource = questionnaire.FileDocuments.ToList();
            filesList.HeightRequest = (filesList.RowHeight + new OnIdiom<int> { Phone = 10, Tablet = 0 }) * (questionnaire.FileDocuments?.Count ?? 1);
        }
        else
        {
            CustomMessageControl emptyListView = new CustomMessageControl(false)
            {
                ControlResourceKey = ResourceConstants.R_DOCUMENT_FILE_EMPTY_VIEW_KEY,
                PageResources = ParentPage.PageData,
            };
            filesList.Header = emptyListView;
            filesList.HeightRequest = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_DEFAULT_CARD_HEIGHT_STYLE], CultureInfo.InvariantCulture);
            filesList.ItemsSource = new List<FileDocumentModel>();
        }
        filesList.ItemTapped += FilesList_ItemTapped;
        return bodyGrid;
    }

    private void AttachmentPage_OnSendButtonClicked(object sender, EventArgs e)
    {
        CustomAttachmentModel attachment = sender as CustomAttachmentModel;
        if (!attachment.IsDisabledSaveButton)
        {
            Guid existingGuid = Guid.Empty;
            FileDocumentModel existingDoc = null;
            if (GenericMethods.IsListNotEmpty(_removedDocuments))
            {
                existingDoc = _removedDocuments?.First(x => x.UniqueKey == attachment.UniqueKey);
            }
            if (!string.IsNullOrWhiteSpace(existingDoc?.DocumentSourceID))
            {
                existingGuid = existingDoc.FileDocumentID;
                _removedDocuments.Remove(existingDoc);
            }
            FileDocumentModel document = new FileDocumentModel
            {
                ClientFileDocumentID = attachment.ID == Guid.Empty ? (existingGuid == Guid.Empty ? GenericMethods.GenerateGuid() : existingDoc.ClientFileDocumentID) : (attachment.ID),
                DocumentName = attachment.AttachmentBase64,
                FileDocumentName = attachment.FileName,
                DocumentDescription = attachment.Text,
                IsUnreadHeader = true,
                IsActive = true,
                ShowRemoveButton = true,
                IsDownloaded = true,
                ShowRemoveButtonText = ParentPage.GetResourceValueByKey(ResourceConstants.R_DELETE_ACTION_KEY),
                AddedOn = App._essentials.ConvertToLocalTime(GenericMethods.GetUtcDateTime),
                AddedByID = existingGuid == Guid.Empty ? 0 : existingDoc.AddedByID,
                FormattedDate = string.Concat(ParentPage.GetResourceValueByKey(ResourceConstants.R_DOCUMENT_FILE_UPLOADEDBY_TEXT_KEY), Constants.STRING_SPACE, ParentPage.GetResourceValueByKey(ResourceConstants.R_DOCUMENT_FILE_YOU_TEXT_KEY), Constants.STRING_SPACE, DateTime.Now.ToString(Constants.DEFAULT_DATE_FORMAT, CultureInfo.InvariantCulture)),
                UniqueKey = attachment.UniqueKey
            };
            if (existingGuid != Guid.Empty)
            {
                document.FileDocumentID = existingGuid;
            }
            new FileService(App._essentials).GetFileIcon(document);
            if (attachment.ID == Guid.Empty)
            {
                _documentData.FileDocuments.Insert(0, document);
            }
            else
            {
                _documentData.FileDocuments.Find(x => x.ClientFileDocumentID == document.ClientFileDocumentID)
                  .DocumentDescription = attachment.Text?.Trim();
                _documentData.FileDocuments.Find(x => x.ClientFileDocumentID == document.ClientFileDocumentID)
                  .IsSynced = false;
            }
            OnFileUpdated?.Invoke(document.UniqueKey, new EventArgs());
        }
    }

    private async void FilesList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        CustomListView list = sender as CustomListView;
        list.ItemTapped -= FilesList_ItemTapped;
        FileDocumentModel document = (list.SelectedItem as FileDocumentModel);
        if (document != null && !string.IsNullOrWhiteSpace(document.DocumentName))
        {
            CustomAttachmentModel attachment = new CustomAttachmentModel
            {
                ID = document.ClientFileDocumentID,
                AttachmentBase64 = document.DocumentName,
                FileName = document.FileDocumentName,
                FileType = GetFileExtension(document.FileDocumentName),
                //todo:AttachmentSource = document.ImageSource,
                DefaultIcon = document.DocumentImage,
                IsActive = false,
                Text = document.DocumentDescription,
                IsDisabledSaveButton = !document.ShowRemoveButton,
                ControlResourceKey = ResourceConstants.R_DOCUMENT_CAPTION_KEY,
                UniqueKey = list.StyleId,
            };
            await ShowAttachmentViewAsync(attachment).ConfigureAwait(true);
        }
        list.ItemTapped += FilesList_ItemTapped;
    }

    private async Task ShowAttachmentViewAsync(CustomAttachmentModel attachment)
    {
        CustomAttachmentPopupPage attachmentPage = new CustomAttachmentPopupPage(attachment, ParentPage);
        attachmentPage.OnSendButtonClicked += AttachmentPage_OnSendButtonClicked;
        //todo:await Navigation.PushPopupAsync(attachmentPage).ConfigureAwait(true);
    }

    private AppFileExtensions GetFileExtension(string file)
    {
        return file.Split(Constants.DOT_SEPARATOR)[1].ToEnum<AppFileExtensions>();
    }

    private async void UploadNewClicked(object sender, EventArgs e)
    {
        string uniqueKey = (sender as CustomButtonControl).StyleId;

        if (_documentData.FileDocuments.Any(x => x.UniqueKey == string.Concat(Constants.FILE_LIST, Constants.SYMBOL_DASH, uniqueKey.Split(Constants.SYMBOL_DASH)[1])))
        {
            await ParentPage.DisplayMessagePopupAsync(ResourceConstants.R_UPLOAD_ONLY_ONE_DOCUMENT_KEY, false, true, false);
        }
        else
        {
            MaxFileUploadSize = ParentPage.GetSettingsValueByKey(SettingsConstants.S_LARGE_IMAGE_RESOLUTION_KEY);
            SupportedFileTypes = ParentPage.GetSettingsValueByKey(SettingsConstants.S_UPLOAD_SUPPORTED_FILE_TYPE_KEY);
            List<string> actions = Actionlist(FieldTypes.UploadControl);
            await ImageSourceSelectionAsync(actions).ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(_base64String) && _base64String != ImageConstants.I_UPLOAD_ICON_PNG)
            {
                if (GenericMethods.IsExtensionSupported(SupportedFileTypes, _uploadedFileExtension))
                {
                    CustomAttachmentModel attachment = new CustomAttachmentModel
                    {
                        AttachmentBase64 = GetBase64WithPrefix(),
                        FileName = FileNameWithExtention,
                        ControlResourceKey = ResourceConstants.R_DOCUMENT_CAPTION_KEY,
                        FileType = _uploadedFileExtension.ToEnum<AppFileExtensions>(),
                        UniqueKey = string.Concat(Constants.FILE_LIST, Constants.SYMBOL_DASH, uniqueKey.Split(Constants.SYMBOL_DASH)[1])
                    };
                    await ShowAttachmentViewAsync(attachment).ConfigureAwait(true);
                }
                else
                {
                    await ParentPage.DisplayMessagePopupAsync(
                           ParentPage.GetResourceValueByKey(ResourceConstants.R_SUPPORTED_UPLOAD_FILE_TYPE_KEY).Replace("{0}", SupportedFileTypes),
                           ParentPage.OnPupupActionClicked, false, true, true).ConfigureAwait(true);
                }
                _base64String = ImageConstants.I_UPLOAD_ICON_PNG;
            }
        }
    }

    private async void DeleteDocumentClicked(object sender, EventArgs e)
    {
        if (!_isAttachmentDeleteClicked)
        {
            _isAttachmentDeleteClicked = true;
            _selectedID = (sender as CustomLabelControl).ClassId;
            await ParentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnMessgeDeleteDocumentsActionClicked, true, true, false).ConfigureAwait(true);
        }
    }

    private void OnMessgeDeleteDocumentsActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 1:
                ParentPage.OnClosePupupAction(sender, e);
                DeleteDocument();
                break;
            case 2:
                ParentPage.OnClosePupupAction(sender, e);
                break;
            default:// to do
                break;
        }
        _selectedID = string.Empty;
        _isAttachmentDeleteClicked = false;
    }

    private void DeleteDocument()
    {
        FileDocumentModel document = _documentData.FileDocuments.FirstOrDefault(x => x.ClientFileDocumentID == new Guid(_selectedID));
        if (document != null)
        {
            _documentData.FileDocuments.Remove(document);
            if (document.FileDocumentID != Guid.Empty)
            {
                document.IsActive = false;
                document.IsSynced = false;
                if (_removedDocuments == null)
                {
                    _removedDocuments = new List<FileDocumentModel>();
                }
                _removedDocuments.Add(document);
            }
            OnFileUpdated?.Invoke(document.UniqueKey, new EventArgs());
        }
    }

    private View CreateMessageView(QuestionnaireDTO questionnaire)
    {
        HtmlWebViewSource instructionWebViewSource = new HtmlWebViewSource
        {
            Html = ParentPage.GetSettingsValueByKey(SettingsConstants.S_HTML_WRAPPER_KEY).Replace(Constants.STRING_FROMAT, questionnaire.QuestionDetail.InstructionsText)
        };
        return new CustomWebView
        {
            HeightRequest = 1,
            IsAutoIncreaseHeight = true,
            ShowBusyIndicator = true,
            Source = instructionWebViewSource,
            IsEnabled = false,
            Margin = new Thickness(0, 0, 0, Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture))
        };
    }

    private View CreateDynamicEntry(QuestionnaireDTO questionnaire)
    {
        return new CustomEntryControl
        {
            PageResources = ParentPage.PageData,
            ControlResourceKey = questionnaire.PhoneNumber,
            Value = questionnaire.QuestionnaireQuestionAnswer.AnswerValue,
            IsUnderLine = !(questionnaire.PhoneNumber == ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY),
            ShowHeader = false,
        };
    }

    private View CreateDropDown(QuestionnaireDTO questionnaire)
    {
        if (!GenericMethods.IsListNotEmpty(questionnaire.DropDownOptions) && GenericMethods.IsListNotEmpty(questionnaire.QuestionOptions))
        {
            questionnaire.DropDownOptions = GetDropDownOptions(questionnaire.QuestionOptions);
        }
        return new CustomBindablePickerControl
        {
            IsUnderLine = !(questionnaire.PhoneNumber == ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY),
            ShowHeader = false,
            PageResources = ParentPage.PageData,
            ControlResourceKey = questionnaire.PhoneNumber,
            ItemSource = questionnaire.DropDownOptions,
            SelectedValue = Convert.ToInt64(string.IsNullOrWhiteSpace(questionnaire.QuestionnaireQuestionAnswer.AnswerValue) ? Constants.NUMBER_ZERO : questionnaire.QuestionnaireQuestionAnswer.AnswerValue, CultureInfo.InvariantCulture)
        };
    }

    private View CreateDynamicNumericEntry(QuestionnaireDTO questionnaire)
    {
        return new CustomEntryControl
        {
            ControlType = FieldTypes.DecimalEntryControl,
            Value = questionnaire.QuestionnaireQuestionAnswer.AnswerValue,
            IsUnderLine = !(questionnaire.PhoneNumber == ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY),
            ShowHeader = false,
            PageResources = ParentPage.PageData,
            ControlResourceKey = questionnaire.PhoneNumber
        };
    }

    private View CreateDynamicMultiLineEntry(QuestionnaireDTO questionnaire)
    {
        return new CustomMultiLineEntryControl
        {
            PageResources = ParentPage.PageData,
            ControlResourceKey = questionnaire.PhoneNumber,
            ShowHeader = false,
            IsUnderLine = !(questionnaire.PhoneNumber == ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY),
            Value = questionnaire.QuestionnaireQuestionAnswer.AnswerValue,
        };
    }

    private View CreateDynamicRadioList(QuestionnaireDTO questionnaire)
    {
        var options = GetOptionSelectList(questionnaire.QuestionOptions, questionnaire.QuestionnaireQuestionAnswer.AnswerValue);
        bool isPatientQuestionnaire = !(questionnaire.PhoneNumber == ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY) && Device.RuntimePlatform == Device.iOS;
        CustomRadioTextList customRadioList = new CustomRadioTextList
        {
            IsHorizontal = false,
            RadioWidth = Device.RuntimePlatform == Device.iOS ? (double)AppImageSize.ImageSizeXXL : 190,
            RadioButtonStyle = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_RADIO_BUTTON_KEY],
            Margin = new Thickness(0, 10, 0, 5)
        };
        customRadioList.ItemsSource = options.Select(x => x.DisplayText);
        customRadioList.SelectedIndex = options.IndexOf(options.FirstOrDefault(x => x.IsSelected));
        return customRadioList;
    }

    private View CreateDynamicMultiSelectList(QuestionnaireDTO questionnaire)
    {
        var view = new CustomCheckBoxListControl { ApplyMargin = new OnIdiom<bool> { Phone = false, Tablet = true } };
        view.SetOptions(GetOptionSelectList(questionnaire.QuestionOptions, questionnaire.QuestionnaireQuestionAnswer.AnswerValue));
        return view;
    }

    private View CreateDynamicSliderBar(QuestionnaireDTO questionnaire)
    {
        SliderViewModel sliderViewModel = new SliderViewModel
        {
            SlidebarRightLabelText = string.Empty,
            SlidebarStepSize = questionnaire.Question.SliderSteps,
            SlidebarQuestionText = questionnaire.Question.CaptionText,
            SlidebarLeftLabelText = string.Empty,
            SlidebarMinValue = questionnaire.Question.MinValue.ToString(CultureInfo.InvariantCulture),
            SlidebarMaxValue = questionnaire.Question.MaxValue.ToString(CultureInfo.InvariantCulture),
            Value = questionnaire.QuestionnaireQuestionAnswer.AnswerValue,
            IsVerticalSlider = questionnaire.Question.QuestionTypeID == QuestionType.VerticalSliderQuestionKey ? true : false
        };
        return new CustomSliderControl(sliderViewModel)
        {
            StyleId = string.Concat(Constants.FILE_LIST, Constants.SYMBOL_DASH, questionnaire.Question.QuestionID, questionnaire.LanguageID)
        };
    }

    private View CreateDynamicDateTime(QuestionnaireDTO questionnaire)
    {
        LibSettings.TryGetDateFormatSettings(ParentPage.PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
        return new CustomDateTimeControl
        {
            IsUnderLine = !(questionnaire.PhoneNumber == ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY),
            ControlType = FieldTypes.DateTimeControl,
            PageResources = ParentPage.PageData,
            ControlResourceKey = questionnaire.PhoneNumber,
            ShowHeader = false,
            DateFormat = GenericMethods.GetDateTimeFormat(DateTimeType.Date, dayFormat, monthFormat, yearFormat),
            TimeFormat = GenericMethods.GetDateTimeFormat(DateTimeType.Time, dayFormat, monthFormat, yearFormat),
            SetTime = string.IsNullOrEmpty(questionnaire.QuestionnaireQuestionAnswer.AnswerValue)
                ? default
                : App._essentials.ConvertToLocalTime(DateTime.Parse(questionnaire.QuestionnaireQuestionAnswer.AnswerValue, CultureInfo.InvariantCulture)).TimeOfDay,
        };
    }

    private View CreateDynamicDate(QuestionnaireDTO questionnaire)
    {
        LibSettings.TryGetDateFormatSettings(ParentPage.PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
        return new CustomDateTimeControl
        {
            IsUnderLine = !(questionnaire.PhoneNumber == ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY),
            ControlType = FieldTypes.DateControl,
            PageResources = ParentPage.PageData,
            ControlResourceKey = questionnaire.PhoneNumber,
            ShowHeader = false,
            GetSetDate = string.IsNullOrEmpty(questionnaire.QuestionnaireQuestionAnswer.AnswerValue) ? (DateTime?)null : DateTime.Parse(questionnaire.QuestionnaireQuestionAnswer.AnswerValue, CultureInfo.InvariantCulture),
            DateFormat = GenericMethods.GetDateTimeFormat(DateTimeType.Date, dayFormat, monthFormat, yearFormat),
        };
    }

    private View CreateDynamicTime(QuestionnaireDTO questionnaire)
    {
        LibSettings.TryGetDateFormatSettings(ParentPage.PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
        return new CustomDateTimeControl
        {
            IsUnderLine = !(questionnaire.PhoneNumber == ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY),
            ControlType = FieldTypes.TimeControl,
            PageResources = ParentPage.PageData,
            ControlResourceKey = questionnaire.PhoneNumber,
            ShowHeader = false,
            TimeFormat = GenericMethods.GetDateTimeFormat(DateTimeType.Time, dayFormat, monthFormat, yearFormat),
            SetTime = string.IsNullOrEmpty(questionnaire.QuestionnaireQuestionAnswer.AnswerValue)
            ? default
            : App._essentials.ConvertToLocalTime(DateTime.Parse(questionnaire.QuestionnaireQuestionAnswer.AnswerValue, CultureInfo.InvariantCulture)).TimeOfDay
        };
    }

    private List<OptionSelectModel> GetOptionSelectList(List<QuestionnaireQuestionOptionModel> questionOptions, string selectedAnswer)
    {
        List<OptionSelectModel> options = new List<OptionSelectModel>();
        OptionSelectModel optionSelect;
        foreach (var option in questionOptions)
        {
            optionSelect = new OptionSelectModel
            {
                DisplayText = option.CaptionText,
                Value = option.QuestionOptionID.ToString(CultureInfo.InvariantCulture),
                IsSelected = (option.IsSelected || (selectedAnswer?.Contains(option.QuestionOptionID.ToString()) ?? false))
            };
            options.Add(optionSelect);
        }
        return options;
    }

    private List<OptionModel> GetDropDownOptions(List<QuestionnaireQuestionOptionModel> questionOptions)
    {
        List<OptionModel> options = new List<OptionModel>();
        OptionModel optionSelect;
        foreach (var option in questionOptions)
        {
            optionSelect = new OptionModel
            {
                OptionText = option.CaptionText,
                OptionID = option.QuestionOptionID,
                IsSelected = option.IsSelected
            };
            options.Add(optionSelect);
        }
        return options;
    }

    protected override void DeleteUploads()
    {
        throw new NotImplementedException();
    }

    #endregion
}