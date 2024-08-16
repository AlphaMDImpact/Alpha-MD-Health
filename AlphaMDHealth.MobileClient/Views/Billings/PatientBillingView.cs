using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientBillingView : ViewManager
{
    private readonly CustomBindablePickerControl _programPicker;
    private readonly CustomBindablePickerControl _programCaregiverPicker;
    private readonly CustomDateTimeControl _billingDate;
    private readonly CustomEntryControl _billingTotalAmount;
    private readonly CustomEntryControl _billingAmountPaid;
    private readonly CustomEntryControl _billingDiscount;
    private readonly CustomBindablePickerControl _paymentMode;
    private readonly CustomButtonControl _deleteBill;
    private readonly CustomButtonControl _addItem;
    private readonly CustomButtonControl _shareBill;
    private CustomBindablePickerControl _billItemPicker;
    private readonly CustomLabelControl _addItemError;
    private readonly Grid _mainLayout;
    private readonly Grid _itemLayout;
    private readonly BillingItemDTO _billingsData = new BillingItemDTO { PatientBillItem = new PatientBillModel(), PatientBillItems = new List<PatientBillItemModel>() };
    private bool _isDeleteClicked = false;
    public bool _isProgramDeleted { get; set; } = false;
    private CustomEntryControl _billingAmount;
    private CustomLabelControl _removeButton;
    //private List<OptionModel> _filterProviders = new List<OptionModel>();
    //private List<OptionModel> _filterBillingItems = new List<OptionModel>();
    private int _previousSelectedProgramIndex = -1;
    /// <summary>
    /// Onsuccess save Event
    /// </summary>
    public event EventHandler<EventArgs> OnSaveSuccess;

    public PatientBillingView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new PatientBillService(App._essentials);
        _billingDate = new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateControl,
            ControlResourceKey = ResourceConstants.R_ENTER_DATE_KEY,
        };
        _programPicker = CreateDropDown(ResourceConstants.R_SELECT_PROGRAM_KEY, string.Empty);
        _programCaregiverPicker = CreateDropDown(ResourceConstants.R_DOCTOR_NAME_KEY, string.Empty);
        _billingTotalAmount = CreateDisabledEntry(ResourceConstants.R_TOTAL_AMOUNT_KEY, string.Empty);
        _billingAmountPaid = CreateDisabledEntry(ResourceConstants.R_AMOUNT_PAID_KEY, string.Empty);
        _billingDiscount = new CustomEntryControl
        {
            ControlType = FieldTypes.DecimalEntryControl,
            ControlResourceKey = ResourceConstants.R_DISCOUNT_KEY,
        };
        _paymentMode = CreateDropDown(ResourceConstants.R_PAYMENT_MODE_NAME_KEY, string.Empty);
        _addItem = new CustomButtonControl(ButtonType.TabButton);
        _deleteBill = new CustomButtonControl(ButtonType.DeleteWithoutMargin) { IsVisible = false };
        _addItemError = new CustomLabelControl(LabelType.ClientErrorLabel) { IsVisible = false };
        _shareBill = new CustomButtonControl(ButtonType.TabButton) { IsVisible = false, VerticalOptions = LayoutOptions.End, HeightRequest = 50 };
        _mainLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            ColumnSpacing = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture),
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
            },
            ColumnDefinitions =
            {
                new ColumnDefinition {Width = GridLength.Star},
                new ColumnDefinition {Width = GridLength.Star}
            }
        };
        _itemLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            ColumnSpacing = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture),
            RowDefinitions = { new RowDefinition { Height = GridLength.Auto } },
            ColumnDefinitions =
            {
                new ColumnDefinition {Width = GridLength.Star},
                new ColumnDefinition {Width = GridLength.Star},
                new ColumnDefinition {Width = GridLength.Auto}
            }
        };

        _mainLayout.Add(_programPicker, 0, 0);
        Grid.SetColumnSpan(_programPicker, 2);
        _mainLayout.Add(_programCaregiverPicker, 0, 1);
        Grid.SetColumnSpan(_programCaregiverPicker, 2);
        _mainLayout.Add(_billingDate, 0, 2);
        Grid.SetColumnSpan(_billingDate, 2);
        _mainLayout.Add(_itemLayout, 0, 3);
        Grid.SetColumnSpan(_itemLayout, 2);
        _mainLayout.Add(_addItemError, 0, 4);
        Grid.SetColumnSpan(_addItemError, 2);
        _mainLayout.Add(_addItem, 0, 5);
        Grid.SetColumnSpan(_addItem, 2);
        _mainLayout.Add(_billingTotalAmount, 0, 6);
        Grid.SetColumnSpan(_billingTotalAmount, 2);
        _mainLayout.Add(_billingDiscount, 0, 7);
        Grid.SetColumnSpan(_billingDiscount, 2);
        _mainLayout.Add(_billingAmountPaid, 0, 8);
        Grid.SetColumnSpan(_billingAmountPaid, 2);
        _mainLayout.Add(_paymentMode, 0, 9);
        Grid.SetColumnSpan(_paymentMode, 2);
        _mainLayout.Add(_deleteBill, 0, 10);
        _mainLayout.Add(_shareBill, 1, 10);
        Content = _mainLayout;
    }

    public async override Task LoadUIAsync(bool isRefreshRequest)
    {
        _billingsData.PatientBillItem.PatientBillID = Guid.Parse(GenericMethods.MapValueType<string>(GetParameterValue(nameof(PatientBillModel.PatientBillID))));
        _billingsData.SelectedUserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));

        await (ParentPage.PageService as PatientBillService).GetPatientBillingDataAsync(_billingsData).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_billingsData.ErrCode == ErrorCode.OK)
        {
            await AssignControlResources();
        }
        else
        {
            OnSaveSuccess?.Invoke(_billingsData.ErrCode.ToString(), new EventArgs());
            //todo:await Navigation.PopAllPopupAsync();
        }
    }

    private async Task AssignControlResources()
    {
        _programPicker.PageResources = ParentPage.PageData;
        _programCaregiverPicker.PageResources = ParentPage.PageData;
        _billingDate.PageResources = ParentPage.PageData;
        _billingTotalAmount.PageResources = ParentPage.PageData;
        _billingDiscount.PageResources = ParentPage.PageData;
        _billingDiscount.OnAdvanceEntryUnfocused += OnDiscount_OnCustomEntryTextChanged;
        _paymentMode.PageResources = ParentPage.PageData;
        _billingAmountPaid.PageResources = ParentPage.PageData;
        _deleteBill.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_DELETE_ACTION_KEY);
        _addItem.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
        _shareBill.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_PRINT_KEY);
        _addItem.Clicked += AddItem_Clicked;
        _programPicker.SelectedValuesChanged += OnProgramPicker_SelectedValuesChanged;
        _programCaregiverPicker.OnAdvancePickerUnfocused += ProgramCaregiverPicker_SelectedValuesChanged;
        _paymentMode.ItemSource = _billingsData.PaymentModeOptionList;
        //_filterProviders = _billingsData.PatientProvidersOptionList;
        //_filterBillingItems = _billingsData.PatientBillingItems;
        if (_billingsData.PatientBillItem.PatientBillID != Guid.Empty)
        {
            _programPicker.ItemSource = _billingsData.PatientProgramOptionList;
            _programPicker.SelectedValue = _billingsData?.PatientProgramOptionList?.FirstOrDefault(x => x.OptionID == _billingsData?.PatientBillItem?.ProgramID)?.OptionID ?? 0;
            _itemLayout.Children.Clear();
            foreach (var item in _billingsData.PatientBillItems.Select((value, i) => new { i, value }))
            {
                var condtionData = item.value;
                var index = item.i;
                AddItemInUI(index, out CustomBindablePickerControl patientBillingItemPicker, out CustomEntryControl billingAmount, out CustomLabelControl removeButton);
                patientBillingItemPicker.SelectedValuesChanged += PatientBillingItemPicker_SelectedValuesChanged;
                billingAmount.Value = Convert.ToString(item.value.Amount, CultureInfo.InvariantCulture);
                patientBillingItemPicker.ItemSource = _billingsData.PatientBillingItems.Where(x => x.GroupName == _billingsData.PatientBillItem.ProgramID.ToString() || x.OptionID == -1).ToList();
                removeButton.StyleId = condtionData.BillingItemID.ToString();
                patientBillingItemPicker.SelectedValue = _billingsData?.PatientBillingItems?.FirstOrDefault(x => x.OptionID == condtionData?.BillingItemID)?.OptionID ?? 0;
            }
            var programData = _billingsData.PatientProgramOptionList.Find(x => x.OptionID == _billingsData.PatientBillItem.ProgramID);
            if (programData != null)
            {
                _programPicker.SelectedValue = programData.OptionID;
                _isProgramDeleted = !programData.IsDisabled;
            }
            _programPicker.SelectedValue = _billingsData?.PatientProgramOptionList?.FirstOrDefault(x => x.OptionID == _billingsData?.PatientBillItem?.ProgramID)?.OptionID ?? 0;
            _programCaregiverPicker.SelectedValue = _billingsData?.PatientProvidersOptionList?.FirstOrDefault(x => x.OptionID == _billingsData?.PatientBillItem?.ProviderID)?.OptionID ?? 0;
            await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
            _billingDate.GetSetDate = _billingsData.PatientBillItem.BillDateTime?.DateTime;
            _billingTotalAmount.Value = Convert.ToString(_billingsData.PatientBillItem.GrossTotal, CultureInfo.InvariantCulture);
            _billingDiscount.Value = _billingsData.PatientBillItem.Discount <= 0 ? default : Convert.ToString(_billingsData.PatientBillItem.Discount, CultureInfo.InvariantCulture);
            _billingAmountPaid.Value = Convert.ToString(_billingsData.PatientBillItem.TotalPaid, CultureInfo.InvariantCulture);
            _paymentMode.SelectedValue = _billingsData.PaymentModeOptionList.FirstOrDefault(x => x.OptionID == _billingsData.PatientBillItem.PaymentModeID).OptionID;
            _programPicker.IsEnabled = false;
             //Disable controls if program is removed for that patient
            _programCaregiverPicker.IsEnabled = !_isProgramDeleted;
            _billingDate.IsEnabled = !_isProgramDeleted;
            _billingDiscount.IsEnabled = !_isProgramDeleted;
            _deleteBill.IsEnabled = !_isProgramDeleted;
            _paymentMode.IsEnabled = !_isProgramDeleted;
            _addItem.IsEnabled = !_isProgramDeleted;
            _itemLayout.IsEnabled = !_isProgramDeleted;
            if (ParentPage.CheckFeaturePermissionByCode(AppPermissions.PatientBillDelete.ToString()))
            {
                _deleteBill.IsVisible = true;
                _deleteBill.Clicked += DeleteBill_Clicked;
            }
            if (ParentPage.CheckFeaturePermissionByCode(AppPermissions.PatientBillShare.ToString()))
            {
                _shareBill.IsVisible = true;
                _shareBill.Clicked += ShareBill_Clicked;
            }
        }
        else
        {
            _programPicker.ItemSource = _billingsData.PatientProgramOptionList.FindAll(x => x.IsDisabled);
            await AddDefualtAddedRow();
        }
    }

    private async Task AddDefualtAddedRow()
    {
        _billItemPicker = CreateDropDown(ResourceConstants.R_BILLING_ITEM_NAME_KEY, string.Concat(ResourceConstants.R_BILLING_ITEM_NAME_KEY, Constants.SYMBOL_DOT_STRING, "0"));
        _billingAmount = CreateDisabledEntry(ResourceConstants.R_AMOUNT_KEY, string.Concat(ResourceConstants.R_AMOUNT_KEY, Constants.SYMBOL_DOT_STRING, "0"));
        _removeButton = new CustomLabelControl(LabelType.RemoveLabelLeft) { ClassId = "0", StyleId = "0" };
        _itemLayout.Add(_billItemPicker, 0, 0);
        _itemLayout.Add(_billingAmount, 1, 0);
        _itemLayout.Add(_removeButton, 2, 0);
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        _billingDate.GetSetDate = App._essentials.ConvertToLocalTime(DateTime.UtcNow).DateTime;
        _billItemPicker.SelectedValuesChanged += PatientBillingItemPicker_SelectedValuesChanged;
        _billItemPicker.PageResources = ParentPage.PageData;
        _billingAmount.PageResources = ParentPage.PageData;
        _removeButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_DELETE_ACTION_KEY);
        TapGestureRecognizer removeTabButton = new TapGestureRecognizer();
        removeTabButton.Tapped += TapGestureRecognizer_Tapped;
        _removeButton.GestureRecognizers.Add(removeTabButton);
    }

    private void ProgramCaregiverPicker_SelectedValuesChanged(object sender, EventArgs e)
    {
        CustomBindablePicker picker = (CustomBindablePicker)sender;
        if (picker.SelectedIndex == -1)
        {
            picker.SelectedItem = null;
            picker.SelectedItem = -1;
            picker.Title = null;
            picker.Title = ParentPage.GetResourceValueByKey(ResourceConstants.R_DOCTOR_NAME_KEY);
        }
    }

    private async void OnProgramPicker_SelectedValuesChanged(object sender, EventArgs e)
    {
        CustomBindablePicker picker = (CustomBindablePicker)sender;
        if (picker.SelectedIndex != -1)
        {
            if (_previousSelectedProgramIndex > -1 && _previousSelectedProgramIndex != picker.SelectedIndex)
            {
                _programCaregiverPicker.SelectedValue = 0;
                _itemLayout.Children.Clear();
                await AddDefualtAddedRow();
            }
            _previousSelectedProgramIndex = picker.SelectedIndex;
            _billingTotalAmount.Value = string.Empty;
            _billingDiscount.Value = string.Empty;
            _billingAmountPaid.Value = string.Empty;
            //_billingsData.PatientProvidersOptionList = _filterProviders.Where(x => x.GroupName == _programPicker.SelectedValue.ToString() || x.OptionID == -1).ToList();
            //_billingsData.PatientBillingItems = _filterBillingItems.Where(x => x.GroupName == _programPicker.SelectedValue.ToString() || x.OptionID == -1).ToList();
            _programCaregiverPicker.ItemSource = _billingsData.PatientProvidersOptionList;
            if (_billItemPicker != null)
            {
                _billItemPicker.ItemSource = _billingsData.PatientBillingItems;
            }
        }
    }

    private void OnDiscount_OnCustomEntryTextChanged(object sender, EventArgs e)
    {
        CalculateAmountPaid();
    }

    private void CalculateAmountPaid()
    {
        if (!string.IsNullOrWhiteSpace(_billingTotalAmount.Value))
        {
            var discountValue = string.IsNullOrWhiteSpace(_billingDiscount.Value) ? 0 : Convert.ToDouble(_billingDiscount.Value, CultureInfo.InvariantCulture);
            _billingAmountPaid.Value = (Convert.ToDouble(_billingTotalAmount.Value, CultureInfo.InvariantCulture) - discountValue).ToString();
        }
    }

    private void AddItemInUI(int index, out CustomBindablePickerControl patientBillingItemPicker, out CustomEntryControl billingAmount, out CustomLabelControl removeButton)
    {
        _itemLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        patientBillingItemPicker = CreateDropDown(ResourceConstants.R_BILLING_ITEM_NAME_KEY, string.Concat(ResourceConstants.R_BILLING_ITEM_NAME_KEY, Constants.SYMBOL_DOT_STRING, index));
        billingAmount = CreateDisabledEntry(ResourceConstants.R_AMOUNT_KEY, string.Concat(ResourceConstants.R_AMOUNT_KEY, Constants.SYMBOL_DOT_STRING, index));
        removeButton = new CustomLabelControl(LabelType.RemoveLabelLeft) { Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_DELETE_ACTION_KEY), ClassId = index.ToString(), StyleId = index.ToString() };
        TapGestureRecognizer removeTabButton = new TapGestureRecognizer();
        removeTabButton.Tapped += TapGestureRecognizer_Tapped;
        removeButton.GestureRecognizers.Add(removeTabButton);
        _itemLayout.Add(patientBillingItemPicker, 0, index);
        _itemLayout.Add(billingAmount, 1, index);
        _itemLayout.Add(removeButton, 2, index);
        patientBillingItemPicker.PageResources = ParentPage.PageData;
        patientBillingItemPicker.ItemSource = _billingsData.PatientBillingItems;
        billingAmount.PageResources = ParentPage.PageData;
    }

    private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        var button = sender as CustomLabelControl;
        bool userNotClickEmptyRecord = true;
        //todo: 
        //foreach (var child in _itemLayout.Children.ToList().Where(child => Grid.GetRow(child) == Grid.GetRow(button)))
        //{
        //    if (child is CustomBindablePickerControl && ((child as CustomBindablePickerControl).SelectedValue <= 0 && !(child as CustomBindablePickerControl).IsValid))
        //    {
        //        userNotClickEmptyRecord = false;
        //    }
        //    _itemLayout.Children.Remove(child);
        //}
        var deleteItem = _billingsData.PatientBillItems.FirstOrDefault(x => x.BillingItemID == Convert.ToInt16(button.StyleId) && x.IsActive);
        /*if(deleteItem != null)
        {
            userNotClickEmptyRecord = true;
        }*/
        if (deleteItem != null && userNotClickEmptyRecord)
        {
            _billingsData.PatientBillItems.Remove(deleteItem);
            deleteItem.IsActive = false;
            _billingsData.PatientBillItems.Add(deleteItem);
            _billingTotalAmount.Value = _billingsData.PatientBillItems.Where(x => x.IsActive).Select(x => x.Amount).Sum().ToString();
            CalculateAmountPaid();
        }
    }

    private CustomBindablePickerControl CreateDropDown(string key, string index)
    {
        return new CustomBindablePickerControl
        {
            ControlResourceKey = key,
            ClassId = index
        };
    }

    private CustomEntryControl CreateDisabledEntry(string key, string index)
    {
        return new CustomEntryControl
        {
            ControlType = FieldTypes.NumericEntryControl,
            ControlResourceKey = key,
            IsEnabled = false,
            ClassId = index
        };
    }

    internal async Task OnSaveButtonClickedAsync()
    {
        var isPageValid = ParentPage.IsFormControlValid(_mainLayout);
        var isItemValid = ParentPage.IsFormControlValid(_itemLayout);
        System.Diagnostics.Debug.WriteLine($" ===={isPageValid} === {isItemValid}");
        if (isPageValid && isItemValid)
        {
            if (_billingsData.PatientBillItems.FindAll(x => x.IsActive).Count > 0)
            {

                System.Diagnostics.Debug.WriteLine($"kumkum undar ===={isPageValid} === {isItemValid}");
                AppHelper.ShowBusyIndicator = true;
                MapAndSaveData();
                await (ParentPage.PageService as PatientBillService).SavePatientBillingDataAsync(_billingsData).ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
                if (_billingsData.ErrCode == ErrorCode.OK)
                {
                    _ = ParentPage.SyncDataWithServerAsync(Pages.PatientBillingPage, false, default).ConfigureAwait(true);
                    await InvokeAndClosePopupAsync().ConfigureAwait(true);
                }
                else
                {
                    ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(_billingsData.ErrCode.ToString()));
                }
            }
            else
            {
                _addItemError.IsVisible = true;
                _addItemError.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_DROPDOWN_SELECTION_VALIDATION_KEY);
            }
        }
    }

    private async Task InvokeAndClosePopupAsync()
    {
        AppHelper.ShowBusyIndicator = false;
        OnSaveSuccess?.Invoke(ErrorCode.OK.ToString(), new EventArgs());
        //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
    }

    private async void ShareBill_Clicked(object sender, EventArgs e)
    {
        var parameter = ParentPage.AddParameters(
            ParentPage.CreateParameter(nameof(PatientBillModel.PatientBillID), _billingsData.PatientBillItem.PatientBillID.ToString()),
            ParentPage.CreateParameter(nameof(PatientBillModel.ProgramColor), StyleConstants.TRANSPARENT_COLOR_STRING),
            ParentPage.CreateParameter(nameof(BaseDTO.SelectedUserID), _billingsData.SelectedUserID.ToString(CultureInfo.InvariantCulture))
        );
        var patientAddEditPage = new PatientBillSharePopupPage(ParentPage, parameter);
        //todo:await Navigation.PushPopupAsync(patientAddEditPage).ConfigureAwait(false);
    }

    private void MapAndSaveData()
    {
        var datetime = _billingDate.GetSetDate.Value;
        _billingsData.PatientBillItem = new PatientBillModel
        {
            PatientID = _billingsData.SelectedUserID,
            PatientBillID = _billingsData.PatientBillItem?.PatientBillID ?? Guid.Empty,
            ProgramID = _programPicker.SelectedValue,
            ProviderID = _programCaregiverPicker.SelectedValue,
            BillDateTime = new DateTimeOffset(datetime.Year, datetime.Month, datetime.Day, 0, 0, 0, TimeSpan.Zero),
            GrossTotal = Convert.ToDouble(_billingTotalAmount.Value, CultureInfo.InvariantCulture),
            Discount = string.IsNullOrWhiteSpace(_billingDiscount.Value) ? 0 : Convert.ToDouble(_billingDiscount.Value, CultureInfo.InvariantCulture),
            PaymentModeID = (short)_paymentMode.SelectedValue,
            TotalPaid = Convert.ToDouble(_billingAmountPaid.Value, CultureInfo.InvariantCulture),
            IsActive = !_isDeleteClicked,
            IsSynced = false
        };
        if (_billingsData.PatientBillItem.PatientBillID == Guid.Empty)
        {
            _billingsData.PatientBillItems = _billingsData.PatientBillItems.Where(x => x.IsActive).ToList();
        }
    }

    private async void DeleteBill_Clicked(object sender, EventArgs e)
    {
        await ParentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnDeleteItemActionClicked, true, true, false).ConfigureAwait(true);
    }

    private async void OnDeleteItemActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 1:
                _isDeleteClicked = true;
                _billingsData.IsActive = false;
                await OnSaveButtonClickedAsync();
                break;
            case 2:
                ParentPage.OnClosePupupAction(sender, e);
                break;
            default:// to do
                break;
        }
    }

    private void AddItem_Clicked(object sender, EventArgs e)
    {
        var lastIndex = _itemLayout.RowDefinitions.Count;
        if (ParentPage.IsFormControlValid(_itemLayout))
        {
            AddItemInUI(lastIndex, out CustomBindablePickerControl patientBillingItemPicker, out CustomEntryControl billingAmount, out CustomLabelControl removeButton);
            patientBillingItemPicker.SelectedValuesChanged += PatientBillingItemPicker_SelectedValuesChanged;
        }
    }

    private void PatientBillingItemPicker_SelectedValuesChanged(object sender, EventArgs e)
    {
        if (_addItemError.IsVisible)
        {
            _addItemError.Text = string.Empty;
        }
        CustomBindablePicker picker = (CustomBindablePicker)sender;
        var index = picker.ClassId.Split(Constants.SYMBOL_DOT)[1];

        //todo: var gridChild = (_itemLayout as Grid).Children.Where(x => x.ClassId != null && x.ClassId.Contains(index));
        //CustomEntryControl amountEntry = null;
        //CustomLabelControl removeButton = null;
        //if (gridChild?.Any() == true)
        //{
        //    amountEntry = gridChild.ElementAt(1) as CustomEntryControl;
        //    removeButton = gridChild.ElementAt(2) as CustomLabelControl;
        //}
        if (picker.SelectedIndex != -1)
        {
            var billinhgItem = _billingsData.PatientBillingItems[picker.SelectedIndex];
            var alreadyAddedItem = _billingsData.PatientBillItems.FirstOrDefault(x => x.BillingItemID == billinhgItem.OptionID && x.IsActive);
            if (alreadyAddedItem == null)
            {
                //todo: AssignedPickerValueChanged(index, amountEntry, removeButton, billinhgItem);
                picker.IsEnabled = false;
            }
            else
            {
                if (index == "0")
                {
                    _billingsData.PatientBillItems = _billingsData.PatientBillItems.Where(x => x.IsActive = false).ToList();
                    //todo: AssignedPickerValueChanged(index, amountEntry, removeButton, billinhgItem);
                    picker.IsEnabled = false;
                }
                else
                {
                    picker.SelectedIndex = -1;
                    picker.SelectedItem = null;
                    //todo: 
                    //if (amountEntry != null)
                    //{
                    //    amountEntry.Value = string.Empty;
                    //}
                }
            }
        }
    }

    private void AssignedPickerValueChanged(string index, CustomEntryControl amountEntry, CustomLabelControl removeButton, OptionModel billinhgItem)
    {
        var pickerAlreadyHavingValue = _billingsData.PatientBillItems.FirstOrDefault(x => x.TempBillingID == index && x.IsActive);
        if (pickerAlreadyHavingValue == null)
        {
            _billingsData.PatientBillItems.Add(new PatientBillItemModel
            {
                PatientBillID = _billingsData.PatientBillItem.PatientBillID,
                BillingItemID = Convert.ToInt16(billinhgItem.OptionID, CultureInfo.InvariantCulture),
                Amount = billinhgItem.ParentOptionID,
                IsActive = true,
                TempBillingID = index
            });
        }
        else
        {
            pickerAlreadyHavingValue.BillingItemID = Convert.ToInt16(billinhgItem.OptionID, CultureInfo.InvariantCulture);
            pickerAlreadyHavingValue.Amount = billinhgItem.ParentOptionID;
        }
        _billingTotalAmount.Value = _billingsData.PatientBillItems.Where(x => x.IsActive).Select(x => x.Amount).Sum().ToString();
        CalculateAmountPaid();
        if (amountEntry != null)
        {
            amountEntry.Value = billinhgItem.ParentOptionID.ToString();
        }
        if (removeButton != null)
        {
            removeButton.StyleId = billinhgItem.OptionID.ToString();
        }
    }

    public async override Task UnloadUIAsync()
    {
        _addItem.Clicked -= AddItem_Clicked;
        _deleteBill.Clicked -= DeleteBill_Clicked;
        _shareBill.Clicked -= ShareBill_Clicked;
        await Task.CompletedTask;
    }
}