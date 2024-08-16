using System.Globalization;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Device = Microsoft.Maui.Controls.Device;
using FlowDirection = Microsoft.Maui.FlowDirection;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represemnts Responsive View
    /// </summary>
    public class ResponsiveView : Border
    {
        private CustomImageControl _icon;
        private CustomLabelControl _cellHeader;
        private CustomLabelControl _cellDescription;
        private CustomLabelControl _cellRightContentHeader;
        private CustomLabelControl _cellRightContentDescription;
        private CustomLabelControl _cellFirstMiddleSatusContentHeader;
        private CustomLabelControl _cellSecondMiddleSatusContentHeader;
        private CustomImageControl _rightIcon;
        private ContentView _imageHolder;
        private Grid _mainGrid;

        private readonly double _margin = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];

        /// <summary>
        /// on click event of cell
        /// </summary>
        public event EventHandler<EventArgs> OnItemClicked;

        /// <summary>
        /// constructor for class Responsive View
        /// </summary>
        /// <param name="customCellModel"></param>
        public ResponsiveView(CustomCellModel customCellModel)
        {
            if (customCellModel.ArrangeHorizontalVertical)
            {
                ArrangeHorizontalVerticalList(customCellModel);
            }
            else if (customCellModel.ArrangeHorizontal)
            {
                HorizontalList(customCellModel);
            }
            else
            {
                CreateVerticalList(customCellModel);
            }
        }

        //use for Education list
        private void ArrangeHorizontalVerticalList(CustomCellModel customCellModel)
        {
            _icon = new CustomImageControl(Constants.STRING_SPACE, Constants.STRING_SPACE);
            AutomationProperties.SetIsInAccessibleTree(_icon, true);
            double margin = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
            if (!string.IsNullOrWhiteSpace(customCellModel.CellLeftSourceIcon))
            {
                _icon.SetBinding(CustomImageControl.SourceProperty, customCellModel.CellLeftSourceIcon);
            }
            if (!string.IsNullOrWhiteSpace(customCellModel.CellLeftDefaultIcon))
            {
                _icon.SetBinding(CustomImageControl.DefaultValueProperty, customCellModel.CellLeftDefaultIcon);
            }
            CustomLabelControl cellHeader = new CustomLabelControl(LabelType.ListHeaderStyle)
            {
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.WordWrap
            };
            AutomationProperties.SetIsInAccessibleTree(cellHeader, true);
            cellHeader.SetBinding(CustomLabelControl.TextProperty, customCellModel.CellHeader);
            CustomLabelControl cellDescription = new(LabelType.SecondrySmallLeft)
            {
                VerticalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.WordWrap
            };
            AutomationProperties.SetIsInAccessibleTree(cellDescription, true);
            cellDescription.SetBinding(CustomLabel.TextProperty, customCellModel.CellDescription);
            var contentGrid = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, margin),
                RowSpacing = 3,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };
            var mainGrid = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
                ColumnSpacing = margin,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };
            _imageHolder = new ContentView
            {
                Content = _icon,
                Margin = new Thickness(0, margin)
            };

            if (!string.IsNullOrWhiteSpace(customCellModel.ImageBackgroundColor))
            {
                _imageHolder.SetBinding(ContentView.BackgroundColorProperty, customCellModel.ImageBackgroundColor);
            }

            mainGrid.Add(_imageHolder, 0, 0);
            contentGrid.Add(cellHeader, 0, 0);
            contentGrid.Add(cellDescription, 0, 1);
            mainGrid.Add(contentGrid, 1, 0);
            if (!string.IsNullOrWhiteSpace(customCellModel.ArrangeHorizontalHeight))
            {
                _imageHolder.SetBinding(Grid.HeightRequestProperty, customCellModel.ArrangeHorizontalHeight);
            }
            if (!string.IsNullOrWhiteSpace(customCellModel.ArrangeHorizontalWidth))
            {
                _imageHolder.SetBinding(Grid.WidthRequestProperty, customCellModel.ArrangeHorizontalWidth);
            }
            _imageHolder.SetBinding(Grid.RowSpanProperty, new Binding { Path = customCellModel.ArrangeHorizontalFullWith, Converter = new BoolToIntegerConvertor(), ConverterParameter = "RowSpan" });
            _imageHolder.SetBinding(Grid.ColumnSpanProperty, new Binding { Path = customCellModel.ArrangeHorizontalFullWith, Converter = new BoolToIntegerConvertor(), ConverterParameter = "ColumnSpan" });
            contentGrid.SetBinding(Grid.RowProperty, new Binding { Path = customCellModel.ArrangeHorizontalFullWith, Converter = new BoolToIntegerConvertor(), ConverterParameter = "TopContentGrid" });
            contentGrid.SetBinding(Grid.ColumnProperty, new Binding { Path = customCellModel.ArrangeHorizontalFullWith, Converter = new BoolToIntegerConvertor(), ConverterParameter = "LeftContentGrid" });
            if (customCellModel.IsList)
            {
                ShowSeprator(0, mainGrid, 3, 2);
            }
            Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_STYLE];
            Stroke = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
            Margin = new Thickness(0, 0, 0, 0);
            Content = mainGrid;
        }

        private void HorizontalList(CustomCellModel customCellModel)
        {
            _icon = new CustomImageControl(Constants.STRING_SPACE, Constants.STRING_SPACE);
            AutomationProperties.SetIsInAccessibleTree(_icon, true);
            double margin = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
            if (!string.IsNullOrWhiteSpace(customCellModel.CellLeftSourceIcon))
            {
                _icon.SetBinding(CustomImageControl.SourceProperty, customCellModel.CellLeftSourceIcon);
            }
            if (!string.IsNullOrWhiteSpace(customCellModel.CellLeftDefaultIcon))
            {
                _icon.SetBinding(CustomImageControl.DefaultValueProperty, customCellModel.CellLeftDefaultIcon);
            }
            CustomLabelControl cellHeader = new(LabelType.SecondryAppSmallLeft)
            {
                VerticalOptions = LayoutOptions.End,
                Margin = AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight ? new Thickness(0, 0, 2 * margin, 0) : new Thickness(2 * margin, 0, 0, 0),
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            AutomationProperties.SetIsInAccessibleTree(cellHeader, true);
            cellHeader.SetBinding(CustomLabel.TextProperty, customCellModel.CellHeader);
            CustomLabelControl cellDescription = new(LabelType.TertiarySmallLeft)
            {
                VerticalOptions = LayoutOptions.End,
                LineBreakMode = LineBreakMode.TailTruncation,
                Margin = AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight ? new Thickness(0, margin / 3, 2 * margin, 2 * margin) : new Thickness(2 * margin, margin / 3, 0, 2 * margin),
            };
            AutomationProperties.SetIsInAccessibleTree(cellDescription, true);
            cellDescription.SetBinding(CustomLabel.TextProperty, customCellModel.CellDescription);
            var imageWidth = (App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, (double)0) - new OnIdiom<double> { Phone = 3, Tablet = 5 } * margin) / new OnIdiom<double> { Phone = 2, Tablet = 4 };
            var mainGrid = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                WidthRequest = imageWidth,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(margin, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(imageWidth, GridUnitType.Absolute) }
                }
            };
            var overLayView = new Frame
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_CARD_OVERLAY_VIEW_FRAME],
                Margin = AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight ? new Thickness(0, 0, new OnIdiom<double> { Phone = 0, Tablet = margin }, 0) : new Thickness(new OnIdiom<double> { Phone = 0, Tablet = margin }, 0, 0, 0)
            };
            AutomationProperties.SetIsInAccessibleTree(overLayView, true);
            var imageGrid = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                HeightRequest = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_DEFAULT_CARD_HEIGHT_STYLE], CultureInfo.InvariantCulture),
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_DEFAULT_CARD_HEIGHT_STYLE], CultureInfo.InvariantCulture), GridUnitType.Absolute) },
                },
                ColumnDefinitions =
                {
                  new ColumnDefinition { Width = new GridLength(imageWidth , GridUnitType.Absolute) },
                },
            };
            imageGrid.Add(_icon, 0, 0);
            mainGrid.Add(imageGrid, 0, 0);
            mainGrid.Add(overLayView, 0, 0);
            Grid.SetRowSpan(imageGrid, 3);
            Grid.SetColumnSpan(imageGrid, 2);
            Grid.SetRowSpan(overLayView, 3);
            Grid.SetColumnSpan(overLayView, 2);
            mainGrid.Add(cellHeader, 1, 1);
            mainGrid.Add(cellDescription, 1, 2);
            Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_STYLE];
            BackgroundColor = Colors.Transparent;
            StrokeThickness = 0;
            Content = mainGrid;
        }

        private void CreateVerticalList(CustomCellModel customCellModel)
        {
            int left = Constants.ZERO_VALUE;
            CreateAllViews(customCellModel);
            _mainGrid = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                Padding = new Thickness(_margin),
                ColumnSpacing = _margin,
                // VerticalOptions = LayoutOptions.Center,medicines label cut issue
                RowSpacing = 3,
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto },
                }
            };
            left = ShowBandColor(customCellModel, left);
            _mainGrid.Add(_icon, left, 0);
            Grid.SetRowSpan(_icon, 4);
            if (customCellModel.ShowIconOnly)
            {
                Grid.SetColumnSpan(_icon, 3);
            }
            else
            {
                left = ShowDotStatus(customCellModel, left);
                CreateCellLeftHeaderDescription(customCellModel, left);
                left = FirstMiddleContentHeaderInPancakeView(customCellModel, left);
                left = SecondMiddleSatusContentHeaderInPancakeView(customCellModel, left);
                RightIconAndContentView(customCellModel, left);
            }
            Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_STYLE];
            Stroke = customCellModel.NoMarginNoSeprator ? (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR] : (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE];
            Margin = new Thickness(0, 0, GenericMethods.GetPlatformSpecificValue(1, 0, 0), new OnIdiom<double> { Phone = /*customCellModel.NoMarginNoSeprator ? 10 : */0, Tablet = 0 });

            if (!string.IsNullOrWhiteSpace(customCellModel.ErrCode))
            {
                SetBinding(Border.BackgroundColorProperty, new Binding { Path = customCellModel.ErrCode, Converter = new ErrorCodeToStringCovertor() });
            }
            if (customCellModel.RemoveLeftMargin)
            {
                _mainGrid.Padding = new Thickness(0, _mainGrid.Padding.Top, _mainGrid.Padding.Right, _mainGrid.Padding.Bottom);
            }
            if (customCellModel.IsList)
            {
                ShowSeprator(left, _mainGrid, 4, 3);
            }
            if (customCellModel.RowHeight > 0)
            {
                _mainGrid.HeightRequest = customCellModel.RowHeight;
            }
            Content = _mainGrid;
        }
        private void ShowSeprator(int left, Grid mainLayout, int row, int rowSpan)
        {
            var seprator = new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = GenericMethods.GetPlatformSpecificValue((Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE], (Color)Application.Current.Resources[StyleConstants.ST_TERTIARY_TEXT_COLOR], Colors.Transparent), 
                Margin = new Thickness(-_margin, 0, -_margin, mainLayout.Padding.Bottom > 0 ? -mainLayout.Padding.Bottom : 0),
                VerticalOptions = LayoutOptions.End,
            };
            AutomationProperties.SetIsInAccessibleTree(seprator, true);
            mainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GenericMethods.GetPlatformSpecificValue(1, 1, 0), GridUnitType.Absolute) });
            mainLayout.Add(seprator, 0, row);
            Grid.SetColumnSpan(seprator, left + rowSpan);
        }
        private void CreateCellLeftHeaderDescription(CustomCellModel customCellModel, int left)
        {
            if (string.IsNullOrWhiteSpace(customCellModel.CellDescription))
            {
                _cellHeader.VerticalOptions = LayoutOptions.Center;
                if (!string.IsNullOrWhiteSpace(customCellModel.CellLeftSourceIcon) && customCellModel.RemoveColumnSpacing)
                {
                    _mainGrid.SetBinding(Grid.ColumnSpacingProperty, customCellModel.CellLeftSourceIcon, converter: new ColSpacingConvetor());
                    _mainGrid.SetBinding(Grid.ColumnSpacingProperty, customCellModel.CellLeftDefaultIcon, converter: new ColSpacingConvetor());
                }
                _cellHeader.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_TEXT_COLOR];
                _mainGrid.Add(_cellHeader, left + 1, 0);
                Grid.SetRowSpan(_cellHeader, 4);
            }
            else
            {
                _cellDescription.SetBinding(CustomLabelControl.TextProperty, customCellModel.CellDescription);
                _cellDescription.IsHtmlLabel = customCellModel.IsDescriptionHtml;
                if (_cellDescription.IsHtmlLabel)
                {
                    _cellDescription.Margin = new Thickness(0, 0, 0, 0);
                }
                if (string.IsNullOrWhiteSpace(customCellModel.CellDescriptionStatusContent))
                {
                    _mainGrid.Add(_cellHeader, left + 1, 1);
                    _mainGrid.Add(_cellDescription, left + 1, 2);
                    _cellDescription.MinimumWidthRequest = 90;
                }
                else
                {
                    var layout = new StackLayout();
                    var formattedString = new FormattedString();
                    var desc_span = new Span();
                    desc_span.SetBinding(Span.TextProperty, customCellModel.CellDescription);
                    formattedString.Spans.Add(desc_span);
                    var leftstatusDescriptionStatus = new Span {
                        //FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                        FontSize = 24
                    };
                    leftstatusDescriptionStatus.SetBinding(Span.TextProperty, customCellModel.CellDescriptionStatusContent);
                    leftstatusDescriptionStatus.SetBinding(Span.TextColorProperty, customCellModel.CellDescriptionStatusContentColor);
                    formattedString.Spans.Add(leftstatusDescriptionStatus);
                    layout.Add(new Label { FormattedText = formattedString });
                    _mainGrid.Add(_cellHeader, left + 1, 1);
                    _mainGrid.Add(layout, left + 1, 2);
                }
            }
        }
        private void RightIconAndContentView(CustomCellModel customCellModel, int left)
        {
            if (string.IsNullOrWhiteSpace(customCellModel.CellRightIcon))
            {
                CellRightHeader(customCellModel, left);
                CellRightDescription(customCellModel, left);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(customCellModel.CellRightContentHeader))
                {
                    _mainGrid.Padding = new Thickness(_margin);
                }
                _mainGrid.Add(_rightIcon, left + 2, 0);
                _rightIcon.SetBinding(CustomImageControl.DefaultValueProperty, customCellModel.CellRightIcon);
                Grid.SetRowSpan(_rightIcon, 4);
            }
        }
        private void CellRightHeader(CustomCellModel customCellModel, int left)
        {
            if (!string.IsNullOrWhiteSpace(customCellModel.IconAsCellRightContentHeader))
            {
                _mainGrid.Add(_rightIcon, left + 2, 1);
                _rightIcon.SetBinding(CustomImageControl.DefaultValueProperty, customCellModel.IconAsCellRightContentHeader);
            }
            else if (string.IsNullOrWhiteSpace(customCellModel.CellRightContentHeader))
            {
                _mainGrid.Padding = new Thickness(_margin);
            }
            else
            {
                _cellRightContentHeader.SetBinding(CustomLabelControl.TextProperty, customCellModel.CellRightContentHeader);
                if (!string.IsNullOrWhiteSpace(customCellModel.CellRightHeaderColor))
                {
                    _cellRightContentHeader.SetBinding(CustomLabelControl.TextColorProperty, customCellModel.CellRightHeaderColor);
                }
                _cellRightContentHeader.TextType = customCellModel.IsRightHeaderHtml ? TextType.Html : TextType.Text;
                if (!string.IsNullOrWhiteSpace(customCellModel.ShowRemoveButton))
                {
                    _cellRightContentHeader.MinimumWidthRequest = 70;
                    _cellRightContentHeader.SetBinding(CustomLabelControl.IsVisibleProperty, customCellModel.ShowRemoveButton);
                    _cellRightContentHeader.SetBinding(CustomLabelControl.ClassIdProperty, customCellModel.CellID);
                    _cellRightContentHeader.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_ERROR_COLOR];
                    TapGestureRecognizer removeTabButton = new TapGestureRecognizer();
                    removeTabButton.Tapped += TapGestureRecognizer_Tapped;
                    _cellRightContentHeader.GestureRecognizers.Add(removeTabButton);
                }
                if (customCellModel.CellRightContentHeaderInPancakeView)
                {
                    _cellRightContentHeader.HorizontalOptions = LayoutOptions.Center;
                    _cellRightContentHeader.VerticalOptions = LayoutOptions.Center;
                    _cellRightContentHeader.HorizontalTextAlignment = TextAlignment.Center;
                    Border status = new() { Style = (Style)Application.Current.Resources[customCellModel.RightHeaderStyle], VerticalOptions = LayoutOptions.Center, Content = _cellRightContentHeader };
                    status.SetBinding(Border.IsVisibleProperty, customCellModel.CellRightContentHeader, converter: new StringToBooleanConvertorForVisibility());
                    _mainGrid.Add(status, left + 2, 1);
                }
                else
                {
                    _mainGrid.Add(_cellRightContentHeader, left + 2, 1);
                }
                if (string.IsNullOrWhiteSpace(customCellModel.CellRightContentDescription))
                {
                    _mainGrid.Children.Remove(_cellRightContentHeader);
                    _mainGrid.Add(_cellRightContentHeader, left + 2, 0);
                    _cellRightContentHeader.VerticalOptions = LayoutOptions.Center;
                    Grid.SetRowSpan(_cellRightContentHeader, 4);
                }
            }
        }
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            OnItemClicked?.Invoke(sender, new EventArgs());
        }
        private void CellRightDescription(CustomCellModel customCellModel, int left)
        {
            if (string.IsNullOrWhiteSpace(customCellModel.CellRightContentDescription))
            {
                _mainGrid.Padding = new Thickness(_margin);
            }
            else
            {
                _cellRightContentDescription.SetBinding(CustomLabelControl.TextProperty, customCellModel.CellRightContentDescription);
                if (customCellModel.CellRightContentDescriptionInPancakeView || customCellModel.RightContentDescriptionInPancakeViewWithoutBorder)
                {
                    RightDescriptionInPancakeView(customCellModel, left);
                }
                else
                {
                    RightDescriptionWithoutPancake(customCellModel, left);
                }
            }
        }

        private void RightDescriptionWithoutPancake(CustomCellModel customCellModel, int left)
        {
            if (!string.IsNullOrWhiteSpace(customCellModel.CellDescriptionColor))
            {
                _cellRightContentDescription.SetBinding(CustomLabelControl.TextColorProperty, customCellModel.CellDescriptionColor);
            }
            _mainGrid.Add(_cellRightContentDescription, left + 2, 2);

            if (string.IsNullOrWhiteSpace(customCellModel.CellRightContentHeader))
            {
                _mainGrid.Padding = new Thickness(string.IsNullOrWhiteSpace(customCellModel.IconAsCellRightContentHeader) ? _margin : 0, _margin, _margin, _margin);
                _cellRightContentDescription.MinimumWidthRequest = 70;
                Grid.SetColumnSpan(_cellHeader, string.IsNullOrWhiteSpace(customCellModel.IconAsCellRightContentHeader) ? 2 : 1);
            }
        }

        private int FirstMiddleContentHeaderInPancakeView(CustomCellModel customCellModel, int left)
        {
            if (!string.IsNullOrWhiteSpace(customCellModel.CellFirstMiddleSatusContentHeader))
            {
                _cellFirstMiddleSatusContentHeader.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                _cellFirstMiddleSatusContentHeader.SetBinding(CustomLabelControl.TextProperty, customCellModel.CellFirstMiddleSatusContentHeader);
                _mainGrid.ColumnDefinitions.Insert(2, new ColumnDefinition { Width = GridLength.Auto });
                _cellFirstMiddleSatusContentHeader.VerticalOptions = LayoutOptions.Center;
                Border status = new() { Style = (Style)Application.Current.Resources[customCellModel.CellMiddleSatusHeaderStyle], VerticalOptions = LayoutOptions.Center, Content = _cellFirstMiddleSatusContentHeader };
                if (!string.IsNullOrWhiteSpace(customCellModel.CellFirstMiddleContentHeaderColor))
                {
                    _cellFirstMiddleSatusContentHeader.SetBinding(CustomLabelControl.TextColorProperty, customCellModel.CellFirstMiddleContentHeaderColor);
                    status.SetBinding(Border.StrokeProperty, customCellModel.CellFirstMiddleContentHeaderColor);
                    status.SetBinding(Border.BackgroundColorProperty, customCellModel.CellFirstMiddleContentHeaderColor, converter: new StatusToBackgroundColorConvertor());
                }
                status.SetBinding(Border.IsVisibleProperty, customCellModel.CellFirstMiddleSatusContentHeader, converter: new StringToBooleanConvertorForVisibility());
                _mainGrid.Add(status, left + 2, 0);
                Grid.SetRowSpan(status, 4);
                left += 1;
            }
            return left;
        }

        private int SecondMiddleSatusContentHeaderInPancakeView(CustomCellModel customCellModel, int left)
        {
            if (!string.IsNullOrWhiteSpace(customCellModel.CellSecondMiddleSatusContentHeader))
            {
                _cellSecondMiddleSatusContentHeader.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                _cellSecondMiddleSatusContentHeader.SetBinding(CustomLabelControl.TextProperty, customCellModel.CellSecondMiddleSatusContentHeader);
                _mainGrid.ColumnDefinitions.Insert(2, new ColumnDefinition { Width = GridLength.Auto });
                _cellSecondMiddleSatusContentHeader.VerticalOptions = LayoutOptions.Center;
                Border status = new() { Style = (Style)Application.Current.Resources[customCellModel.CellMiddleSatusHeaderStyle], VerticalOptions = LayoutOptions.Center, Content = _cellSecondMiddleSatusContentHeader };
                if (!string.IsNullOrWhiteSpace(customCellModel.CellSecondMiddleSatusContentHeaderColor))
                {
                    _cellSecondMiddleSatusContentHeader.SetBinding(CustomLabelControl.TextColorProperty, customCellModel.CellSecondMiddleSatusContentHeaderColor);
                    status.SetBinding(Border.StrokeProperty, customCellModel.CellSecondMiddleSatusContentHeaderColor);
                    status.SetBinding(Border.BackgroundColorProperty, customCellModel.CellSecondMiddleSatusContentHeaderColor, converter: new StatusToBackgroundColorConvertor());
                }
                status.SetBinding(Border.IsVisibleProperty, customCellModel.CellSecondMiddleSatusContentHeader, converter: new StringToBooleanConvertorForVisibility());
                _mainGrid.Add(status, left + 2, 0);
                Grid.SetRowSpan(status, 4);
                left += 1;
            }
            return left;
        }
        private void RightDescriptionInPancakeView(CustomCellModel customCellModel, int left)
        {
            _cellRightContentDescription.FontSize = 20;
            _cellRightContentDescription.VerticalOptions = LayoutOptions.Center;
            _cellRightContentDescription.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
            if (customCellModel.RightContentDescriptionInPancakeViewWithoutBorder)
            {
                _cellRightContentDescription.Style = (Style)Application.Current.Resources[customCellModel.RightDesciptionStyle];
                if (!string.IsNullOrWhiteSpace(customCellModel.CellDescriptionColor))
                {
                    _cellRightContentDescription.SetBinding(CustomLabelControl.TextColorProperty, customCellModel.CellDescriptionColor);
                    _cellRightContentDescription.SetBinding(CustomLabelControl.CurvedBackgroundColorProperty, customCellModel.CellDescriptionColor, converter: new StatusToBackgroundColorConvertor());
                }
                _cellRightContentDescription.SetBinding(CustomLabelControl.IsVisibleProperty, customCellModel.CellRightContentDescription, converter: new StringToBooleanConvertorForVisibility());
                _mainGrid.Add(_cellRightContentDescription, left + 2, string.IsNullOrWhiteSpace(customCellModel.CellRightContentHeader) ? 1 : 2);
                Grid.SetRowSpan(_cellRightContentDescription, string.IsNullOrWhiteSpace(customCellModel.CellRightContentHeader) ? 2 : 1);
            }
            else
            {
                _cellRightContentDescription.HorizontalOptions = LayoutOptions.Center;
                Border rightDescriptionStatus = new() { Style = (Style)Application.Current.Resources[customCellModel.RightDesciptionStyle], VerticalOptions = LayoutOptions.Center, Content = _cellRightContentDescription };
                if (!string.IsNullOrWhiteSpace(customCellModel.CellDescriptionColor))
                {
                    _cellRightContentDescription.SetBinding(CustomLabelControl.TextColorProperty, customCellModel.CellDescriptionColor);
                    rightDescriptionStatus.SetBinding(Border.StrokeProperty, customCellModel.CellDescriptionColor);
                    rightDescriptionStatus.SetBinding(Border.BackgroundColorProperty, customCellModel.CellDescriptionColor, converter: new StatusToBackgroundColorConvertor());
                }
                rightDescriptionStatus.SetBinding(Border.IsVisibleProperty, customCellModel.CellRightContentDescription, converter: new StringToBooleanConvertorForVisibility());
                _mainGrid.Add(rightDescriptionStatus, left + 2, string.IsNullOrWhiteSpace(customCellModel.CellRightContentHeader) ? 1 : 2);
                Grid.SetRowSpan(rightDescriptionStatus, string.IsNullOrWhiteSpace(customCellModel.CellRightContentHeader) ? 2 : 1);
            }
        }

        private int ShowBandColor(CustomCellModel customCellModel, int left)
        {
            var bandBoxView = new BoxView
            {
                WidthRequest = 5,
                Margin = AppStyles.DefaultFlowDirection == FlowDirection.RightToLeft ? new Thickness(10, -_margin, -_margin, -_margin) : new Thickness(-_margin, -_margin, 10, -_margin),
                VerticalOptions = LayoutOptions.Fill,
            };
            AutomationProperties.SetIsInAccessibleTree(bandBoxView, true);
            if (!string.IsNullOrWhiteSpace(customCellModel.BandColor))
            {
                _icon.Margin = AppStyles.DefaultFlowDirection == FlowDirection.RightToLeft ? new Thickness(0, 0, -10, 0) : new Thickness(-10, 0, 0, 0);
                _mainGrid.ColumnDefinitions.Insert(0, new ColumnDefinition { Width = GridLength.Auto });
                _mainGrid.Add(bandBoxView, 0, 0);
                Grid.SetRowSpan(bandBoxView, 4);
                bandBoxView.SetBinding(BoxView.BackgroundColorProperty, customCellModel.BandColor);
                left = 1;
            }
            return left;
        }

        private int ShowDotStatus(CustomCellModel customCellModel, int left)
        {
            var dotBoxView = new Frame
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_DOT_FRAME],
                WidthRequest = 8,
               
            };
            AutomationProperties.SetIsInAccessibleTree(dotBoxView, true);
            if (!string.IsNullOrWhiteSpace(customCellModel.ShowUnreadBadge))
            {
                _mainGrid.ColumnDefinitions.Insert(left + 1, new ColumnDefinition { Width = GridLength.Auto });
                _mainGrid.Add(dotBoxView, left + 1, 1);
                dotBoxView.SetBinding(Microsoft.Maui.Controls.Frame.IsVisibleProperty, customCellModel.ShowUnreadBadge);
                _cellHeader.SetBinding(CustomLabelControl.MarginProperty, new Binding { Path = customCellModel.ShowUnreadBadge, Converter = new MarginConvertor(), ConverterParameter = nameof(customCellModel.CellHeader) });
                _cellDescription.SetBinding(CustomLabelControl.MarginProperty, new Binding { Path = customCellModel.ShowUnreadBadge, Converter = new MarginConvertor(), ConverterParameter = nameof(customCellModel.CellDescription) });
                left += 1;
            }
            return left;
        }

        private void CreateAllViews(CustomCellModel customCellModel)
        {
            _icon = new CustomImageControl(customCellModel.IconSize, customCellModel.IconSize, Constants.STRING_SPACE, Constants.STRING_SPACE, customCellModel.IsCircle);
            AutomationProperties.SetIsInAccessibleTree(_icon, true);
            if (!string.IsNullOrWhiteSpace(customCellModel.CellLeftSourceIcon))
            {
                _icon.SetBinding(CustomImageControl.SourceProperty, customCellModel.CellLeftSourceIcon);
            }
            if (!string.IsNullOrWhiteSpace(customCellModel.CellLeftIconPath))
            {
                _icon.SetBinding(CustomImageControl.ImageUrlSourceProperty, customCellModel.CellLeftIconPath);
            }
            if (!string.IsNullOrWhiteSpace(customCellModel.CellLeftDefaultIcon))
            {
                _icon.SetBinding(CustomImageControl.DefaultValueProperty, customCellModel.CellLeftDefaultIcon);
            }
            if (!string.IsNullOrWhiteSpace(customCellModel.LeftTintColor))
            {
                _icon.SetBinding(CustomImageControl.TintColorProperty, customCellModel.LeftTintColor);
            }
            _cellHeader = new CustomLabelControl(customCellModel.IsLinkHeader ? LabelType.LinkLabelSmallLeft : LabelType.ListHeaderStyle) { FontAttributes = customCellModel.IsCellHeaderInBold ? FontAttributes.Bold : FontAttributes.None };
            AutomationProperties.SetIsInAccessibleTree(_cellHeader, true);
            _cellHeader.SetBinding(CustomLabelControl.TextProperty, customCellModel.CellHeader);

            if (!string.IsNullOrWhiteSpace(customCellModel.IsUnreadHeader))
            {
                _cellHeader.SetBinding(CustomLabelControl.FontAttributesProperty, new Binding(path: customCellModel.IsUnreadHeader, converter: new BoolToFontAttributeConvertor()));
            }

            _cellDescription = new CustomLabelControl(LabelType.SecondrySmallLeftTrailTruncate);
            AutomationProperties.SetIsInAccessibleTree(_cellDescription, true);
            _cellRightContentHeader = new CustomLabelControl(LabelType.TertiarySmallRight) { FontAttributes = customCellModel.IsCellHeaderInBold ? FontAttributes.Bold : FontAttributes.None };
            AutomationProperties.SetIsInAccessibleTree(_cellRightContentHeader, true);
            _cellRightContentDescription = new CustomLabelControl(LabelType.SecondrySmallCenter) { HorizontalOptions = LayoutOptions.End };

            _cellFirstMiddleSatusContentHeader = new CustomLabelControl(LabelType.SecondrySmallCenter);

            _cellSecondMiddleSatusContentHeader = new CustomLabelControl(LabelType.SecondrySmallCenter);

            AutomationProperties.SetIsInAccessibleTree(_cellRightContentDescription, true);
            _rightIcon = new CustomImageControl(customCellModel.RightIconSize, customCellModel.RightIconSize, Constants.STRING_SPACE, Constants.STRING_SPACE, false) { HorizontalOptions = LayoutOptions.End };
            AutomationProperties.SetIsInAccessibleTree(_rightIcon, true);
        }

        /// <summary>
        /// Invoked when binding context is changed
        /// </summary>
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (BindingContext is BaseListItemModel item)
            {
                //todo:_icon.Source = ImageSource.FromStream(() => LibGenericMethods.GetMemoryStreamFromBase64(item.LeftSourceIcon));
                _icon.DefaultValue = item.LeftDefaultIcon;
                if (_imageHolder != null)
                {
                    _imageHolder.BackgroundColor = string.IsNullOrWhiteSpace(item.ImageBackgroundColor)
                        ? Colors.Transparent 
                        : Color.FromArgb(item.ImageBackgroundColor);
                }
            }
        }
    }
}