using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using widget_tests.Platforms.Android;

namespace widget_tests
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HandleWidgetIntent(Intent);
            HandleLauncherWidgetIntent(Intent);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            HandleWidgetIntent(intent);
            HandleLauncherWidgetIntent(intent);
        }

        internal static void HandleWidgetIntent(Intent? intent)
        {
            if (intent is null)
                return;

            if (intent.Action == "com.companyname.widgettests.ACTION_OPEN_WITH")
            {
                var itemName = intent.GetStringExtra("item_name");
                if (!string.IsNullOrEmpty(itemName))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (Shell.Current?.CurrentPage is MainPage mainPage)
                            mainPage.OpenWithItem(itemName);
                    });
                }
                return;
            }
        }

        private static void HandleLauncherWidgetIntent(Intent? intent)
        {
            if (intent is null)
            {
                return;
            }

            var buttonNumber = intent.GetIntExtra(LauncherWidgetProvider.ButtonExtraKey, 0);
            if (buttonNumber <= 0)
            {
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Shell.Current?.CurrentPage is MainPage mainPage)
                {
                    mainPage.SetWelcomeLabelForLauncherButton(buttonNumber);
                }
            });
        }
    }
}
