using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Runtime;
using Android.Widget;

namespace widget_tests.Platforms.Android
{
    [BroadcastReceiver(
        Exported = true,
        Enabled = true,
        Label = "Field Widget")]
    [IntentFilter(new[] { AppWidgetManager.ActionAppwidgetUpdate })]
    [MetaData("android.appwidget.provider", Resource = "@xml/field_widget_info")]
    public class FieldWidgetProvider : AppWidgetProvider
    {
        private const string PreferencesName = "field_widget_prefs";
        private const string WidgetTextKey = "widget_text";

        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            var widgetText = GetWidgetText(context);

            foreach (var appWidgetId in appWidgetIds)
            {
                UpdateWidget(context, appWidgetManager, appWidgetId, widgetText);
            }
        }

        public static void UpdateWidgets(Context context, string text)
        {
            SetWidgetText(context, text);

            var appWidgetManager = AppWidgetManager.GetInstance(context);
            var componentName = new ComponentName(context, Java.Lang.Class.FromType(typeof(FieldWidgetProvider)));
            var appWidgetIds = appWidgetManager.GetAppWidgetIds(componentName);

            foreach (var appWidgetId in appWidgetIds)
            {
                UpdateWidget(context, appWidgetManager, appWidgetId, text);
            }
        }

        private static void UpdateWidget(Context context, AppWidgetManager appWidgetManager, int appWidgetId, string text)
        {
            var views = new RemoteViews(context.PackageName, Resource.Layout.field_widget);
            views.SetTextViewText(Resource.Id.widget_text, text);
            appWidgetManager.UpdateAppWidget(appWidgetId, views);
        }

        private static string GetWidgetText(Context context)
        {
            var prefs = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
            return prefs?.GetString(WidgetTextKey, "Widget Text") ?? "Widget Text";
        }

        private static void SetWidgetText(Context context, string text)
        {
            var prefs = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
            prefs?.Edit()?.PutString(WidgetTextKey, text)?.Apply();
        }
    }
}
