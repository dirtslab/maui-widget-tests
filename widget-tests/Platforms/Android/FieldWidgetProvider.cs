using Android.App;
using Android.Appwidget;
using Android.Content;

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
    }
}
