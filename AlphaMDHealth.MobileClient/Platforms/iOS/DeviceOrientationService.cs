using System;
using Foundation;
using UIKit;

namespace AlphaMDHealth.MobileClient;

public partial class DeviceOrientationService
{
    private static readonly IReadOnlyDictionary<DisplayOrientation, UIInterfaceOrientation> _iosDisplayOrientationMap = new Dictionary<DisplayOrientation, UIInterfaceOrientation>
    {
        [DisplayOrientation.Landscape] = UIInterfaceOrientation.LandscapeLeft,
        [DisplayOrientation.Portrait] = UIInterfaceOrientation.Portrait,
    };
    public partial void SetDeviceOrientation(DisplayOrientation displayOrientation)
    {
        if (_iosDisplayOrientationMap.TryGetValue(displayOrientation, out var iosOrientation))
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(16, 0))
            {
                var scene = (UIApplication.SharedApplication.ConnectedScenes.ToArray()[0] as UIWindowScene);
                if (scene != null)
                {
                    var uiAppplication = UIApplication.SharedApplication;
                    var test = UIApplication.SharedApplication.KeyWindow?.RootViewController;
                    if (test != null)
                    {
                        UIInterfaceOrientationMask NewOrientation;
                        if (iosOrientation == UIInterfaceOrientation.Portrait)
                        {
                            NewOrientation = UIInterfaceOrientationMask.Portrait;
                        }
                        else
                        {
                            NewOrientation = UIInterfaceOrientationMask.LandscapeLeft;
                        }
                        scene.Title = "PerformOrientation";
                        scene.RequestGeometryUpdate(
                            new UIWindowSceneGeometryPreferencesIOS(NewOrientation), error => { System.Diagnostics.Debug.WriteLine(error.ToString()); });
                        test.SetNeedsUpdateOfSupportedInterfaceOrientations();
                        test.NavigationController?.SetNeedsUpdateOfSupportedInterfaceOrientations();
                        //await Task.Delay(1000); //Gives the time to apply the view rotation
                        scene.Title = "";
                    }
                }
            }
            else
            {
                UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)iosOrientation), new NSString("orientation"));
            }
        }
    }
}
