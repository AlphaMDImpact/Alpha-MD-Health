using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(PatientInstructionPage))]
[QueryProperty(nameof(PatientTaskID), "id")]
[QueryProperty(nameof(InstructionID), "identifier")]
public class PatientInstructionPage : BasePage
{
    private long _patientTaskID;
    private long _instructionID;
    private readonly PatientInstructionView _instructionView;

    /// <summary>
    /// Patient Task ID
    /// </summary>
    public string PatientTaskID
    {
        get { return _patientTaskID.ToString(CultureInfo.InvariantCulture); }
        set => _patientTaskID = Convert.ToInt64(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Instruction ID
    /// </summary>
    public string InstructionID
    {
        get { return _instructionID.ToString(CultureInfo.InvariantCulture); }
        set => _instructionID = Convert.ToInt64(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
    }

    public PatientInstructionPage() : base(PageLayoutType.MastersContentPageLayout, true)
    {
        //IsSplitView = MobileConstants.IsTablet;
        _instructionView = new PatientInstructionView(this, null);
        Content = _instructionView;
    }

    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _instructionView.Parameters = AddParameters(
            CreateParameter(nameof(TaskModel.PatientTaskID), _patientTaskID.ToString(CultureInfo.InvariantCulture)),
            CreateParameter(nameof(TaskModel.ItemID), _instructionID.ToString(CultureInfo.InvariantCulture))
        );
        await _instructionView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        await _instructionView.UnloadUIAsync().ConfigureAwait(true);
        HideFooter(false);
        base.OnDisappearing();
    }
}