
namespace AlphaMDHealth.VideoLibrary
{
    /// <summary>
    /// View which handle multiple video call participants
    /// </summary>
    public class MultipleParticipantView : Grid
    {
        private readonly Grid _participantViews;
        private View _localParticipantVideo;

        /// <summary>
        /// View handling multiple video call participants
        /// </summary>
        public MultipleParticipantView()
        {
            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.FillAndExpand;
            ColumnSpacing = 0;
            RowSpacing = 0;
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            };
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            };
            _participantViews = new Grid
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                ColumnSpacing = 0,
                RowSpacing = 0,
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                }
            };
            Children.Add(_participantViews);//, 0, 0);
        }

        /// <summary>
        /// Adds local participant view when connected
        /// </summary>
        /// <param name="participantVideo">Local participant view</param>
        public void AddLocalParticipant(View participantVideo)
        {
            _localParticipantVideo = participantVideo;
            _localParticipantVideo.HorizontalOptions = LayoutOptions.FillAndExpand;
            _localParticipantVideo.VerticalOptions = LayoutOptions.FillAndExpand;
            Children.Add(_localParticipantVideo);//, 0, 0);
            UpdateLocalVideoView();
        }

        /// <summary>
        /// Adds remote participant when connected
        /// </summary>
        /// <param name="participantVideo">Remote participant view</param>
        public void AddParticipant(View participantVideo)
        {
            int rowCount = _participantViews.RowDefinitions.Count;
            int colCount = _participantViews.ColumnDefinitions.Count;
            bool isLandscape = DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height;
            if (_participantViews.Children.Count >= rowCount * colCount)
            {
                if ((isLandscape && rowCount != colCount) || (!isLandscape && rowCount == colCount))
                {
                    _participantViews.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    _participantViews.Children.Add(participantVideo);//, 0, rowCount);
                }
                else
                {
                    _participantViews.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    _participantViews.Children.Add(participantVideo);//, colCount, 0);
                }
            }
            else
            {
                if (isLandscape)
                {
                    _participantViews.Children.Add(participantVideo);//, colCount - ((rowCount * colCount) - _participantViews.Children.Count), rowCount - 1);
                }
                else
                {
                    _participantViews.Children.Add(participantVideo);//, colCount - 1, rowCount - ((rowCount * colCount) - _participantViews.Children.Count));
                }
            }
            UpdateLocalVideoView();
        }

        /// <summary>
        /// Removes remote participant when disconnected
        /// </summary>
        /// <param name="participantVideo">Remote participant view</param>
        public void RemoveParticipant(View participantVideo)
        {
            _participantViews.Children.Remove(participantVideo);
            int rowCount = _participantViews.RowDefinitions.Count;
            int colCount = _participantViews.ColumnDefinitions.Count;
            HandleOrientation(rowCount, colCount);
            UpdateLocalVideoView();
        }

        private void HandleOrientation(int rowCount, int colCount)
        {
            bool isLandscape = DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height;
            if ((isLandscape && colCount <= rowCount) || (!isLandscape && rowCount <= colCount))
            {
                if (IsRowColumnRemovedRequired(rowCount, colCount, isLandscape))
                {
                    if (isLandscape)
                    {
                        _participantViews.RowDefinitions.RemoveAt(rowCount - 1);
                    }
                    else
                    {
                        _participantViews.ColumnDefinitions.RemoveAt(colCount - 1);
                    }
                }
            }
            else
            {
                if (isLandscape)
                {
                    if (IsDefinitionRemovalRequired(colCount, rowCount))
                    {
                        _participantViews.ColumnDefinitions.RemoveAt(colCount - 1);
                        colCount--;
                    }
                }
                else
                {
                    if (IsDefinitionRemovalRequired(rowCount, colCount))
                    {
                        _participantViews.RowDefinitions.RemoveAt(rowCount - 1);
                        rowCount--;
                    }
                }
            }
            UpdateRemoteParticipantViewPositions(rowCount, colCount, isLandscape);
        }

        private bool IsDefinitionRemovalRequired(int preferedDefinitionCount, int secondaryDefinitionCount)
        {
            return (preferedDefinitionCount - 1 > -1) && (preferedDefinitionCount - 1) * secondaryDefinitionCount >= _participantViews.Children.Count;
        }

        private bool IsRowColumnRemovedRequired(int rowCount, int colCount, bool isLandscape)
        {
            return (isLandscape && (rowCount - 1 > -1) && colCount * (rowCount - 1) >= _participantViews.Children.Count) || (!isLandscape && (colCount - 1 > -1) && rowCount * (colCount - 1) >= _participantViews.Children.Count);
        }

        private void UpdateRemoteParticipantViewPositions(int rowCount, int colCount, bool isLandscape)
        {
            int i = 0;
            int j = 0;
            foreach (IView child in _participantViews.Children)
            {
                int countToConsider = rowCount;
                if (isLandscape)
                {
                    countToConsider = colCount;
                    SetRow(child, i);
                    SetColumn(child, j);
                }
                else
                {
                    SetColumn(child, i);
                    SetRow(child, j);
                }
                if (j < countToConsider - 1)
                {
                    j++;
                }
                else
                {
                    j = 0;
                    i++;
                }
            }
        }

        private void UpdateLocalVideoView()
        {
            if (_participantViews.Children.Count > 0)
            {
                if (_localParticipantVideo.WidthRequest < 0)
                {
                    _localParticipantVideo.HorizontalOptions = LayoutOptions.End;
                    _localParticipantVideo.VerticalOptions = LayoutOptions.End;
                    if(DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height)
                    {
                        _localParticipantVideo.HeightRequest = 0.15 * DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
                        _localParticipantVideo.WidthRequest = _localParticipantVideo.HeightRequest * 1.77;
                    }
                    else
                    {
                        _localParticipantVideo.WidthRequest = 0.2 * DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
                        _localParticipantVideo.HeightRequest = _localParticipantVideo.WidthRequest * 1.77;
                    }
                }
            }
            else
            {
                if (_localParticipantVideo.WidthRequest > 0)
                {
                    _localParticipantVideo.HorizontalOptions = LayoutOptions.FillAndExpand;
                    _localParticipantVideo.VerticalOptions = LayoutOptions.FillAndExpand;
                    _localParticipantVideo.WidthRequest = -1;
                    _localParticipantVideo.HeightRequest = -1;
                }
            }
        }
    }
}
