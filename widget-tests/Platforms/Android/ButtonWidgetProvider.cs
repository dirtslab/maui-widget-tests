using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Runtime;
using Android.Widget;

namespace widget_tests.Platforms.Android
{
    [Register("com.companyname.widgettests.ButtonWidgetProvider")]
    public class ButtonWidgetProvider : AppWidgetProvider
    {
        private const string ClickAction = "com.companyname.widgettests.BUTTON_WIDGET_CLICK";
        private const string PreferencesName = "button_widget_prefs";

        // When widget is refreshed/updated. This gives all instances of widget ids, and then they must all be refreshed.
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            foreach (var appWidgetId in appWidgetIds)
            {
                UpdateWidget(context, appWidgetManager, appWidgetId);
            }
        }

        // Receives the click event from the widget button. Each widget has a different click count, so we need to know which widget was clicked.
        public override void OnReceive(Context? context, Intent? intent)
        {
            base.OnReceive(context, intent);

            if (context is null || intent?.Action != ClickAction)
            {
                return;
            }

            var appWidgetId = intent.GetIntExtra(AppWidgetManager.ExtraAppwidgetId, AppWidgetManager.InvalidAppwidgetId);
            if (appWidgetId == AppWidgetManager.InvalidAppwidgetId)
            {
                return;
            }

            var nextCount = GetClickCount(context, appWidgetId) + 1;
            SetClickCount(context, appWidgetId, nextCount);

            var appWidgetManager = AppWidgetManager.GetInstance(context);
            UpdateWidget(context, appWidgetManager, appWidgetId);
        }

        // Updates the widget's UI and click count. Each widget has a different click count.
        private static void UpdateWidget(Context context, AppWidgetManager appWidgetManager, int appWidgetId)
        {
            var views = new RemoteViews(context.PackageName, Resource.Layout.button_widget);
            var clickCount = GetClickCount(context, appWidgetId);
            views.SetTextViewText(Resource.Id.button1, $"This has been clicked {clickCount} times");

            var clickIntent = new Intent(context, typeof(ButtonWidgetProvider));
            clickIntent.SetAction(ClickAction);
            clickIntent.PutExtra(AppWidgetManager.ExtraAppwidgetId, appWidgetId);

            var pendingIntent = PendingIntent.GetBroadcast(
                context,
                appWidgetId,
                clickIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            views.SetOnClickPendingIntent(Resource.Id.button1, pendingIntent);
            appWidgetManager.UpdateAppWidget(appWidgetId, views);
        }

        private static int GetClickCount(Context context, int appWidgetId)
        {
            var prefs = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
            return prefs?.GetInt(GetCountKey(appWidgetId), 0) ?? 0;
        }

        private static void SetClickCount(Context context, int appWidgetId, int count)
        {
            var prefs = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
            prefs?.Edit()?.PutInt(GetCountKey(appWidgetId), count)?.Apply();
        }

        private static string GetCountKey(int appWidgetId) => $"count_{appWidgetId}";
    }
}
