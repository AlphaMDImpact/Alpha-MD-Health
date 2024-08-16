using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class OrganisationProfilePage : BasePage
{
    private OrganisationDTO _organisationData = new OrganisationDTO { OrganisationProfile = new OrganisationModel(), PageDetails = new List<ContentDetailModel>() };
    private string _selectedValue;
    private bool _isEditable;
    private double? _unitPriceForPatient;
    private double? _discountForPatient;
    private double? _minimumToBuyForPatient;
    private double? _unitPriceForOrganisation;
    private double? _discountForOrganisation;
    private double? _minimumToBuyForOrganisation;

    /// <summary>
    /// Organisation ID
    /// </summary>
    [Parameter]
    public long OrganisationID { get; set; }

    /// <summary>
    /// Flag represents is it manage organization flow
    /// </summary>
    [Parameter]
    public bool IsManageOrganization { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _organisationData.IsActive = AppState.RouterData.SelectedRoute.Page == AppPermissions.OrganisationSetup.ToString();
        _organisationData.OrganisationID = OrganisationID;
        await SendServiceRequestAsync(new OrganisationService(AppState.webEssentials).SyncOrganisationProfileFromServerAsync(_organisationData, CancellationToken.None), _organisationData).ConfigureAwait(false);
        _isEditable = LibPermissions.HasPermission(_organisationData.FeaturePermissions, AppPermissions.OrganisationAddEdit.ToString()) || _organisationData.OrganisationID == 0;
        if (_organisationData.ErrCode == ErrorCode.OK && _organisationData.OrganisationID > 0 && _organisationData.PageDetails.Count > 0)
        {
            _selectedValue = _organisationData.Languages.FirstOrDefault(x => x.IsDisabled == true).OptionID.ToString();
            if(IsManageOrganization && _organisationData.OrganisationExternalServices?.Count > 0)
            {
                var patientServiceData = _organisationData.OrganisationExternalServices.FirstOrDefault(x=>x.ForPatient == true);
                if (patientServiceData != null)
                {
                    _unitPriceForPatient = Convert.ToDouble(patientServiceData.UnitPrice);
                    _discountForPatient = Convert.ToDouble(patientServiceData.DiscountPercentage);
                    _minimumToBuyForPatient = Convert.ToDouble(patientServiceData.MinimumQuantityToBuy);
                }
                var organisationServiceData = _organisationData.OrganisationExternalServices.FirstOrDefault(x => x.ForPatient == false);
                if (patientServiceData != null)
                {
                    _unitPriceForOrganisation = Convert.ToDouble(organisationServiceData.UnitPrice);
                    _discountForOrganisation = Convert.ToDouble(organisationServiceData.DiscountPercentage);
                    _minimumToBuyForOrganisation = Convert.ToDouble(organisationServiceData.MinimumQuantityToBuy);
                }
            }
        }
        else if (_organisationData.IsActive && _organisationData.ErrCode != ErrorCode.OK)
        {
            await NavigateToAsync(AppPermissions.LoginView.ToString()).ConfigureAwait(false);
            return;
        }
        _isDataFetched = true;
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            _organisationData.Languages.Find(x => x.OptionID == Convert.ToInt64(_selectedValue)).IsDefault = true;
            OrganisationDTO organizationData = new OrganisationDTO
            {
                OrganisationID = _organisationData.OrganisationID,
                OrganisationProfile = _organisationData.OrganisationProfile,
                PageDetails = new List<ContentDetailModel>(),
                Languages = new List<OptionModel>(),
            };
            organizationData.Languages.Add(_organisationData.Languages.Find(x => x.OptionID == Convert.ToInt64(_selectedValue)));
            organizationData.Languages.AddRange(_organisationData.Languages.Where(x => !x.IsDisabled && x.IsSelected).ToList());
            if (_organisationData.OrganisationID < 1)
            {
                organizationData.IsActive = IsManageOrganization;
                organizationData.Languages.ForEach(item =>
                {
                    organizationData.PageDetails.Add(new ContentDetailModel { LanguageID = (byte)item.OptionID, PageHeading = _organisationData.OrganisationProfile.OrganisationName });
                });
            }
            else
            {
                organizationData.PageDetails = _organisationData.PageDetails;
            }
            if (IsManageOrganization)
            {
                organizationData.OrganisationProfile.PlanID = (short)_organisationData.PaymentPlans.FirstOrDefault(x => x.IsSelected == true).OptionID;
                organizationData.OrganisationProfile.IsManageOrganisation = IsManageOrganization;
                int externalServiceId = (int)_organisationData.ExternalServices.FirstOrDefault(x => x.IsSelected == true).OptionID;
                long organisationServiceIdforOrganisation = 0;
                long organisationServiceIdforPatient = 0;
                if (GenericMethods.IsListNotEmpty(_organisationData?.OrganisationExternalServices))
                {
                    organisationServiceIdforOrganisation = OrganisationID > 0 ? _organisationData.OrganisationExternalServices.FirstOrDefault(x => x.ForPatient == false)?.OrganisationServiceID ?? 0 : 0;
                    organisationServiceIdforPatient = OrganisationID > 0 ? _organisationData.OrganisationExternalServices.FirstOrDefault(x => x.ForPatient == true)?.OrganisationServiceID ?? 0 : 0;
                }
                organizationData.OrganisationExternalServices = new List<OrganisationExternalServiceModel>
                {
                    new OrganisationExternalServiceModel
                    {
                        OrganisationServiceID = organisationServiceIdforOrganisation,
                        OrganisationID = organizationData.OrganisationID,
                        ExternalServiceID = externalServiceId,
                        UnitPrice = (decimal)_unitPriceForOrganisation,
                        DiscountPercentage = _discountForOrganisation != null ? (decimal)_discountForOrganisation : 0,
                        MinimumQuantityToBuy = (int)_minimumToBuyForOrganisation,
                        ForPatient = false
                    },
                    new OrganisationExternalServiceModel
                    {
                        OrganisationServiceID = organisationServiceIdforPatient,
                        OrganisationID = organizationData.OrganisationID,
                        ExternalServiceID = externalServiceId,
                        UnitPrice = (decimal)_unitPriceForPatient,
                        DiscountPercentage = _discountForPatient != null ? (decimal)_discountForPatient : 0,
                        MinimumQuantityToBuy = (int)_minimumToBuyForPatient,
                        ForPatient = true
                    },
                };
            }
            await SendServiceRequestAsync(new OrganisationService(AppState.webEssentials).SyncOrganisationProfileToServerAsync(organizationData, CancellationToken.None, IsManageOrganization), organizationData).ConfigureAwait(true);
            if (organizationData.ErrCode == ErrorCode.OK)
            {
                if (IsManageOrganization)
                {
                    _organisationData.OrganisationID = organizationData.OrganisationID;
                    await NavigateToAsync(AppPermissions.ManageOrganisationsView.ToString()).ConfigureAwait(false);
                    return;
                }
                if (_organisationData.OrganisationID > 0)
                {
                    Success = organizationData.ErrCode.ToString();
                    await NavigateToAsync(AppState.MasterData.DefaultRoute, true).ConfigureAwait(true);
                }
                else
                {
                    await NavigateToAsync(NavigationManager.BaseUri, true).ConfigureAwait(true);
                }
            }
            else
            {
                Error = organizationData.ErrCode.ToString();
            }
        }
    }

    private void OnDefaultLanguageChange()
    {
        if (string.IsNullOrWhiteSpace(_selectedValue))
        {
            var defaultLanguage = _organisationData.Languages.Find(x => x.IsSelected == true);
            if (defaultLanguage != null)
            {
                _organisationData.Languages.ForEach((language) =>
                {
                    language.IsDisabled = language.OptionID == defaultLanguage.OptionID;
                });
            }
        }
        else
        {
            var languageData = _organisationData.Languages.Find(x => x.OptionID == Convert.ToInt64(_selectedValue));
            if (languageData != null)
            {
                _organisationData.Languages.ForEach((language) =>
                {
                    language.IsDisabled = language.OptionID == languageData.OptionID;
                });
            }
        }

    }

    private readonly List<TabDataStructureModel> DataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel
        {
            DataField=nameof(ContentDetailModel.PageHeading),
            ResourceKey= ResourceConstants.R_ORGANISATION_NAME_KEY,
            IsRequired = true
        },
    };

    private async Task OnCancelClickedAsync()
    {
        await NavigateToAsync(AppPermissions.ManageOrganisationsView.ToString()).ConfigureAwait(true);
    }
}