namespace SleepJournal.Behaviors;

/// <summary>
/// Behavior that animates a visual element with a success effect (scale + fade).
/// </summary>
public class SuccessAnimationBehavior : Behavior<VisualElement>
{
    protected override void OnAttachedTo(VisualElement bindable)
    {
        base.OnAttachedTo(bindable);
        AnimateSuccess(bindable);
    }

    private async void AnimateSuccess(VisualElement element)
    {
        try
        {
            // Scale up and fade in
            await Task.WhenAll(
                element.ScaleTo(1.1, 200, Easing.CubicOut),
                element.FadeTo(1, 200)
            );

            // Scale back to normal
            await element.ScaleTo(1.0, 150, Easing.CubicIn);
        }
        catch
        {
            // Animation might fail if element is disposed
            element.Scale = 1.0;
            element.Opacity = 1.0;
        }
    }
}

/// <summary>
/// Behavior that animates a visual element when it first appears (fade in from bottom).
/// </summary>
public class FadeInBehavior : Behavior<VisualElement>
{
    public static readonly BindableProperty DurationProperty =
        BindableProperty.Create(nameof(Duration), typeof(uint), typeof(FadeInBehavior), (uint)300);

    public uint Duration
    {
        get => (uint)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    protected override void OnAttachedTo(VisualElement bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.Opacity = 0;
        bindable.TranslationY = 20;
        AnimateFadeIn(bindable);
    }

    private async void AnimateFadeIn(VisualElement element)
    {
        try
        {
            await Task.Delay(50); // Small delay to ensure element is rendered
            await Task.WhenAll(
                element.FadeTo(1, Duration, Easing.CubicOut),
                element.TranslateTo(0, 0, Duration, Easing.CubicOut)
            );
        }
        catch
        {
            element.Opacity = 1;
            element.TranslationY = 0;
        }
    }

    protected override void OnDetachingFrom(VisualElement bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.Opacity = 1;
        bindable.TranslationY = 0;
    }
}
