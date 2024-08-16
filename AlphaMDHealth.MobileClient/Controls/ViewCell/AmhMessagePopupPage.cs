using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    public class AmhMessagePopupPage : BasePopupPage
    {
        private Grid _actionLayout;
        private BasePage _parent;

        public EventHandler<EventArgs> OnActionClicked { get; set; }

        public AmhMessagePopupPage(Grid mainLayout, List<ButtonActionModel> actions, BasePage page)
        {
            _parent = page;
            if (actions != null || actions.Count > 0)
            {
                AddActionButtons(actions);
                mainLayout.Add(_actionLayout, 0, 5);
                Grid.SetColumnSpan(_actionLayout, 2);
            }
            Background = Color.FromArgb(StyleConstants.OVERLAY_COLOR);
            Border layoutBorder = new Border
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_MESSAGE_POPUP_STYLE],
                Content = mainLayout
            };
            Content = layoutBorder;
        }

        private void AddActionButtons(List<ButtonActionModel> actions)
        {
            _actionLayout = _actionLayout ?? new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
            };
            foreach (var action in actions)
            {
                _actionLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                AmhButtonControl actionButton = new AmhButtonControl(action.FieldType)
                {
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_ACTION_BUTTON_STYLE],
                    PageResources = _parent.PageData,
                    ResourceKey = action.ButtonResourceKey,
                    StyleId = action.ButtonID
                };
                actionButton.OnValueChanged += ActionButton_OnValueChanged;
                _actionLayout.Add(actionButton, actions.IndexOf(action), 0);
            }
        }

        private void ActionButton_OnValueChanged(object sender, EventArgs e)
        {
            if (OnActionClicked != null && sender != null)
            {
                OnActionClicked.Invoke(sender, e);
            }
        }
    }
}