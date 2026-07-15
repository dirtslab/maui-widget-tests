using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Runtime;
using Android.Widget;

namespace widget_tests.Platforms.Android
{
    [Register("com.companyname.widgettests.ListWidgetProvider")]
    [BroadcastReceiver(
       Exported = true,
       Enabled = true,
       Label = "List Widget")]
    [IntentFilter(new[] { AppWidgetManager.ActionAppwidgetUpdate })]
    [MetaData("android.appwidget.provider", Resource = "@xml/list_widget_info")]
    public class ListWidgetProvider : AppWidgetProvider
    {
        private const string PreferencesName = "list_widget_prefs";
        private const string ItemsKey = "list_items";

        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            var items = GetItems(context);

            foreach (var appWidgetId in appWidgetIds)
            {
                UpdateWidget(context, appWidgetManager, appWidgetId, items);
            }
        }

        public static void AddItem(Context context, string itemName)
        {
            var items = GetItems(context).ToList();
            items.Add(itemName);
            UpdateWidgets(context, items.ToArray());
        }

        public static void RemoveItem(Context context, string itemName)
        {
            var items = GetItems(context).ToList();
            var index = items.FindLastIndex(i => string.Equals(i, itemName, StringComparison.Ordinal));

            if (index >= 0)
                items.RemoveAt(index);

            UpdateWidgets(context, items.ToArray());
        }

        public static bool RemoveLastItem(Context context, out string? removedItem)
        {
            var items = GetItems(context).ToList();

            if (items.Count == 0)
            {
                removedItem = null;
                return false;
            }

            removedItem = items[^1];
            items.RemoveAt(items.Count - 1);
            UpdateWidgets(context, items.ToArray());
            return true;
        }

        public static void UpdateWidgets(Context context, string[] items)
        {
            SaveItems(context, items);

            var appWidgetManager = AppWidgetManager.GetInstance(context);
            var componentName = new ComponentName(context, Java.Lang.Class.FromType(typeof(ListWidgetProvider)));
            var appWidgetIds = appWidgetManager.GetAppWidgetIds(componentName);

            foreach (var appWidgetId in appWidgetIds)
            {
                UpdateWidget(context, appWidgetManager, appWidgetId, items);
            }
        }

        private static void UpdateWidget(Context context, AppWidgetManager appWidgetManager, int appWidgetId, string[] itemsList)
        {
            var views = new RemoteViews(context.PackageName, Resource.Layout.list_widget);
            var items = new RemoteViews.RemoteCollectionItems.Builder();

            // A single pending intent template is required for collection-view item clicks.
            // Each item's fill-in intent supplies item-specific extras that are merged into
            // this template at click time before the activity is launched.
            var templateIntent = new Intent(context, typeof(MainActivity))
                .SetAction("com.companyname.widgettests.ACTION_OPEN_WITH")
                .AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
            // The template must be MUTABLE so that each item's fill-in intent extras
            // (e.g. "item_name") can be merged into it at click time. An immutable
            // template still launches the activity, but Android silently drops the
            // fill-in extras, so MainActivity never sees ACTION_OPEN_WITH / item_name.
            var pendingIntentTemplate = PendingIntent.GetActivity(
                context, 0, templateIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Mutable);
            views.SetPendingIntentTemplate(Resource.Id.list_widget_items, pendingIntentTemplate);

            for (var i = 0; i < itemsList.Length; i++)
            {
                var itemView = new RemoteViews(context.PackageName, Resource.Layout.list_widget_item);
                itemView.SetTextViewText(Resource.Id.list_widget_item_text, itemsList[i]);

                // The fill-in intent carries only the item-specific data.
                // It is merged with the template above when the user taps the item.
                var fillInIntent = new Intent().PutExtra("item_name", itemsList[i]);
                itemView.SetOnClickFillInIntent(Resource.Id.list_widget_item_text, fillInIntent);

                items.AddItem(i, itemView);
            }

            views.SetRemoteAdapter(
                Resource.Id.list_widget_items,
                items.SetHasStableIds(true)
                    .SetViewTypeCount(1)
                    .Build());

            appWidgetManager.UpdateAppWidget(appWidgetId, views);
        }

        private static string[] GetItems(Context context)
        {
            var prefs = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
            var storedItems = prefs?.GetString(ItemsKey, null);

            if (string.IsNullOrWhiteSpace(storedItems))
                return [];

            var items = storedItems.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return items.Length > 0 ? items : [];
        }

        private static void SaveItems(Context context, string[] items)
        {
            var prefs = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
            prefs?.Edit()?.PutString(ItemsKey, string.Join('\n', items))?.Apply();
        }
    }
}
