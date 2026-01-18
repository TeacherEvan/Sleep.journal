using SleepJournal.ViewModels;

namespace SleepJournal.Views;

public partial class WelcomePage : ContentPage
{
    public WelcomePage(WelcomePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object? sender, EventArgs e)
    {
        // Staggered fade-in animations for a smooth entrance
        await Task.WhenAll(
            TitleLabel.FadeTo(1, 800, Easing.CubicOut),
            TitleLabel.TranslateTo(0, 0, 800, Easing.CubicOut)
        );

        await SubtitleLabel.FadeTo(1, 600, Easing.CubicOut);

        // Animate features with stagger
        await Task.Delay(200);
        var feature1Task = AnimateFeature(Feature1, 0);
        await Task.Delay(150);
        var feature2Task = AnimateFeature(Feature2, 150);
        await Task.Delay(150);
        var feature3Task = AnimateFeature(Feature3, 300);

        await Task.WhenAll(feature1Task, feature2Task, feature3Task);

        // Animate button
        await Task.WhenAll(
            GetStartedButton.FadeTo(1, 600, Easing.CubicOut),
            GetStartedButton.ScaleTo(1, 600, Easing.SpringOut)
        );

        // Subtle pulse animation on hint
        await HintLabel.FadeTo(0.7, 800, Easing.SinInOut);
        _ = PulseHintLabel();
    }

    private async Task AnimateFeature(View feature, int delay)
    {
        feature.TranslationX = -30;
        feature.Opacity = 0;

        await Task.Delay(delay);

        await Task.WhenAll(
            feature.FadeTo(1, 500, Easing.CubicOut),
            feature.TranslateTo(0, 0, 500, Easing.CubicOut)
        );
    }

    private async Task PulseHintLabel()
    {
        while (true)
        {
            await HintLabel.FadeTo(0.4, 2000, Easing.SinInOut);
            await HintLabel.FadeTo(0.7, 2000, Easing.SinInOut);
        }
    }
}
