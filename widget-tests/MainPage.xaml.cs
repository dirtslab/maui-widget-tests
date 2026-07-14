using widget_tests.Platforms.Android;

namespace widget_tests
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void OnEntryButtonClicked(object? sender, EventArgs e)
        {
            var text = Entry1.Text ?? string.Empty;
            FieldWidgetProvider.UpdateWidgets(global::Android.App.Application.Context, text);
        }
    }
}
