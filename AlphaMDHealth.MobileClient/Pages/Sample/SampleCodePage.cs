//using AlphaMDHealth.Utility;

//namespace AlphaMDHealth.MobileClient;

//[RouteRegistration("SampleCodePage")]
//[QueryProperty("EmpID", "empID")]
//public class SampleCodePage : BasePage
//{
//    private string _empID;
//    private readonly SampleCodeView _sampleAddEditView;

//    public string EmpID
//    {
//        get
//        {
//            return _empID;
//        }
//        set => _empID = Uri.UnescapeDataString(value);
//    }

//    public SampleCodePage() : base(PageLayoutType.AddEditPageLayout, true)
//    {
//        _sampleAddEditView = new SampleCodeView(this);
//        Content = _sampleAddEditView;
//    }

//    protected override async void OnAppearing()
//    {
//        base.OnAppearing();
//        await _sampleAddEditView.LoadUIDataAsync(_empID).ConfigureAwait(true);
//        AppHelper.ShowBusyIndicator = false;
//    }

//    protected override void OnDisappearing()
//    {
//        base.OnDisappearing();
//        _sampleAddEditView.UnLoadUIData();
//    }
//}