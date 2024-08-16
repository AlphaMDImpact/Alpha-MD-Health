using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AlphaMDHealth.WebClient
{
    public partial class Header
    {
        /// <summary>
        /// Click event of side bar toggle
        /// </summary>
        [Parameter]
        public EventCallback<EventArgs> OnSidebarToggleClicked { get; set; }

        /// <summary>
        /// menu click event
        /// </summary>
        [Parameter]
        public EventCallback<MenuModel> OnMenuClicked { get; set; }

        /// <summary>
        /// flag representing before or after login
        /// </summary>
        [Parameter]
        public bool IsAfterLoginLayout { get; set; }

        /// <summary>
        /// flag representing before or after login
        /// </summary>
        [Parameter]
        public int Screenwidth { get; set; }

        string selectedLanguage;

        protected override async void OnInitialized()
        {
            selectedLanguage = AppState.SelectedLanguageID.ToString();
            StateHasChanged();
            base.OnInitialized();
        }

        private async void OnParentClicked(MenuItemEventArgs args)
        {
            if (args?.Value != null && args.Value is MenuModel)
            {
                await OnFeatureMenuClickAsync(args.Value as MenuModel);
            }
        }

        private void OnChildClicked(MenuItemEventArgs args)
        {
            // console.Log($"{args.Text} from child clicked");
        }

        private List<OptionModel> GetLanguageOptions()
        {
            return (from item in AppState.MasterData?.Languages
                    where item.IsActive
                    select new OptionModel
                    {
                        OptionID = Convert.ToByte(item.LanguageID),
                        OptionText = item.LanguageName,
                        IsSelected = item.LanguageID == AppState.SelectedLanguageID
                    })?.ToList() ?? new List<OptionModel>();
        }

        private async Task NavigateView(AppPermissions targetPage)
        {
            var item = new MenuModel { TargetPage = targetPage.ToString() };
            await OnFeatureMenuClickAsync(item);
        }

        private async Task OnLanguageChangeAsync(object e)
        {
            if (AppState.SelectedLanguageID != Convert.ToByte(string.IsNullOrWhiteSpace(selectedLanguage) ? 0 : selectedLanguage))
            {
                AppState.SelectedLanguageID = Convert.ToByte(string.IsNullOrWhiteSpace(selectedLanguage) ? 0 : selectedLanguage);
                if (AppState.RouterData.SelectedRoute.Page == AppPermissions.SMSAuthenticationView.ToString())
                {
                    await NavigateView(AppPermissions.SMSAuthenticationView).ConfigureAwait(true);
                }
                else
                {
                    await NavigateView(AppPermissions.LanguageSelectionView).ConfigureAwait(true);
                }
            }
        }

        private async Task OnFeatureMenuClickAsync(MenuModel menuItem)
        {
            await OnMenuClicked.InvokeAsync(menuItem);
        }
    }
}