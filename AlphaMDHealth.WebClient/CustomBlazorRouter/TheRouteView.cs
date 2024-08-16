using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Reflection;

namespace AlphaMDHealth.WebClient
{
    public class TheRouteView : IComponent
    {
        private readonly RenderFragment _renderDelegate;
        private readonly RenderFragment _renderPageWithParametersDelegate;
        private RenderHandle _renderHandle;

        /// <summary>
        /// Gets or sets the route data. This determines the page that will be
        /// displayed and the parameter values that will be supplied to the page.
        /// </summary>
        [Parameter]
        public RouteData RouteData { get; set; }

        /// <summary>
        /// Gets or sets the type of a layout to be used if the page does not
        /// declare any layout. If specified, the type must implement <see cref="IComponent"/>
        /// and accept a parameter named <see cref="LayoutComponentBase.Body"/>.
        /// </summary>
        [Parameter]
        public Type DefaultLayout { get; set; }

        public TheRouteView()
        {
            // Cache the delegate instances
            //if(RouteData. AppState.Routes
            _renderDelegate = Render;
            _renderPageWithParametersDelegate = RenderPageWithParameters;
        }

        /// <inheritdoc />
        public void Attach(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
        }

        /// <inheritdoc />
        public Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);

            if (RouteData == null)
            {
                throw new InvalidOperationException($"The {nameof(RouteView)} component requires a non-null value for the parameter {nameof(RouteData)}.");
            }

            ////builder.(2, "RouterRouteData", RouteData.RouterRouteData);
            _renderHandle.Render(_renderDelegate);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="builder">The <see cref="RenderTreeBuilder"/>.</param>
        protected virtual void Render(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                Type pageLayoutType = RouteData.PageType.GetCustomAttribute<LayoutAttribute>()?.LayoutType
                    ?? DefaultLayout;
                const int sequenceNumber = 2;
                builder.OpenComponent<LayoutView>(0);
                builder.AddAttribute(1, nameof(LayoutView.Layout), pageLayoutType);
                builder.AddAttribute(sequenceNumber, nameof(LayoutView.ChildContent), _renderPageWithParametersDelegate);

                builder.CloseComponent();
            }
        }

        private void RenderPageWithParameters(RenderTreeBuilder builder)
        {
            builder.OpenComponent(0, RouteData.PageType);

            foreach (System.Collections.Generic.KeyValuePair<string, object> kvp in RouteData.RouteValues)
            {
                builder.AddAttribute(1, kvp.Key, kvp.Value);
            }
            builder.AddAttribute(1, "RouterDataRoute", RouteData.RouterRouteData);
            builder.CloseComponent();
        }
    }
}
