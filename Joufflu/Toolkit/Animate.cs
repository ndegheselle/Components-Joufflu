using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Joufflu.Toolkit;

/// <summary>
/// Attached properties that animate any <see cref="UIElement"/> without touching its template.
/// <para>
/// Set <see cref="BounceProperty"/> to <c>true</c> for a looping vertical hop — useful to draw
/// attention to an element (a notification badge, a call-to-action button). Because it is a plain
/// boolean it can be bound, so the hop starts and stops with your view model:
/// <c>toolkit:Animate.Bounce="{Binding HasUnread}"</c>.
/// </para>
/// <para>
/// The animation runs on a <see cref="TranslateTransform"/> added to the element's
/// <see cref="UIElement.RenderTransform"/>, so it never affects layout and never moves its
/// neighbours. Any transform already set is kept and composed with it, and restored when the
/// bounce stops. Tune the hop with <see cref="BounceHeightProperty"/> and
/// <see cref="BounceDurationProperty"/>.
/// </para>
/// <para>
/// The hop pauses itself whenever the element is not visible — collapsed or hidden, under a
/// collapsed ancestor, or out of the visual tree — and resumes when it comes back, so a bounce
/// left on a hidden element costs nothing. Nothing needs to be unhooked to let the element be
/// collected either, whether the bounce was stopped first or not.
/// </para>
/// </summary>
public static class Animate
{
    /// <summary>Whether the element hops continuously. Bindable: <c>false</c> stops the loop and restores the element.</summary>
    public static readonly DependencyProperty BounceProperty =
        DependencyProperty.RegisterAttached(
            "Bounce",
            typeof(bool),
            typeof(Animate),
            new PropertyMetadata(false, OnBounceChanged));

    public static bool GetBounce(DependencyObject obj) => (bool)obj.GetValue(BounceProperty);
    public static void SetBounce(DependencyObject obj, bool value) => obj.SetValue(BounceProperty, value);

    /// <summary>
    /// Height of the first hop in DIPs (defaults to 6). The two rebounds that follow it are
    /// fractions of this height, so the whole settle scales with it.
    /// </summary>
    public static readonly DependencyProperty BounceHeightProperty =
        DependencyProperty.RegisterAttached(
            "BounceHeight",
            typeof(double),
            typeof(Animate),
            new PropertyMetadata(6d, OnBounceSettingChanged));

    public static double GetBounceHeight(DependencyObject obj) => (double)obj.GetValue(BounceHeightProperty);
    public static void SetBounceHeight(DependencyObject obj, double value) => obj.SetValue(BounceHeightProperty, value);

    /// <summary>
    /// Length of one full cycle — the hop and its rebounds plus the pause before the next one
    /// (defaults to 1s). A shorter duration reads as impatient, a longer one as a slow idle nudge.
    /// </summary>
    public static readonly DependencyProperty BounceDurationProperty =
        DependencyProperty.RegisterAttached(
            "BounceDuration",
            typeof(Duration),
            typeof(Animate),
            new PropertyMetadata(new Duration(TimeSpan.FromSeconds(1)), OnBounceSettingChanged));

    public static Duration GetBounceDuration(DependencyObject obj) => (Duration)obj.GetValue(BounceDurationProperty);
    public static void SetBounceDuration(DependencyObject obj, Duration value) => obj.SetValue(BounceDurationProperty, value);

    /// <summary>
    /// The transform the bounce animates, the transform it replaced, and the clock driving it.
    /// Kept per-element so the animation can be paused, removed, and the original
    /// <see cref="UIElement.RenderTransform"/> put back.
    /// </summary>
    private static readonly DependencyProperty StateProperty =
        DependencyProperty.RegisterAttached(
            "State",
            typeof(BounceState),
            typeof(Animate),
            new PropertyMetadata(null));

    private sealed class BounceState(TranslateTransform translate, Transform? original)
    {
        public TranslateTransform Translate { get; } = translate;

        public Transform? Original { get; } = original;

        /// <summary>Replaced whenever the hop is rebuilt, so only the current clock is controlled.</summary>
        public AnimationClock? Clock { get; set; }
    }

    private static void OnBounceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if ((bool)e.NewValue)
            Start(element);
        else
            Stop(element);
    }

    // Height and duration are baked into the key frames, so a change while bouncing needs a rebuild.
    private static void OnBounceSettingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && GetBounce(element))
            Start(element);
    }

    private static void Start(UIElement element)
    {
        if (element.GetValue(StateProperty) is not BounceState state)
        {
            var translate = new TranslateTransform();
            Transform? original = element.RenderTransform;

            // Transform.Identity is the default and is frozen, so treat it as "no transform" rather
            // than composing with it; anything else is kept so the bounce doesn't drop a rotation or
            // scale the element already had.
            if (original is null || original == Transform.Identity)
            {
                original = null;
                element.RenderTransform = translate;
            }
            else
            {
                // A transform declared in XAML can be frozen; clone it before putting it in a group.
                Transform composed = original.IsFrozen ? original.Clone() : original;
                element.RenderTransform = new TransformGroup { Children = { composed, translate } };
            }

            state = new BounceState(translate, original);
            element.SetValue(StateProperty, state);
            // Only while the element is on screen: a hop nobody can see is wasted frames, and
            // nothing else ever stops it — the animation repeats forever. IsVisible covers
            // Collapsed/Hidden, a collapsed ancestor and being out of the tree altogether, in one
            // flag. The handler is a static method, so subscribing roots nothing.
            element.IsVisibleChanged += OnIsVisibleChanged;
        }

        // A clock of our own (rather than BeginAnimation) is what makes the animation pausable.
        AnimationClock clock = BuildBounce(element).CreateClock();
        state.Clock = clock;
        state.Translate.ApplyAnimationClock(TranslateTransform.YProperty, clock);

        // An element gets its first IsVisibleChanged only once it is rendered, so a bounce set in
        // XAML starts out invisible and is resumed by the handler below.
        if (!element.IsVisible)
            clock.Controller?.Pause();
    }

    private static void Stop(UIElement element)
    {
        if (element.GetValue(StateProperty) is not BounceState state)
            return;

        element.IsVisibleChanged -= OnIsVisibleChanged;
        // Passing null removes the animation and lets the property fall back to its local value (0).
        state.Translate.ApplyAnimationClock(TranslateTransform.YProperty, null);
        state.Clock = null;
        element.RenderTransform = state.Original ?? Transform.Identity;
        element.SetValue(StateProperty, null);
    }

    // Hidden, collapsed, or no longer in the tree: hold the clock rather than let it tick against
    // nothing. It resumes mid-hop when the element comes back, which is invisible to the eye.
    private static void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not UIElement element || element.GetValue(StateProperty) is not BounceState state)
            return;

        ClockController? controller = state.Clock?.Controller;
        if (controller is null)
            return;

        if ((bool)e.NewValue)
        {
            controller.Resume();
        }
        else
        {
            controller.Pause();
            // Park it at rest rather than mid-air: the element would otherwise come back offset
            // for a frame before the next hop brings it down.
            controller.Seek(TimeSpan.Zero, TimeSeekOrigin.BeginTime);
        }
    }

    /// <summary>
    /// Height of each rebound as a fraction of <see cref="BounceHeightProperty"/>. A ball loses
    /// the same share of its energy on every impact, so the heights fall off geometrically —
    /// the second rebound is to the first what the first is to the hop.
    /// </summary>
    private static readonly double[] ReboundHeights = [0.42, 0.18];

    /// <summary>
    /// Key times of the settle, as fractions of the cycle: the element rises to the full height
    /// and lands (0 → 0.48), then rebounds twice, each shorter than the last because a lower hop
    /// spends less time in the air. What is left of the cycle is the idle tail.
    /// </summary>
    private static readonly double[] ReboundPeakTimes = [0.58, 0.74];
    private static readonly double[] ReboundLandTimes = [0.68, 0.80];

    /// <summary>
    /// Up, down, twice again smaller, then wait. Each rise decelerates into its peak
    /// (<c>EaseOut</c>) and each fall accelerates into the landing (<c>EaseIn</c>), so the element
    /// reads as settling under gravity rather than pulsing; it then holds still for the rest of
    /// the cycle.
    /// </summary>
    private static DoubleAnimationUsingKeyFrames BuildBounce(UIElement element)
    {
        Duration duration = GetBounceDuration(element);
        // A cycle can be set to Automatic/Forever, which has no TimeSpan to take fractions of.
        TimeSpan cycle = duration.HasTimeSpan ? duration.TimeSpan : TimeSpan.FromSeconds(1);
        double height = GetBounceHeight(element);

        var animation = new DoubleAnimationUsingKeyFrames
        {
            Duration = cycle,
            RepeatBehavior = RepeatBehavior.Forever,
            KeyFrames =
            {
                new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0)),
                Up(-height, 0.24),
                Down(0.48),
            },
        };

        for (int i = 0; i < ReboundHeights.Length; i++)
        {
            animation.KeyFrames.Add(Up(-height * ReboundHeights[i], ReboundPeakTimes[i]));
            animation.KeyFrames.Add(Down(ReboundLandTimes[i]));
        }

        // Idle tail: keeps the element at rest until the cycle restarts.
        animation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromPercent(1)));

        return animation;

        static EasingDoubleKeyFrame Up(double peak, double at) =>
            new(peak, KeyTime.FromPercent(at)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };

        static EasingDoubleKeyFrame Down(double at) =>
            new(0, KeyTime.FromPercent(at)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };
    }
}
