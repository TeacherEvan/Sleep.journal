using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace SleepJournal.Behaviors;

/// <summary>
/// Behavior that creates a twinkling stars effect in the background.
/// </summary>
public class TwinkleStarsBehavior : Behavior<BoxView>
{
    private BoxView? _attachedElement;
    private AbsoluteLayout? _starContainer;
    private readonly Random _random = new();

    protected override void OnAttachedTo(BoxView bindable)
    {
        base.OnAttachedTo(bindable);
        _attachedElement = bindable;

        bindable.Loaded += OnLoaded;
    }

    protected override void OnDetachingFrom(BoxView bindable)
    {
        base.OnDetachingFrom(bindable);

        if (bindable != null)
        {
            bindable.Loaded -= OnLoaded;
        }

        _attachedElement = null;
        _starContainer = null;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (_attachedElement?.Parent is Grid parentGrid)
        {
            CreateStarField(parentGrid);
        }
    }

    private void CreateStarField(Grid parentGrid)
    {
        _starContainer = new AbsoluteLayout
        {
            InputTransparent = true,
            ZIndex = -1
        };

        // Add star container to parent grid (spanning all rows)
        Grid.SetRowSpan(_starContainer, 3);
        parentGrid.Children.Insert(0, _starContainer);

        // Create stars
        for (int i = 0; i < 50; i++)
        {
            CreateStar();
        }
    }

    private void CreateStar()
    {
        if (_starContainer == null) return;

        var size = _random.Next(1, 4);
        var star = new Ellipse
        {
            Fill = new SolidColorBrush(Colors.White),
            WidthRequest = size,
            HeightRequest = size,
            Opacity = 0
        };

        var x = _random.NextDouble();
        var y = _random.NextDouble();

        AbsoluteLayout.SetLayoutBounds(star, new Rect(x, y, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
        AbsoluteLayout.SetLayoutFlags(star, AbsoluteLayoutFlags.PositionProportional);

        _starContainer.Children.Add(star);

        // Animate twinkling
        AnimateStar(star);
    }

    private async void AnimateStar(Ellipse star)
    {
        var delay = _random.Next(0, 3000);
        await Task.Delay(delay);

        while (true)
        {
            try
            {
                var duration = (uint)_random.Next(1000, 3000);
                var maxOpacity = _random.NextDouble() * 0.6 + 0.2; // 0.2 to 0.8

                await star.FadeTo(maxOpacity, duration, Easing.SinInOut);
                await star.FadeTo(0, duration, Easing.SinInOut);

                // Random pause between twinkles
                await Task.Delay(_random.Next(500, 2000));
            }
            catch
            {
                // Element might be disposed
                break;
            }
        }
    }
}
