using Microsoft.Maui.Controls.Compatibility;
using Grid = Microsoft.Maui.Controls.Grid;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Base page to use common pages logic
/// </summary>
public partial class BasePage
{
    #region Page Field Validations

    /// <summary>
    /// Check if a form is Valid
    /// </summary>
    /// <param name="pageControls">list of control to be validated</param>
    /// <returns>Is Form Valid</returns>
    public bool IsFormValid(List<View> pageControls)
    {
        return RunValidation(pageControls);
    }

    /// <summary>
    /// Check if a form is Valid
    /// </summary>
    /// <returns>Is Form Valid</returns>
    public bool IsFormValid()
    {
        GetChildren(Content);
        PageControls = PageControls.FindAll(x => x.IsVisible);
        return RunValidation(PageControls);
    }

    /// <summary>
    /// Check if a View is Valid
    /// </summary>
    /// <param name="view">list of control to be validated</param>
    /// <returns>is View Valid</returns>
    public bool IsFormValid(View view)
    {
        GetChildren(((ContentView)view).Content);
        return RunValidation(PageControls);
    }

    /// <summary>
    /// Check if a View is Valid
    /// </summary>
    /// <param name="view">list of control to be validated</param>
    /// <returns>is View Valid</returns>
    public bool IsFormControlValid(View view)
    {
        PageControls = new List<View>();
        IList<IView> viewChild = (view as Grid).Children.ToList();
        PageControls.AddRange((IEnumerable<View>)viewChild);
        foreach (View child in viewChild)
        {
            GetChildren(child);
        }
        return RunValidation(PageControls);
    }

    private void GetChildren(View view)
    {
        if (view is Layout<View> layout)
        {
            List<View> viewChildren = layout.Children.ToList();
            PageControls.AddRange(viewChildren);
            foreach (View child in viewChildren)
            {
                GetChildren(child);
            }
        }
        else if (view.GetType() == typeof(ContentView))
        {
            View viewChild = ((ContentView)view).Content;
            PageControls.Add(viewChild);
            GetChildren(viewChild);
        }
        else if (view.GetType() == typeof(ScrollView))
        {
            View viewChild = ((ScrollView)view).Content;
            PageControls.Add(viewChild);
            GetChildren(viewChild);
        }
        else if (view.GetType() == typeof(Frame))
        {
            View viewChild = ((Frame)view).Content;
            PageControls.Add(viewChild);
            GetChildren(viewChild);
        }
        else if (view.GetType() == typeof(Grid))
        {
            //List<View> viewChild = (view as Grid).Children.ToList();
            //PageControls.AddRange(viewChild);
            foreach (View child in (view as Grid).Children.ToList())
            {
                PageControls.Add(child);
            }
            foreach (View child in (view as Grid).Children.ToList())
            {
                GetChildren(child);
            }
        }
        //else if (view.GetType() == typeof(CustomGradient))
        //{
        //    var viewChild = ((CustomGradient)view).Content;
        //    PageControls.Add(viewChild);
        //    GetChildren(viewChild);
        //}
        //todo:
        //else if (view.GetType() == typeof(PancakeView))
        //{
        //    View viewChild = ((PancakeView)view).Content;
        //    PageControls.Add(viewChild);
        //    GetChildren(viewChild);
        //}
        else
        {
            //To be added
        }
    }

    /// <summary>
    /// Run Validation
    /// </summary>
    /// <param name="pageControls">list of controls to be validated</param>
    /// <returns>return isValid</returns>
    public bool RunValidation(List<View> pageControls)
    {
        bool isFormValid = true;
        foreach (View control in pageControls)
        {
            AmhBaseControl customControl = null;
            switch (control.GetType().Name)
            {
                case nameof(AmhEntryControl):
                    customControl = (AmhEntryControl)control;
                    break;
                case nameof(AmhMultilineEntryControl):
                    customControl = (AmhMultilineEntryControl)control;
                    break;
                case nameof(AmhNumericEntryControl):
                    customControl = (AmhNumericEntryControl)control;
                    break;
                case nameof(AmhMobileNumberControl):
                    customControl = (AmhMobileNumberControl)control;
                    break;
                case nameof(AmhDateTimeControl):
                    customControl = (AmhDateTimeControl)control;
                    break;
                case nameof(AmhCheckBoxControl):
                    customControl = (AmhCheckBoxControl)control;
                    break;
                case nameof(AmhRadioButtonControl):
                    customControl = (AmhRadioButtonControl)control;
                    break;
                case nameof(AmhColorPickerControl):
                    customControl = (AmhColorPickerControl)control;
                    break;
                case nameof(AmhSingleSelectDropDownControl):
                    customControl = (AmhSingleSelectDropDownControl)control;
                    break;
                case nameof(AmhMultiSelectDropDownControl):
                    customControl = (AmhMultiSelectDropDownControl)control;
                    break;
                case nameof(AmhSliderControl):
                    customControl = (AmhSliderControl)control;
                    break;
                case nameof(AmhUploadControl):
                    customControl = (AmhUploadControl)control;
                    break;
                case nameof(AmhSwitchControl):
                    customControl = (AmhSwitchControl)control;
                    break;
                default:
                    //will use for future implementation
                    break;
            }
            isFormValid = AddMyValicationBasedOnEnabledControled(isFormValid, customControl);
        }
        return isFormValid;
    }

    private bool AddMyValicationBasedOnEnabledControled(bool isFormValid, AmhBaseControl customControl)
    {
        if (customControl != null && customControl.IsEnabled)
        {
            customControl.ValidateControl(true);
            isFormValid = isFormValid && customControl.IsValid;
        }

        return isFormValid;
    }

    #endregion

    ///// <summary>
    ///// Used to create and attach sync status view
    ///// </summary>
    //public void CreateSyncStatusView()
    //{
    //    if (SyncStatusView != null)
    //    {
    //        MasterGrid.Children.Remove(SyncStatusView);
    //    }
    //    SyncStatusView = new SyncStatusView();
    //    MasterGrid.Add(SyncStatusView, 0, 0);
    //}

    ///// <summary>
    ///// Start or stop sync in progress loading indicator based on current sync status
    ///// </summary>
    //public void ShowSyncProgress()
    //{
    //    SyncStatusView?.ShowSyncProgress();
    //}

    /// <summary>
    /// Dispose objects
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose objects
    /// </summary>
    /// <param name="disposing">object which indicates disposing or not</param>
    protected virtual void Dispose(bool disposing)
    {
        // Cleanup
        //SyncStatusView?.Dispose();
    }

    /// <summary>
    /// ResetView landscepe variable to reset view
    /// </summary>
    public void SetOrientation()
    {
        _isLandscape = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Landscape;
    }
}