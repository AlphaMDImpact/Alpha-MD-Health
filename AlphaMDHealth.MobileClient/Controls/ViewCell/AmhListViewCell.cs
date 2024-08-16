using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

internal class AmhListViewCell : Grid
{
    internal AmhListViewCell(AmhViewCellModel sourceFields, bool isSelectedViewCell, EventHandler<EventArgs> onRightViewClicked)
    {
        Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE];
        if (isSelectedViewCell)
        {
            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_TERTIARY_APP_COLOR];
        }

        AddColorBand(sourceFields);

        Grid container = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_LIST_VIEWCELL_GRID_STYLE]
        };

        int totalRows = DefineLayout(container, sourceFields);
        AddImage(container, totalRows, 0, sourceFields.LeftFieldType, sourceFields.LeftField, sourceFields.LeftImage, sourceFields.LeftIcon, string.Empty, null);
        AddField(container, sourceFields.LeftHeader, sourceFields.LeftHeaderField, sourceFields.LeftHeaderFieldType, 1, 0);
        AddField(container, sourceFields.LeftDescription, sourceFields.LeftDescriptionField, sourceFields.LeftDescriptionFieldType, 1, 1);
        AddField(container, sourceFields.RightHeader, sourceFields.RightHeaderField, sourceFields.RightHeaderFieldType, 2, 0);
        AddField(container, sourceFields.RightDescription, sourceFields.RightDescriptionField, sourceFields.RightDescriptionFieldType, 2, 1);
        AddImage(container, totalRows, 3, sourceFields.RightFieldType, sourceFields.RightField, sourceFields.RightImage, sourceFields.RightIcon, sourceFields.RightHTMLLabelField, onRightViewClicked);
        this.Add(container, 1, 0);
    }

    private void AddColorBand(AmhViewCellModel sourceFields)
    {
        var band = new BoxView
        {
            WidthRequest = 5,
            VerticalOptions = LayoutOptions.Fill,
        };
        if (!string.IsNullOrWhiteSpace(sourceFields.BandColor))
        {
            band.SetBinding(BoxView.ColorProperty, sourceFields.BandColor, converter: new StringToColorConverter());
        }
        this.Add(band, 0, 0);
    }

    private void AddImage(Grid container, int totalRows, int col, FieldTypes fieldType, string fieldTypeString, string value, string icon, string html, EventHandler<EventArgs> onRightViewClicked)
    {
        AmhBaseControl field = null;
        if (col != 0 && !string.IsNullOrWhiteSpace(html))
        {
            field = new AmhLabelControl(FieldTypes.HtmlSecondaryLabelControl);
            field.SetBinding(AmhImageControl.ValueProperty, html);
        }
        else if (!string.IsNullOrWhiteSpace(value) || !string.IsNullOrWhiteSpace(icon))
        {
            var size = totalRows > 1 && col != 3 ? AppImageSize.ImageSizeXL : AppImageSize.ImageSizeM;
            field = CreateBindableField(fieldType, fieldTypeString, value, icon, size);
        }
        if (field != null)
        {
            field.VerticalOptions = LayoutOptions.CenterAndExpand;
            container.Add(field, col, 0);
            if (totalRows > 1)
            {
                container.SetRowSpan(field, totalRows);
            }
            if (onRightViewClicked != null)
            {
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (sender, e) =>
                {
                    onRightViewClicked?.Invoke(sender, e);
                };
                field.GestureRecognizers.Add(tapGestureRecognizer);
            }
        }
    }

    private void AddField(Grid container, string fieldData, string fieldTypeString, FieldTypes fieldType, int cell, int row)
    {
        if (!string.IsNullOrWhiteSpace(fieldData))
        {
            AmhFieldTypeMapper dynamicField = CreateBindableField(fieldType, fieldTypeString, fieldData, string.Empty);
            container.Add(dynamicField, cell, row);
        }
    }

    private AmhFieldTypeMapper CreateBindableField(FieldTypes fieldType, string fieldTypeString, string value, string icon, AppImageSize size = default)
    {
        AmhFieldTypeMapper field = new AmhFieldTypeMapper(fieldType);
        if (size != default)
        {
            field.ImageWidth = size;
            field.ImageHeight = size;
        };
        if (!string.IsNullOrWhiteSpace(value))
        {
            field.SetBinding(AmhFieldTypeMapper.ValueProperty, value);
        }
        if (!string.IsNullOrWhiteSpace(icon))
        {
            field.SetBinding(AmhFieldTypeMapper.IconProperty, icon);
        }
        if (!string.IsNullOrWhiteSpace(fieldTypeString))
        {
            field.SetBinding(AmhFieldTypeMapper.FieldTypeProperty, fieldTypeString);
        }
        return field;
    }

    private int DefineLayout(Grid container, AmhViewCellModel sourceFields)
    {
        RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = GridLength.Auto } };
        ColumnDefinitions = new ColumnDefinitionCollection {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Star }
        };

        var hasLeftImage = HasLeftImageCell(sourceFields);
        int totalRows = GetTotalRows(sourceFields);
        int totalCells = GetTotalCells(sourceFields, hasLeftImage);

        container.RowDefinitions = new RowDefinitionCollection();
        for (int row = 0; row < totalRows; row++)
        {
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        };

        container.ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Auto } };
        for (int cell = 0; cell < totalCells; cell++)
        {
            container.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = (hasLeftImage && cell == 1) || (!hasLeftImage && cell == 0)
                    ? GridLength.Star
                    : GridLength.Auto
            });
        };

        return totalRows;
    }

    private int GetTotalRows(AmhViewCellModel sourceFields)
    {
        return (HasHeaderRow(sourceFields) ? 1 : 0)
            + (HasDescriptionRow(sourceFields) ? 1 : 0);
    }

    private int GetTotalCells(AmhViewCellModel sourceFields, bool hasLeftImage)
    {
        return (hasLeftImage ? 1 : 0)
            + (HasLeftContentCell(sourceFields) ? 1 : 0)
            + (HasRightContentCell(sourceFields) ? 1 : 0)
            + (HasRightImageCell(sourceFields) ? 1 : 0);
    }

    private bool HasRightImageCell(AmhViewCellModel sourceFields)
    {
        return !string.IsNullOrWhiteSpace(sourceFields.RightImage)
            || !string.IsNullOrWhiteSpace(sourceFields.RightIcon)
            || !string.IsNullOrWhiteSpace(sourceFields.RightHTMLLabelField);
    }

    private bool HasRightContentCell(AmhViewCellModel sourceFields)
    {
        return !string.IsNullOrWhiteSpace(sourceFields.RightHeader)
            || !string.IsNullOrWhiteSpace(sourceFields.RightDescription);
    }

    private bool HasLeftContentCell(AmhViewCellModel sourceFields)
    {
        return !string.IsNullOrWhiteSpace(sourceFields.LeftHeader)
            || !string.IsNullOrWhiteSpace(sourceFields.LeftDescription);
    }

    private bool HasLeftImageCell(AmhViewCellModel sourceFields)
    {
        return !string.IsNullOrWhiteSpace(sourceFields.LeftImage)
            || !string.IsNullOrWhiteSpace(sourceFields.LeftIcon);
    }

    private bool HasDescriptionRow(AmhViewCellModel sourceFields)
    {
        return !string.IsNullOrWhiteSpace(sourceFields.LeftDescription)
            || !string.IsNullOrWhiteSpace(sourceFields.RightDescription);
    }

    private bool HasHeaderRow(AmhViewCellModel sourceFields)
    {
        return !string.IsNullOrWhiteSpace(sourceFields.LeftHeader)
            || !string.IsNullOrWhiteSpace(sourceFields.RightHeader);
    }
}