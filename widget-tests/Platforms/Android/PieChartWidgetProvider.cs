using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AColor = Android.Graphics.Color;
using APaint = Android.Graphics.Paint;
using ARectF = Android.Graphics.RectF;

namespace widget_tests.Platforms.Android
{
    [Register("com.companyname.widgettests.PieChartWidgetProvider")]
    [BroadcastReceiver(
        Exported = true,
        Enabled = true,
        Label = "Pie Chart Widget")]
    [IntentFilter(new[] { AppWidgetManager.ActionAppwidgetUpdate })]
    [MetaData("android.appwidget.provider", Resource = "@xml/pie_widget_info")]
    public class PieChartWidgetProvider : AppWidgetProvider
    {
        private const string PreferencesName = "pie_widget_prefs";
        private const string ValuesKey = "pie_values";
        private const string LabelsKey = "pie_labels";

        // Distinct slice colors
        private static readonly int[] SliceColors =
        [
            AColor.ParseColor("#FF6384"),
            AColor.ParseColor("#36A2EB"),
            AColor.ParseColor("#FFCE56"),
            AColor.ParseColor("#4BC0C0"),
        ];

        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            var (values, labels) = GetData(context);

            foreach (var id in appWidgetIds)
                UpdateWidget(context, appWidgetManager, id, values, labels);

            UpdateGeneratedPreview(context, appWidgetManager, values, labels);
        }

        /// <summary>Called from the main page whenever the user submits new data.</summary>
        public static void UpdateWidgets(Context context, float[] values, string[] labels)
        {
            SaveData(context, values, labels);

            var manager = AppWidgetManager.GetInstance(context);
            var component = new ComponentName(context, Java.Lang.Class.FromType(typeof(PieChartWidgetProvider)));
            var ids = manager.GetAppWidgetIds(component);

            foreach (var id in ids)
                UpdateWidget(context, manager, id, values, labels);

            UpdateGeneratedPreview(context, manager, values, labels);
        }

        private static void UpdateWidget(Context context, AppWidgetManager manager, int widgetId,
            float[] values, string[] labels)
        {
            var views = CreatePieRemoteViews(context, values, labels, 400, 400);
            manager.UpdateAppWidget(widgetId, views);
        }

        private static void UpdateGeneratedPreview(Context context, AppWidgetManager manager, float[] values, string[] labels)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.VanillaIceCream)
                return;

            var component = new ComponentName(context, Java.Lang.Class.FromType(typeof(PieChartWidgetProvider)));
            var preview = CreatePieRemoteViews(context, values, labels, 420, 420);

            manager.SetWidgetPreview(component, AppWidgetCategory.HomeScreen, preview);
        }

        private static RemoteViews CreatePieRemoteViews(Context context, float[] values, string[] labels, int width, int height)
        {
            var views = new RemoteViews(context.PackageName, Resource.Layout.pie_widget);
            var bitmap = DrawPieChart(values, labels, width, height);
            views.SetImageViewBitmap(Resource.Id.pie_chart_image, bitmap);
            return views;
        }

        // ── Drawing ──────────────────────────────────────────────────────────

        private static Bitmap DrawPieChart(float[] values, string[] labels, int width, int height)
        {
            var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888)!;
            var canvas = new Canvas(bitmap);

            canvas.DrawColor(AColor.ParseColor("#FF1C1C1E")); // dark background

            float total = 0;
            foreach (var v in values)
                if (v > 0) total += v;

            if (total <= 0)
            {
                // Draw placeholder circle
                using var emptyPaint = new APaint { AntiAlias = true, Color = AColor.DarkGray };
                canvas.DrawOval(GetOvalRect(width, height), emptyPaint);
                return bitmap;
            }

            float startAngle = -90f;
            var oval = GetOvalRect(width, height);

            using var slicePaint = new APaint { AntiAlias = true };
            slicePaint.SetStyle(APaint.Style.Fill);

            using var strokePaint = new APaint { AntiAlias = true, Color = AColor.ParseColor("#FF1C1C1E") };
            strokePaint.SetStyle(APaint.Style.Stroke);
            strokePaint.StrokeWidth = 3f;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] <= 0) continue;

                float sweep = (values[i] / total) * 360f;
                slicePaint.Color = new AColor(SliceColors[i % SliceColors.Length]);

                canvas.DrawArc(oval, startAngle, sweep, true, slicePaint);
                canvas.DrawArc(oval, startAngle, sweep, true, strokePaint);

                // Label in the middle of the slice
                DrawSliceLabel(canvas, labels[i], values[i], total, startAngle, sweep, oval);

                startAngle += sweep;
            }

            // Draw legend
            DrawLegend(canvas, values, labels, total, width, height);

            return bitmap;
        }

        private static ARectF GetOvalRect(int width, int height)
        {
            // Leave room at bottom for legend
            float legendHeight = 60f;
            float size = Math.Min(width, height - legendHeight);
            float left = (width - size) / 2f;
            float top = 0f;
            return new ARectF(left + 10, top + 10, left + size - 10, top + size - 10);
        }

        private static void DrawSliceLabel(Canvas canvas, string label, float value, float total,
            float startAngle, float sweep, ARectF oval)
        {
            if (sweep < 20f) return; // skip tiny slices

            float midAngle = (float)((startAngle + sweep / 2.0) * Math.PI / 180.0);
            float cx = oval.CenterX();
            float cy = oval.CenterY();
            float r = (oval.Width() / 2f) * 0.65f;

            float tx = cx + (float)Math.Cos(midAngle) * r;
            float ty = cy + (float)Math.Sin(midAngle) * r;

            using var textPaint = new APaint { AntiAlias = true, Color = AColor.White, TextSize = 22f };
            textPaint.SetStyle(APaint.Style.Fill);
            textPaint.TextAlign = APaint.Align.Center;

            float pct = (value / total) * 100f;
            string text = $"{pct:F0}%";
            canvas.DrawText(text, tx, ty + 8f, textPaint);
        }

        private static void DrawLegend(Canvas canvas, float[] values, string[] labels, float total,
            int width, int height)
        {
            float legendHeight = 60f;
            float y = height - legendHeight + 16f;
            float itemWidth = width / (float)values.Length;

            using var dotPaint = new APaint { AntiAlias = true };
            using var textPaint = new APaint { AntiAlias = true, Color = AColor.White, TextSize = 20f };
            textPaint.TextAlign = APaint.Align.Left;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] <= 0) continue;

                float x = i * itemWidth + 10f;
                dotPaint.Color = new AColor(SliceColors[i % SliceColors.Length]);
                canvas.DrawCircle(x + 10f, y, 10f, dotPaint);
                canvas.DrawText(labels[i], x + 26f, y + 7f, textPaint);
            }
        }

        // ── Persistence ───────────────────────────────────────────────────────

        private static void SaveData(Context context, float[] values, string[] labels)
        {
            var prefs = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
            var editor = prefs?.Edit();
            editor?.PutString(ValuesKey, string.Join(",", values.Select(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture))));
            editor?.PutString(LabelsKey, string.Join(",", labels));
            editor?.Apply();
        }

        private static (float[] values, string[] labels) GetData(Context context)
        {
            var prefs = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
            var valStr = prefs?.GetString(ValuesKey, "25,25,25,25") ?? "25,25,25,25";
            var lblStr = prefs?.GetString(LabelsKey, "A,B,C,D") ?? "A,B,C,D";

            var values = valStr.Split(',')
                .Select(s => float.TryParse(s, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : 0f)
                .ToArray();

            var labels = lblStr.Split(',');
            return (values, labels);
        }
    }
}
