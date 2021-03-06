﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects;
using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Modes.Osu.Objects.Drawables;
using osu.Game.Modes.Objects.Types;
using OpenTK.Graphics;

namespace osu.Game.Modes.Osu.Objects
{
    public abstract class OsuHitObject : HitObject, IHasCombo, IHasPosition
    {
        public const double OBJECT_RADIUS = 64;

        private const double hittable_range = 300;
        private const double hit_window_50 = 150;
        private const double hit_window_100 = 80;
        private const double hit_window_300 = 30;

        public Vector2 Position { get; set; }

        public Vector2 StackedPosition => Position + StackOffset;

        public virtual Vector2 EndPosition => Position;

        public Vector2 StackedEndPosition => EndPosition + StackOffset;

        public virtual int StackHeight { get; set; }

        public Vector2 StackOffset => new Vector2(StackHeight * Scale * -6.4f);

        public double Radius => OBJECT_RADIUS * Scale;

        public float Scale { get; set; } = 1;

        public abstract HitObjectType Type { get; }

        public Color4 ComboColour { get; set; }
        public virtual bool NewCombo { get; set; }
        public int ComboIndex { get; set; }

        public double HitWindowFor(OsuScoreResult result)
        {
            switch (result)
            {
                default:
                    return 300;
                case OsuScoreResult.Hit50:
                    return 150;
                case OsuScoreResult.Hit100:
                    return 80;
                case OsuScoreResult.Hit300:
                    return 30;
            }
        }

        public OsuScoreResult ScoreResultForOffset(double offset)
        {
            if (offset < HitWindowFor(OsuScoreResult.Hit300))
                return OsuScoreResult.Hit300;
            if (offset < HitWindowFor(OsuScoreResult.Hit100))
                return OsuScoreResult.Hit100;
            if (offset < HitWindowFor(OsuScoreResult.Hit50))
                return OsuScoreResult.Hit50;
            return OsuScoreResult.Miss;
        }

        public virtual void SetDefaultsFromBeatmap(Beatmap<OsuHitObject> beatmap)
        {
            Scale = (1.0f - 0.7f * (beatmap.BeatmapInfo.BaseDifficulty.CircleSize - 5) / 5) / 2;
        }
    }
}
