using widget_tests.Platforms.Android;

namespace widget_tests
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnUpdateWidgetClicked(object? sender, EventArgs e)
        {
            var labels = new[]
            {
                Label1.Text ?? "A",
                Label2.Text ?? "B",
                Label3.Text ?? "C",
                Label4.Text ?? "D",
            };

            var values = new[]
            {
                ParseValue(Value1.Text),
                ParseValue(Value2.Text),
                ParseValue(Value3.Text),
                ParseValue(Value4.Text),
            };

            if (values.All(v => v <= 0))
            {
                StatusLabel.TextColor = Colors.Red;
                StatusLabel.Text = "Please enter at least one positive value.";
                return;
            }

            PieChartWidgetProvider.UpdateWidgets(
                global::Android.App.Application.Context, values, labels);

            StatusLabel.TextColor = Colors.Green;
            StatusLabel.Text = "Widget updated!";
        }

        private static float ParseValue(string? text)
        {
            if (float.TryParse(text, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var result) && result > 0)
                return result;
            return 0f;
        }
    }
}
