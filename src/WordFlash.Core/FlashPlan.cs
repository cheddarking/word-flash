namespace WordFlash.Core;

public sealed record FlashPlan(TimeSpan Delay, OverlayPosition Position, TimeSpan Duration, Verse Verse);
