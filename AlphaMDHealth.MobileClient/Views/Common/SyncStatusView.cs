using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// View used to display sync status
/// </summary>
public class SyncStatusView : ContentView, IDisposable
{
    private readonly SvgImageView _progressImage;
    private readonly CustomLabelControl _progressLabel;
    private CancellationTokenSource _tokenSource;
    private bool _isImageRotating;

    /// <summary>
    /// Default constructor of sync in progress view
    /// </summary>
    public SyncStatusView()
    {
        IsVisible = false;
        _progressLabel = new CustomLabelControl(LabelType.SecondryAppExtarSmallLeft)
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.WordWrap,
            Padding = new Thickness(0, (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING] / 2)
        };
        _progressImage = new SvgImageView(ImageConstants.I_SYNC_ICON_PNG, AppImageSize.ImageSizeXS, AppImageSize.ImageSizeXS, _progressLabel.TextColor)
        {
            HorizontalOptions = LayoutOptions.End,
        };
        var contentView = new Grid
        {
            RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = GridLength.Auto } },
            ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } },
            Padding = new Thickness((double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], 0),
            ColumnSpacing = 5,
            VerticalOptions = LayoutOptions.CenterAndExpand,
            HorizontalOptions = LayoutOptions.CenterAndExpand,
        };
        contentView.Add(_progressImage, 0, 0);
        contentView.Add(_progressLabel, 1, 0);
        Content = contentView;
        VerticalOptions = LayoutOptions.CenterAndExpand;
        HorizontalOptions = LayoutOptions.FillAndExpand;
        BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_TEXT_COLOR];
    }

    /// <summary>
    /// Dispose sync progress
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose sync progress
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        _tokenSource?.Dispose();
    }

    /// <summary>
    /// Update sync inprogress view
    /// </summary>
    /// <param name="isInternetConnected">true if is for sync else false</param>
    /// <param name="message">Text to be displayed</param>
    /// <param name="isLeftAligned">true if is to be left aligned</param>
    public void UpdateShowBackgroundSyncField(bool isInternetConnected, string message, bool isLeftAligned)
    {
        if (isInternetConnected)
        {
            _progressLabel.Text = message;
            if (BasePage.SyncCount > 0)
            {
                ShowSyncProgress();
            }
            else
            {
                IsVisible = false;
            }
        }
        else
        {
            _progressImage.IsVisible = false;
            _progressLabel.Text = message;
            (Content as Grid).ColumnSpacing = 0;
            if (isLeftAligned)
            {
                (Content as Grid).HorizontalOptions = LayoutOptions.StartAndExpand;
            }
            IsVisible = true;
        }
    }

    /// <summary>
    /// Changes the Visibility of Progress Bar in case of Background Sync
    /// </summary>
    public void ShowSyncProgress()
    {
        bool showLabel = BasePage.SyncCount > 0;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!_isImageRotating)
            {
                _isImageRotating = true;
                RotateProgress();
            }
            IsVisible = showLabel;
        });
    }

    private void RotateProgress()
    {
        if (BasePage.SyncCount > 0)
        {
            if (_tokenSource == null)
            {
                _tokenSource = new CancellationTokenSource();
                CancellationToken token = _tokenSource.Token;
                Task.Factory.StartNew(async () =>
                {
                    while (BasePage.SyncCount > 0)
                    {
                        await _progressImage.RotateTo(360, 800, Easing.Linear).ConfigureAwait(true);
                        await _progressImage.RotateTo(0, 0).ConfigureAwait(true);
                    }
                }, token);
            }
        }
        else
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
                _tokenSource = null;
            }
        }
    }
}