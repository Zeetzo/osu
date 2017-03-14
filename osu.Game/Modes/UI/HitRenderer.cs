﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Modes.Mods;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Screens.Play;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace osu.Game.Modes.UI
{
    public abstract class HitRenderer : Container
    {
        public event Action<JudgementInfo> OnJudgement;
        public event Action OnAllJudged;

        internal readonly PlayerInputManager InputManager = new PlayerInputManager();

        /// <summary>
        /// Whether all the HitObjects have been judged.
        /// </summary>
        protected abstract bool AllObjectsJudged { get; }

        protected void TriggerOnJudgement(JudgementInfo j)
        {
            OnJudgement?.Invoke(j);

            if (AllObjectsJudged)
                OnAllJudged?.Invoke();
        }
    }

    public abstract class HitRenderer<TObject> : HitRenderer
        where TObject : HitObject
    {
        public Beatmap<TObject> Beatmap;

        public IEnumerable<DrawableHitObject> DrawableObjects => Playfield.HitObjects.Children;

        protected override Container<Drawable> Content => content;
        protected override bool AllObjectsJudged => Playfield.HitObjects.Children.All(h => h.Judgement.Result.HasValue);

        protected Playfield<TObject> Playfield;

        private Container content;

        protected HitRenderer(WorkingBeatmap beatmap)
        {
            Debug.Assert(beatmap != null, "HitRenderer initialized with a null beatmap.");

            // Convert + process the beatmap
            Beatmap = CreateBeatmapConverter().Convert(beatmap.Beatmap);
            Beatmap.HitObjects.ForEach(CreateBeatmapProcessor().SetDefaults);
            CreateBeatmapProcessor().PostProcess(Beatmap);

            applyMods(beatmap.Mods.Value);

            RelativeSizeAxes = Axes.Both;

            InputManager.Add(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    Playfield = CreatePlayfield(),
                }
            });

            AddInternal(InputManager);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            loadObjects();

            if (InputManager?.ReplayInputHandler != null)
                InputManager.ReplayInputHandler.ToScreenSpace = Playfield.ScaledContent.ToScreenSpace;
        }

        private void loadObjects()
        {
            foreach (TObject h in Beatmap.HitObjects)
            {
                DrawableHitObject<TObject> drawableObject = GetVisualRepresentation(h);

                if (drawableObject == null)
                    continue;

                drawableObject.OnJudgement += onJudgement;

                Playfield.Add(drawableObject);
            }

            Playfield.PostProcess();
        }

        private void applyMods(IEnumerable<Mod> mods)
        {
            if (mods == null)
                return;

            foreach (var mod in mods.OfType<IApplicableMod<TObject>>())
                mod.Apply(this);
        }

        private void onJudgement(DrawableHitObject<TObject> o, JudgementInfo j) => TriggerOnJudgement(j);

        protected abstract DrawableHitObject<TObject> GetVisualRepresentation(TObject h);
        protected abstract Playfield<TObject> CreatePlayfield();

        protected abstract IBeatmapConverter<TObject> CreateBeatmapConverter();
        protected abstract IBeatmapProcessor<TObject> CreateBeatmapProcessor();
    }
}
