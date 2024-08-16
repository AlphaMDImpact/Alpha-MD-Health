using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient
{
    public class QuestionnaireTaskView : DynamicControlView
    {
        private readonly QuestionnaireDTO _questionnaireData = new QuestionnaireDTO();
        private readonly CustomInfoControl _helpView;
        private readonly CustomButtonControl _nextFinishButton;
        private readonly CustomButtonControl _prevButton;
        private readonly ContentView _answerView;
        private readonly ScrollView _answerScroll;
        private readonly Label _questionContentView; //todo:HtmlLabel
        private readonly Grid _buttonLayout;
        private readonly CustomProgressBarControl _progressBar;
        private View _controlObj;
        private readonly CustomMessageControl _startAndDoneView;
        private readonly List<View> _controls = new List<View>();
        private readonly double _padding;
        private long? _previousQuestionID;
        private readonly int _roleID;
        private View _answerViewContent;
        private bool _allowNext = true;
        private string _readingDateTimeErrorMessage;

        public QuestionnaireTaskView(BasePage page, object parameters) : base(page, parameters)
        {
            _padding = Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
            ParentPage.PageService = new QuestionnaireService(App._essentials);
            _startAndDoneView = new CustomMessageControl(false)
            {
                ShowIcon = true,
                MessageType = MessageType.ConfirmationPopup,
                Padding = new Thickness(0, 0, 0, _padding),
                ShouldRemoveWebViewMargin = true
            };
            _questionContentView = new Label
            {
                LineBreakMode = LineBreakMode.WordWrap,
                Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_MEDIUM_STYLE],
                // As RTL is not working in iOS, Set HorizontalTextAlignment
                HorizontalTextAlignment = Device.RuntimePlatform == Device.iOS && (FlowDirection)App.Current.Resources[StyleConstants.ST_FLOW_DIRECTION] == FlowDirection.RightToLeft
                ? TextAlignment.End
                : TextAlignment.Start,

            };
            _answerView = new ContentView { Padding = new Thickness(1) };
            _answerScroll = new ScrollView
            {
                Content = _answerView
            };
            _helpView = new CustomInfoControl(true);
            _nextFinishButton = new CustomButtonControl(ButtonType.PrimaryWithoutMargin);
            _prevButton = new CustomButtonControl(ButtonType.PrimaryWithoutMargin);
            ParentPage.PageLayout.RowSpacing = _padding;
            ParentPage.AddRowColumnDefinition(GridLength.Auto, 1, true);
            ParentPage.AddRowColumnDefinition(GridLength.Star, 1, true);
            ParentPage.AddRowColumnDefinition(GridLength.Auto, 2, true);
            _buttonLayout = new Grid
            {
                Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                BackgroundColor = Color.FromArgb(StyleConstants.SEPARATOR_N_DISABLED_COLOR),
                ColumnSpacing = 1,
                Padding = new Thickness(-_padding, 0),
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition{ Height = GridLength.Auto },
                    new RowDefinition{ Height = GridLength.Auto }
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                   new ColumnDefinition { Width = GridLength.Star },
                   new ColumnDefinition { Width = GridLength.Star }
                },
            };
            _progressBar = new CustomProgressBarControl
            {
                Progress = 0,
            };
            _buttonLayout.Add(_progressBar, 0, 0);
            Grid.SetColumnSpan(_progressBar, 2);
            _buttonLayout.Add(_prevButton, 0, 1);
            _buttonLayout.Add(_nextFinishButton, 1, 1);
            _roleID = App._essentials.GetPreferenceValue(StorageConstants.PR_ROLE_ID_KEY, 0);
            if (_roleID == (int)RoleName.Patient || _roleID == (int)RoleName.CareTaker)
            {
                SetPageContent(ParentPage.PageLayout);
            }
        }

        public override async Task LoadUIAsync(bool isRefreshRequest)
        {
            if (!isRefreshRequest)
            {
                _questionnaireData.PatientTaskID = GenericMethods.MapValueType<string>(GetParameterValue(nameof(QuestionnaireDTO.PatientTaskID)));
                _questionnaireData.Questionnaire = new QuestionnaireModel
                {
                    QuestionnaireID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(TaskModel.ItemID)))
                };
                _questionnaireData.SelectedUserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));
            }
            _questionnaireData.LanguageID = (byte)App._essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
            await (ParentPage.PageService as QuestionnaireService).GetQuestionnaireAsync(_questionnaireData).ConfigureAwait(true);
            ParentPage.PageData = ParentPage.PageService.PageData;
            if (_questionnaireData.ErrCode == ErrorCode.OK)
            {
                if (_questionnaireData.QuestionnaireQuestionAnswer != null && _questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID > 0)
                {
                    _previousQuestionID = _questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID;
                }
                RoleName loggedInUserRole = (RoleName)_roleID;
                // If (Questionnaire) DefaultRespondent is Provider then patient is not allowed to answer and if (Questionnaire) DefaultRespondent is Patient then provider is not allowed to answer
                if (((loggedInUserRole == RoleName.Patient || loggedInUserRole == RoleName.CareTaker)
                    && _questionnaireData.Questionnaire.DefaultRespondentID != (short)ReadingAddedBy.ProviderKey)
                    || ((loggedInUserRole == RoleName.Doctor || loggedInUserRole == RoleName.Nurse || loggedInUserRole == RoleName.FrontDesk)
                    && _questionnaireData.Questionnaire.DefaultRespondentID != (short)ReadingAddedBy.PatientKey))
                {
                    await SetTitleViewAsync().ConfigureAwait(true);
                    await RenderUIAccordingToQuestionnairActionAsync().ConfigureAwait(true);
                    _nextFinishButton.Clicked += OnButtonsClicked;
                    _prevButton.Clicked += OnButtonsClicked;
                }
                else
                {
                    string messageToDisplay = string.Format(CultureInfo.InvariantCulture, ParentPage.GetResourceValueByKey(ResourceConstants.R_CANNOT_CONTINUE_TASK_MESSAGE_KEY),
                        ParentPage.GetResourceValueByKey(loggedInUserRole == RoleName.Patient ?
                        ResourceConstants.R_PROVIDER_KEY : ResourceConstants.R_PATIENT_KEY));
                    await ParentPage.DisplayMessagePopupAsync(messageToDisplay, OnClosePopoUp, false, true, true).ConfigureAwait(false);
                }
            }
            else
            {
                await ParentPage.DisplayMessagePopupAsync(_questionnaireData.ErrCode.ToString(), OnClosePopoUp, false, true, false).ConfigureAwait(false);
            }
            AppHelper.ShowBusyIndicator = false;
        }

        public override async Task UnloadUIAsync()
        {
            if (App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
            {
                return;
            }
            _nextFinishButton.Clicked -= OnButtonsClicked;
            _prevButton.Clicked -= OnButtonsClicked;
            DetachEvents();
            await Task.CompletedTask;
        }

        public string GetPageTitle()
        {
            return _questionnaireData.QuestionnaireDetail.CaptionText;
        }

        private async Task RenderUIAccordingToQuestionnairActionAsync()
        {
            ParentPage.PageLayout.Children.Clear();
            if (_questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.StartQuestionnaire
                || _questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.Done)
            {
                if (_startAndDoneView.OnActionClicked != null)
                {
                    _startAndDoneView.OnActionClicked -= OnMessgeViewActionClicked;
                }
                GenerateStartOrDoneView();
                ParentPage.PageLayout.Add(_startAndDoneView, 0, 0);
                Grid.SetRowSpan(_startAndDoneView, 4);
            }
            else
            {
                DetachEvents();
                _answerView.Content = null;
                ParentPage.PageLayout.Add(_answerScroll, 0, 1);
                ParentPage.PageLayout.Add(_questionContentView, 0, 0);
                ParentPage.PageLayout.Add(_helpView, 0, 2);
                _questionContentView.Text = _questionnaireData.Question.IsRequired
                    ? string.Concat(_questionnaireData.QuestionDetail.CaptionText.Split(new[] { Constants.HTML_RICH_TEXT_END_TAG }, StringSplitOptions.None)[0], Constants.RED_ASTRICK_HTML, Constants.HTML_RICH_TEXT_END_TAG)
                    : _questionnaireData.QuestionDetail.CaptionText;
                await AddControlsInLayoutAsync().ConfigureAwait(true);
                ParentPage.PageLayout.Add(_buttonLayout, 0, 3);
                RenderUIWithData();
            }
        }

        private void RenderUIWithData()
        {
            _prevButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_PREVIOUS_ACTION_KEY);
            _prevButton.CommandParameter = ResourceConstants.R_PREVIOUS_ACTION_KEY;
            RenderHelpText();
            CalculateProgressBar();
            EnableDisableControls();
        }

        private void RenderHelpText()
        {
            if (string.IsNullOrWhiteSpace(_questionnaireData.QuestionDetail.InstructionsText) || _questionnaireData.Question.QuestionTypeID == QuestionType.RichTextQuestionKey)
            {
                _helpView.IsVisible = false;
            }
            else
            {
                _helpView.SetInfoValue(_questionnaireData.QuestionDetail.InstructionsText, ParentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SHOW_MORE_KEY).ResourceValue,
                ParentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SHOW_LESS_KEY).ResourceValue, Convert.ToInt32(ParentPage.GetSettingsValueByKey(SettingsConstants.S_INSTRUCTION_TEXT_LENGTH_KEY), CultureInfo.InvariantCulture));
                _helpView.IsVisible = true;
            }
        }

        private void CalculateProgressBar()
        {
            if (_questionnaireData.Questionnaire.NoOfQuestions > 0)
            {
                _progressBar.Progress = _questionnaireData.NumberOfQuestionAnswer.Value / (double)_questionnaireData.Questionnaire.NoOfQuestions;
            }
        }

        private void EnableDisableControls()
        {
            _nextFinishButton.IsEnabled = true;
            _nextFinishButton.IsVisible = true;
            switch (_questionnaireData.Questionnaire.QuestionnaireAction)
            {
                case QuestionnaireAction.Finish:
                    UpdateNextButton(false, ResourceConstants.R_FINISH_ACTION_KEY);
                    break;
                case QuestionnaireAction.Next:
                    UpdateNextButton(false, ResourceConstants.R_NEXT_ACTION_KEY);
                    break;
                case QuestionnaireAction.PreviousAndNext:
                    UpdateNextButton(!_questionnaireData.Question.IsStartingQuestion, ResourceConstants.R_NEXT_ACTION_KEY);
                    break;
                case QuestionnaireAction.PreviousAndFinish:
                    UpdateNextButton(true, ResourceConstants.R_FINISH_ACTION_KEY);
                    break;
                default:
                    // future implementation
                    break;
            }
        }

        private async Task SetTitleViewAsync()
        {
            if (!IsPatientPage() && DeviceInfo.Idiom == DeviceIdiom.Phone)
            {
                await ParentPage.OverrideTitleViewAsync(new MenuView(MenuLocation.Header, _questionnaireData.QuestionnaireDetail.CaptionText, ShellMasterPage.CurrentShell.CurrentPage.IsAddEditPage)).ConfigureAwait(true);
            }
        }

        private void UpdateNextButton(bool isPrevButtonEnabled, string resourceKey)
        {
            _prevButton.IsEnabled = isPrevButtonEnabled;
            _nextFinishButton.CommandParameter = resourceKey;
            _nextFinishButton.Text = ParentPage.GetResourceValueByKey(resourceKey);
        }

        private async Task GetQuestionnaireAsync(string questionnaireAction)
        {
            AppHelper.ShowBusyIndicator = true;
            await (ParentPage.PageService as QuestionnaireService).GetQuestionAsync(_questionnaireData, questionnaireAction).ConfigureAwait(true);
            if (_questionnaireData.ErrCode == ErrorCode.OK)
            {
                await ParentPage.SyncDataWithServerAsync(Pages.QuestionnaireTaskPage, false, default).ConfigureAwait(true);
                await RenderUIAccordingToQuestionnairActionAsync().ConfigureAwait(true);
            }
            else
            {
                await ParentPage.DisplayMessagePopupAsync(_questionnaireData.ErrCode.ToString(), ParentPage.OnClosePupupAction, false, true, false).ConfigureAwait(false);
            }
            AppHelper.ShowBusyIndicator = false;
        }

        private void GenerateStartOrDoneView()
        {
            OptionModel[] action;
            if (_questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.StartQuestionnaire)
            {
                _startAndDoneView.ControlResourceKey = QuestionnaireAction.StartQuestionnaire.ToString();
                action = new[] {
                        new OptionModel { GroupName = ButtonType.PrimaryWithMargin.ToString(), OptionText = ResourceConstants.R_START_ACTION_KEY, SequenceNo = 1 },
                    };
                _startAndDoneView.StyleId = ResourceConstants.R_START_ACTION_KEY;
            }
            else
            {
                _startAndDoneView.ControlResourceKey = QuestionnaireAction.Done.ToString();
                action = new[] {
                        new OptionModel { GroupName = ButtonType.PrimaryWithMargin.ToString(), OptionText = ResourceConstants.R_OK_ACTION_KEY, SequenceNo = 1 },
                    };
                _startAndDoneView.StyleId = ResourceConstants.R_OK_ACTION_KEY;
            }
            _startAndDoneView.PageResources = ParentPage.PageData;
            _startAndDoneView.Actions = action;
            _startAndDoneView.OnActionClicked += OnMessgeViewActionClicked;
        }

        private async void OnMessgeViewActionClicked(object sender, int e)
        {
            await ButtonClickedAsync(_startAndDoneView.StyleId);
        }

        #region logical

        private async void OnButtonsClicked(object sender, EventArgs e)
        {
            _nextFinishButton.Clicked -= OnButtonsClicked;
            _prevButton.Clicked -= OnButtonsClicked;
            await ButtonClickedAsync(((CustomButton)sender).CommandParameter.ToString());
            _nextFinishButton.Clicked += OnButtonsClicked;
            _prevButton.Clicked += OnButtonsClicked;
        }

        private async Task ButtonClickedAsync(string key)
        {
            if (key == ResourceConstants.R_START_ACTION_KEY)
            {
                await GetQuestionnaireAsync(key).ConfigureAwait(true);
                AnimateContent(true);
            }
            else if (key == ResourceConstants.R_PREVIOUS_ACTION_KEY)
            {
                _questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID = _previousQuestionID;
                if (_questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID > 0)
                {
                    await GetQuestionnaireAsync(key).ConfigureAwait(true);
                    if (_questionnaireData.QuestionnaireQuestionAnswer?.PreviousQuestionID != 0)
                    {
                        _previousQuestionID = _questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID;
                        AnimateContent(false);
                    }
                }
                else
                {
                    await GetQuestionnaireAsync(key).ConfigureAwait(true);
                }
            }
            else if (key == ResourceConstants.R_NEXT_ACTION_KEY || key == ResourceConstants.R_FINISH_ACTION_KEY)
            {

                await GetNextQuestionAsync().ConfigureAwait(true);

            }
            else if (key == ResourceConstants.R_OK_ACTION_KEY)
            {
                await NavigateToPreviosPageAsync().ConfigureAwait(true);
            }
            else
            {
                // for future implementation
            }
        }

        private async Task NavigateToPreviosPageAsync()
        {
            if (MobileConstants.IsTablet)
            {
                InvokeListRefresh(ErrorCode.OK.ToString(), new EventArgs());
            }
            else
            {
                await ParentPage.PopPageAsync(true).ConfigureAwait(true);
            }
        }

        private void DetachEvents()
        {
            try
            {
                if (_answerViewContent != null)
                {
                    //Detach Selection Change event , and it will atach again after loading data in radio list
                    if (_answerViewContent is CustomRadioTextList && _answerViewContent.GetType() == typeof(CustomRadioTextList))
                    {
                        (_answerViewContent as CustomRadioTextList).OnSelectionChanged -= Control_OnSelectionChanged;
                    }
                    if (_answerViewContent is Grid && ((Grid)_answerViewContent).Children.Count > 1 && ((Grid)_answerViewContent).Children[1].GetType() == typeof(CustomListView))
                    {
                        OnFileUpdated -= RefreshFilesAsync;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private async Task GetNextQuestionAsync()
        {
            await GetCurrentQuestionAnswer().ConfigureAwait(true);
            if (((_questionnaireData.Question.QuestionTypeID == QuestionType.HorizontalSliderQuestionKey || _questionnaireData.Question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey ||
                _questionnaireData.Question.QuestionTypeID == QuestionType.VerticalSliderQuestionKey || _questionnaireData.Question.QuestionTypeID == QuestionType.SingleSelectQuestionKey ||
                _questionnaireData.Question.QuestionTypeID == QuestionType.MultiSelectQuestionKey || _questionnaireData.Question.QuestionTypeID == QuestionType.MeasurementQuestionKey)
                && _questionnaireData.Question.IsRequired && string.IsNullOrWhiteSpace(_questionnaireData.QuestionnaireQuestionAnswer.AnswerValue)))
            {
                if (!string.IsNullOrWhiteSpace(_readingDateTimeErrorMessage))
                {
                    DisplayReadingDateErrorLabel(_readingDateTimeErrorMessage);
                }
                else if (MobileConstants.IsDevicePhone)
                {
                    ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(ResourceConstants.R_QUESTIONNAIRE_MANDATORY_ANSWER_ERROR_KEY));
                }
                else
                {
                    InvokeListRefresh(ResourceConstants.R_QUESTIONNAIRE_MANDATORY_ANSWER_ERROR_KEY, null);
                }
            }
            else
            {
                if (ParentPage.IsFormValid(_controls) && _allowNext)
                {
                    await NavigateToNextQuestionAsync().ConfigureAwait(true);
                }
            }
        }

        private async Task NavigateToNextQuestionAsync()
        {
            if (_questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID > 0 && _previousQuestionID == 0)
            {
                _previousQuestionID = _questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID;
            }
            else
            {
                _previousQuestionID = _questionnaireData.QuestionnaireQuestionAnswer.QuestionID;
            }
            await GetQuestionnaireAsync(_questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.PreviousAndFinish ? ResourceConstants.R_FINISH_ACTION_KEY : ResourceConstants.R_NEXT_ACTION_KEY).ConfigureAwait(true);
            AnimateContent(true);
        }

        private async Task GetCurrentQuestionAnswer()
        {
            if (_controlObj == null)
            {
                return;
            }
            var dynamicControl = _questionnaireData.Question.QuestionTypeID;
            if (dynamicControl == QuestionType.FilesAndDocumentQuestionKey)
            {
                _questionnaireData.FileDocuments = new List<FileDocumentModel> { };
                _questionnaireData.FileDocuments = GetFileValue(string.Concat(Constants.FILE_LIST, Constants.SYMBOL_DASH, _questionnaireData.Question.QuestionID, _questionnaireData.LanguageID))?.ToList();
                if (GenericMethods.IsListNotEmpty(_questionnaireData.FileDocuments))
                {
                    new FileService(App._essentials).MapQuestionnaireFileData((long)_questionnaireData.Question.CategoryID, _questionnaireData.FileDocuments.First());
                    _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = _questionnaireData.FileDocuments.First().DocumentDescription;
                }
                else
                {
                    if (_questionnaireData.FileDocuments?.Count < 1 && _questionnaireData.Question.IsRequired)
                    {
                        _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = string.Empty;
                        return;
                    }
                    _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = string.Empty;
                }
                if (GenericMethods.IsListNotEmpty(_removedDocuments))
                {
                    _questionnaireData.FileDocuments.Add(_removedDocuments.First());
                }
            }
            else if (_questionnaireData.Question.QuestionTypeID == QuestionType.MeasurementQuestionKey)
            {
                var result = await (_readingDetailView as PatientReadingView).OnSaveButtonClickedAsync().ConfigureAwait(true);
                _allowNext = result.Item1;
                _readingDateTimeErrorMessage = result.Item3;
                if (!string.IsNullOrWhiteSpace(_readingDateTimeErrorMessage))
                {
                    DisplayReadingDateErrorLabel(_readingDateTimeErrorMessage);
                }
                if (result.Item2 == Guid.Empty)
                {
                    _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = string.Empty;
                }
                else
                {
                    _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = result.Item2.ToString();
                }
            }
            else
            {
                if (GetControlValuesHandler.ContainsKey(dynamicControl))
                {
                    _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = GetControlValuesHandler[dynamicControl](_controlObj);
                    if (dynamicControl == QuestionType.SingleSelectQuestionKey)
                    {
                        _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = GetSingleSelectAnswer(_questionnaireData.QuestionnaireQuestionAnswer.AnswerValue, _questionnaireData.QuestionOptions?.ToList());
                    }
                }
            }
            _questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID = _previousQuestionID;
        }

        private void DisplayReadingDateErrorLabel(string errorMessage)
        {
            if (MobileConstants.IsTablet && !IsPatientPage())
            {
                InvokeListRefresh(errorMessage, null);
            }
            else
            {
                ParentPage.DisplayOperationStatus(errorMessage);
            }
        }

        private void AnimateContent(bool isNext)
        {
            AnimationDirection animationDirection;
            animationDirection = App._essentials.GetPreferenceValue(StorageConstants.PR_IS_RIGHT_ALIGNED_KEY, false) ? AnimateTo(isNext) : AnimateTo(!isNext);
            ////DO NOT AWAIT THESE CALLS AS IT GIVES A BAD USER EXPERIENCE
            ////WE ALSO WANT Translation and FadeIn EFFECT TO HAPPEN AT THE SAME TIME
            ParentPage.PageLayout.TranslateTo((animationDirection == AnimationDirection.Left) ? -ParentPage.PageLayout.Width : ParentPage.PageLayout.Width, ParentPage.PageLayout.Y, 450, Easing.CubicOut);
            ParentPage.PageLayout.TranslationX = (animationDirection == AnimationDirection.Left) ? ParentPage.PageLayout.Width : -ParentPage.PageLayout.Width;
            ParentPage.PageLayout.FadeTo(0, 200);
            ParentPage.PageLayout.FadeTo(1, 100);
            ParentPage.PageLayout.TranslateTo(0, 0, 225, Easing.CubicIn);
        }

        private AnimationDirection AnimateTo(bool isNext)
        {
            return isNext ? AnimationDirection.Right : AnimationDirection.Left;
        }

        private async Task AddControlsInLayoutAsync()
        {
            var dynamicControl = _questionnaireData.Question.QuestionTypeID;
            _controlObj = null;
            _questionnaireData.PhoneNumber = ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY;
            _controlObj = CreateControlValuesHandler[dynamicControl](_questionnaireData);
            if (_controlObj != null)
            {
                if (dynamicControl == QuestionType.DateTimeQuestionKey || dynamicControl == QuestionType.DateQuestionKey)
                {
                    await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
                    if (!string.IsNullOrWhiteSpace(_questionnaireData.QuestionnaireQuestionAnswer.AnswerValue))
                    {
                        (_controlObj as CustomDateTimeControl).GetSetDate = App._essentials.ConvertToLocalTime(DateTime.Parse(_questionnaireData.QuestionnaireQuestionAnswer.AnswerValue, CultureInfo.InvariantCulture)).Date;
                    }
                }
                _controls.Clear();
                _controls.Add(_controlObj);
                _answerView.Content = _controlObj;
                _answerViewContent = _answerView.Content;
                await _answerScroll.ScrollToAsync(0, 0, false).ConfigureAwait(true);
                await AttachControlEventsAsync().ConfigureAwait(true);
            }
            else
            {
                // If the quetion type is not available then show the error
                if (MobileConstants.IsTablet)
                {
                    InvokeListRefresh(ErrorCode.ErrorWhileRetrievingRecords.ToString(), null);
                }
                else
                {
                    await ParentPage.DisplayMessagePopupAsync(ErrorCode.ErrorWhileRetrievingRecords.ToString(), OnPopupActionClicked).ConfigureAwait(true);
                }
                await NavigateToPreviosPageAsync().ConfigureAwait(true);
            }
        }

        private async void OnPopupActionClicked(object sender, int e)
        {
            ////todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
        }

        private async void OnClosePopoUp(object sender, int e)
        {
            ////todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
            if (MobileConstants.IsTablet)
            {
                InvokeListRefresh(ErrorCode.NotFound.ToString(), new AlphaMDHealth.Model.CustomEventArgs());
            }
            else
            {
                await ParentPage.PopPageAsync(true).ConfigureAwait(false);
            }
        }

        private async Task AttachControlEventsAsync()
        {
            if (_answerViewContent.GetType() == typeof(Grid))
            {
                var layout = (Grid)_answerViewContent;
                if (layout.Children.Count > 0)
                {
                    var child = ((Grid)_answerViewContent).Children[0];
                    if (child.GetType() == typeof(CustomMultiLineEntryControl) || child.GetType() == typeof(CustomEntryControl))
                    {
                        BaseContentView control = null;
                        switch (child.GetType().FullName)
                        {
                            case nameof(CustomMultiLineEntryControl):
                                control = (CustomMultiLineEntryControl)child;
                                break;
                            case nameof(CustomEntryControl):
                                control = (CustomEntryControl)child;
                                break;
                        }

                        control.Focused += Control_Focused;
                        control.Unfocused += Control_Unfocused;
                    }
                    if (((Grid)_answerViewContent).Children.Count > 1 && ((Grid)_answerViewContent).Children[1].GetType() == typeof(CustomListView))
                    {
                        OnFileUpdated += RefreshFilesAsync;
                    }
                }
            }
            // Automatic redirect code for Radio buttons
            if (_answerViewContent is CustomRadioTextList radioBoxList && _answerViewContent.GetType() == typeof(CustomRadioTextList))
            {
                radioBoxList.OnSelectionChanged += Control_OnSelectionChanged;
            }
            if (_answerViewContent is PatientReadingView)
            {
                await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
                await _readingDetailView.LoadUIAsync(false).ConfigureAwait(true);
            }
        }

        private async void Control_OnSelectionChanged(object sender, EventArgs e)
        {
            await GetNextQuestionAsync().ConfigureAwait(true);
        }

        private void Control_Unfocused(object sender, FocusEventArgs e)
        {
            _buttonLayout.IsVisible = true;
        }
        private void Control_Focused(object sender, FocusEventArgs e)
        {
            var view = _answerViewContent;
            if (view.GetType() == typeof(Grid))
            {
                _buttonLayout.IsVisible = false;
                var child = ((Grid)view).Children[0];
                if (child is CustomMultiLineEntryControl || child is CustomEntryControl)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await _answerScroll.ScrollToAsync((Element)child, ScrollToPosition.End, false).ConfigureAwait(true);
                    });
                }
            }
        }

        private void RefreshFilesAsync(object sender, EventArgs e)
        {
            string uniqueKey = sender?.ToString();
            List<FileDocumentModel> documents = GetFileValue(string.Concat(Constants.FILE_LIST, Constants.SYMBOL_DASH, _questionnaireData.Question.QuestionID, _questionnaireData.LanguageID))?.ToList();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (GenericMethods.IsListNotEmpty(documents))
                {
                    //todo: 
                    //((_answerViewContent as Grid).Children.FirstOrDefault(x => x.StyleId == uniqueKey) as CustomListView).Header = null;
                    //((_answerViewContent as Grid).Children.FirstOrDefault(x => x.StyleId == uniqueKey) as CustomListView).ItemsSource = documents;
                }
                else
                {
                    CustomMessageControl _emptyListView = new CustomMessageControl(false)
                    {
                        ControlResourceKey = ResourceConstants.R_DOCUMENT_FILE_EMPTY_VIEW_KEY,
                        PageResources = ParentPage.PageData,
                    };
                    //todo:
                    //((_answerViewContent as Grid).Children.FirstOrDefault(x => x.StyleId == uniqueKey) as CustomListView).Header = _emptyListView;
                    //((_answerViewContent as Grid).Children.FirstOrDefault(x => x.StyleId == uniqueKey) as CustomListView).HeightRequest = Convert.ToDouble(Application.Current.Resources[LibStyleConstants.ST_DEFAULT_CARD_HEIGHT_STYLE], CultureInfo.InvariantCulture);
                    //((_answerViewContent as Grid).Children.FirstOrDefault(x => x.StyleId == uniqueKey) as CustomListView).ItemsSource = new List<FileDocumentModel>();
                }
            });
        }
        #endregion
    }
}