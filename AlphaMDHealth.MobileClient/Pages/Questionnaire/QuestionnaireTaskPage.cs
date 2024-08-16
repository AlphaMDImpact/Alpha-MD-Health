using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration("QuestionnaireTaskPage")]
[QueryProperty(nameof(PatientTaskID), "id")]
[QueryProperty(nameof(QuestionnaireID), "identifier")]
public class QuestionnaireTaskPage : BasePage
{
    private long _patientTaskID;
    private long _questionnaireID;
    private readonly QuestionnaireTaskView _questionnaireView;

    /// <summary>
    /// Patient Task ID
    /// </summary>
    public string PatientTaskID
    {
        get { return _patientTaskID.ToString(CultureInfo.InvariantCulture); }
        set => _patientTaskID = Convert.ToInt64(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Questionnaire ID
    /// </summary>
    public string QuestionnaireID
    {
        get { return _questionnaireID.ToString(CultureInfo.InvariantCulture); }
        set => _questionnaireID = Convert.ToInt64(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
    }

    public QuestionnaireTaskPage() : base(PageLayoutType.MastersContentPageLayout, false)
    {
        //IsSplitView = MobileConstants.IsTablet;
        _questionnaireView = new QuestionnaireTaskView(this, null);
        //todo: PageLayout.Effects.Add(new CustomSafeAreaInsetEffect());
        HideFooter(true);
        SetPageContent(false);
    }

    protected override async void OnAppearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
            base.OnAppearing();
            _questionnaireView.Parameters = AddParameters(
                CreateParameter(nameof(QuestionnaireDTO.PatientTaskID), _patientTaskID.ToString(CultureInfo.InvariantCulture)),
                CreateParameter(nameof(TaskModel.ItemID), _questionnaireID.ToString(CultureInfo.InvariantCulture))
            );
            await _questionnaireView.LoadUIAsync(false).ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
        }
    }

    protected override async void OnDisappearing()
    {
        await _questionnaireView.UnloadUIAsync().ConfigureAwait(true);
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            HideFooter(false);
            base.OnDisappearing();
        }
    }
}