using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(PatientTaskPage))]
[QueryProperty(nameof(TaskModel.PatientTaskID), "id")]
public class PatientTaskPage : BasePage
{
    private readonly PatientTaskView _tasksView;
    private int _taskID;

    /// <summary>
    /// PatientTaskid
    /// </summary>
    public string PatientTaskID
    {
        get { return _taskID.ToString(CultureInfo.InvariantCulture); }
        set
        {
            _taskID = Convert.ToInt32(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
        }
    }

    public PatientTaskPage()
    {
        _tasksView = new PatientTaskView(this, null);
        SetPageContent(true);
    }

    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _tasksView.Parameters = AddParameters(CreateParameter(nameof(TaskModel.PatientTaskID), PatientTaskID));
        await _tasksView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        await _tasksView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }
}