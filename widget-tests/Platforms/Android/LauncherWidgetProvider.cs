using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;

namespace widget_tests.Platforms.Android
{
    [BroadcastReceiver(
       Exported = true,
       Enabled = true,
       Label = "Launcher Widget")]
    [IntentFilter(new[] { AppWidgetManager.ActionAppwidgetUpdate })]
    [MetaData("android.appwidget.provider", Resource = "@xml/launcher_widget_info")]
    internal class LauncherWidgetProvider : AppWidgetProvider
    {
        public const string ButtonExtraKey = "launcher_widget_button";

        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            foreach (var appWidgetId in appWidgetIds)
            {
                UpdateWidget(context, appWidgetManager, appWidgetId);
            }
        }

        private static void UpdateWidget(Context context, AppWidgetManager appWidgetManager, int appWidgetId)
        {
            var views = new RemoteViews(context.PackageName, Resource.Layout.launcher_widget);

            views.SetOnClickPendingIntent(Resource.Id.button1, CreateLaunchPendingIntent(context, appWidgetId, 1));
            views.SetOnClickPendingIntent(Resource.Id.button2, CreateLaunchPendingIntent(context, appWidgetId, 2));
            views.SetOnClickPendingIntent(Resource.Id.button3, CreateLaunchPendingIntent(context, appWidgetId, 3));
            views.SetOnClickPendingIntent(Resource.Id.button4, CreateLaunchPendingIntent(context, appWidgetId, 4));

            appWidgetManager.UpdateAppWidget(appWidgetId, views);
        }

        private static PendingIntent CreateLaunchPendingIntent(Context context, int appWidgetId, int buttonNumber)
        {
            var launchIntent = new Intent(context, typeof(MainActivity));

            launchIntent.PutExtra(ButtonExtraKey, buttonNumber);

            var requestCode = (appWidgetId * 10) + buttonNumber;
            return PendingIntent.GetActivity(
                context,
                requestCode,
                launchIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        }
    }
}
