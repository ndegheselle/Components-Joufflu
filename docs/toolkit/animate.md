---
title: Animate
parent: Toolkit
nav_order: 10
---

# Animate

## Animate.Bounce

`Bounce` loops a vertical hop on any element — a notification badge, a
call-to-action button — without touching its template.

The element rises to the full height, lands, then rebounds twice at roughly 42%
and 18% of it before coming to rest: heights fall off geometrically and each
rebound spends less time in the air than the last, the way something bouncing
loses the same share of its energy on every impact. That settle is what keeps it
from reading as a mechanical up-down pulse.

It animates a `TranslateTransform` added to the element's `RenderTransform`, so
it never affects layout and never moves its neighbours. `BounceHeight` sets how
far the first hop rises (in DIPs, default `6`), the rebounds scaling with it,
and `BounceDuration` the length of a full cycle (default `0:0:1`) — the hop and
its rebounds plus the pause before the next one, so a longer duration reads as a
slow idle nudge rather than a slower hop.

```xml
<!-- Bounce loops a vertical hop on any element, template untouched -->
<Button toolkit:Animate.Bounce="True">Look at me</Button>

<!-- Tune the hop: height in DIPs, duration of a full cycle (hop + pause) -->
<fonts:FontIcon
    toolkit:Animate.Bounce="True"
    toolkit:Animate.BounceHeight="10"
    toolkit:Animate.BounceDuration="0:0:0.6"
    Text="{x:Static fonts:LucideFontIcons.Bell}" />

<!-- Hidden elements pause their own hop: no frames spent on a -->
<!-- bounce nobody can see, resumed when the element is back   -->
<Border Visibility="Collapsed" toolkit:Animate.Bounce="True" />
```

`Bounce` is a plain boolean, so bind it to a view-model flag to start and stop
the loop at runtime.

```xml
<!-- Bounce is a plain bool, so bind it to start and stop the loop -->
<feedback:Badge toolkit:Animate.Bounce="{Binding HasUnread}">3</feedback:Badge>

<!-- A transform already on the element is kept and composed with -->
<!-- the bounce, then restored once Bounce goes back to False     -->
<Border toolkit:Animate.Bounce="{Binding HasUnread}">
    <Border.RenderTransform>
        <RotateTransform Angle="8" />
    </Border.RenderTransform>
</Border>
```

{: .note }
> `BounceHeight` and `BounceDuration` are baked into the animation's key frames,
> so changing either while the element is bouncing restarts the hop from the top
> of its cycle.

## Hidden elements and lifetime

The hop pauses itself whenever the element is not visible — `Collapsed` or
`Hidden`, under a collapsed ancestor, or out of the visual tree — and resumes
where it left off when the element comes back, parked at rest in the meantime.
A bounce left on a hidden element therefore costs no frames.

Nothing has to be unhooked for the element to be collected: the helper keeps no
reference back to it, and an element dropped with `Bounce` still `True` is
collected exactly like one that was stopped first — WPF retires the clock once
its target goes away.
