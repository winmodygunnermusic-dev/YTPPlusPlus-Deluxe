using System;
using System.Collections.Generic;

namespace YTPDeluxe
{
    public interface IEffect
    {
        string Name { get; }
        EffectCategory Category { get; }
        void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters);
    }

    public enum EffectCategory
    {
        Video,
        Audio,
        Hybrid,
        Special,
        PostRender
    }

    public sealed class EffectParameters
    {
        public int Probability { get; set; }
        public int Intensity { get; set; }
        public Random Random { get; set; }
        public MediaLibrary Library { get; set; }
        public string TempDirectory { get; set; }

        public EffectParameters()
        {
            Probability = 50;
            Intensity = 50;
            Random = new Random();
        }
    }

    public sealed class EffectSetting
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public int Probability { get; set; }
        public int Intensity { get; set; }

        public EffectSetting()
        {
            Enabled = true;
            Probability = 50;
            Intensity = 50;
        }
    }

    public static class EffectsFactory
    {
        private static readonly List<IEffect> Effects = new List<IEffect>();

        static EffectsFactory()
        {
            Register(new ReverseClipEffect());
            Register(new SpeedEffect());
            Register(new FrameShuffleEffect());
            Register(new MirrorModeEffect());
            Register(new InvertColorsEffect());
            Register(new RainbowOverlayEffect());
            Register(new DeepFryVisionEffect());
            Register(new ExplosionSpamEffect());
            Register(new ScreenFlipEffect());
            Register(new InfiniteMirrorEffect());
            Register(new TrailingReverseEffect());
            Register(new RandomSoundInjectionEffect());
            Register(new ChorusEchoEffect());
            Register(new AperiodicEchoEffect());
            Register(new VibratoPitchBendEffect());
            Register(new PitchShiftEffect());
            Register(new StutterLoopEffect());
            Register(new EarrapeModeEffect());
            Register(new AutoTuneChaosEffect());
            Register(new AudioCrustEffect());
            Register(new DanceModeEffect());
            Register(new SquidwardModeEffect());
            Register(new SusEffect());
            Register(new MemeInjectionEffect());
            Register(new SentenceMixingEffect());
            Register(new GreenScreenPortalEffect());
            Register(new ChaosTimelineEffect());
            Register(new ClassicNostalgiaModeEffect());
            Register(new RaveModeEffect());
            Register(new RandomClipShuffleEffect());
            Register(new SubliminalAdvertisingEffect());
            Register(new QuarterShuffleEffect());
            Register(new GradientOverlayEffect());
        }

        public static IList<IEffect> GetAllEffects()
        {
            return new List<IEffect>(Effects);
        }

        public static IEffect GetEffect(string name)
        {
            for (int i = 0; i < Effects.Count; i++)
            {
                if (String.Equals(Effects[i].Name, name, StringComparison.OrdinalIgnoreCase))
                    return Effects[i];
            }

            return null;
        }

        private static void Register(IEffect effect)
        {
            Effects.Add(effect);
        }
    }

    internal abstract class EffectBase : IEffect
    {
        protected EffectBase(string name, EffectCategory category)
        {
            Name = name;
            Category = category;
        }

        public string Name { get; private set; }
        public EffectCategory Category { get; private set; }

        public abstract void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters);

        protected static string Scale(EffectParameters parameters, double low, double high)
        {
            double t = Math.Max(0, Math.Min(100, parameters.Intensity)) / 100.0;
            return Utilities.Invariant(low + ((high - low) * t));
        }
    }

    internal sealed class ReverseClipEffect : EffectBase
    {
        public ReverseClipEffect() : base("Reverse Clip", EffectCategory.Video) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("reverse"); videoClip.AudioFilters.Add("areverse"); }
    }

    internal sealed class SpeedEffect : EffectBase
    {
        public SpeedEffect() : base("Speed Up / Slow Down", EffectCategory.Video) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters)
        {
            double speed = parameters.Random.Next(0, 2) == 0 ? 0.5 : 1.25 + (parameters.Intensity / 50.0);
            videoClip.VideoFilters.Add("setpts=" + Utilities.Invariant(1.0 / speed) + "*PTS");
            videoClip.AudioFilters.Add("atempo=" + Utilities.ClampAtempo(speed));
        }
    }

    internal sealed class FrameShuffleEffect : EffectBase
    {
        public FrameShuffleEffect() : base("Frame Shuffle", EffectCategory.Video) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("framestep=2,setpts=N/FRAME_RATE/TB"); }
    }

    internal sealed class MirrorModeEffect : EffectBase
    {
        public MirrorModeEffect() : base("Mirror Mode", EffectCategory.Video) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("hflip"); }
    }

    internal sealed class InvertColorsEffect : EffectBase
    {
        public InvertColorsEffect() : base("Invert Colors", EffectCategory.Video) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("negate"); }
    }

    internal sealed class RainbowOverlayEffect : EffectBase
    {
        public RainbowOverlayEffect() : base("Rainbow Overlay", EffectCategory.Video) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("hue=h=2*PI*t:s=1.5"); }
    }

    internal sealed class DeepFryVisionEffect : EffectBase
    {
        public DeepFryVisionEffect() : base("Deep Fry Vision", EffectCategory.Video) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("eq=contrast=2.0:saturation=2.5:brightness=0.05,unsharp=5:5:1.5"); }
    }

    internal sealed class ExplosionSpamEffect : EffectBase
    {
        public ExplosionSpamEffect() : base("Explosion Spam", EffectCategory.Video) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters)
        {
            videoClip.VideoFilters.Add("boxblur=2:1");
            videoClip.AudioFilters.Add("volume=1.6");
        }
    }

    internal sealed class ScreenFlipEffect : EffectBase
    {
        public ScreenFlipEffect() : base("Screen Flip", EffectCategory.Video) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("vflip"); }
    }

    internal sealed class InfiniteMirrorEffect : EffectBase
    {
        public InfiniteMirrorEffect() : base("Infinite Mirror", EffectCategory.Video) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("scale=iw*0.96:ih*0.96,pad=iw/0.96:ih/0.96:(ow-iw)/2:(oh-ih)/2:black"); }
    }

    internal sealed class TrailingReverseEffect : EffectBase
    {
        public TrailingReverseEffect() : base("Trailing Reverse", EffectCategory.Video) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("tmix=frames=5:weights='5 4 3 2 1'"); videoClip.AudioFilters.Add("areverse"); }
    }

    internal sealed class RandomSoundInjectionEffect : EffectBase
    {
        public RandomSoundInjectionEffect() : base("Random Sound Injection", EffectCategory.Audio) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { audioClip.Notes.Add("Inject a random sound effect from the Sound FX library during assembly."); }
    }

    internal sealed class ChorusEchoEffect : EffectBase
    {
        public ChorusEchoEffect() : base("Chorus / Echo", EffectCategory.Audio) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.AudioFilters.Add("aecho=0.8:0.88:60:0.4"); }
    }

    internal sealed class AperiodicEchoEffect : EffectBase
    {
        public AperiodicEchoEffect() : base("Aperiodic Echo", EffectCategory.Audio) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.AudioFilters.Add("aecho=0.8:0.75:120|351:0.4|0.25"); }
    }

    internal sealed class VibratoPitchBendEffect : EffectBase
    {
        public VibratoPitchBendEffect() : base("Vibrato / Pitch Bend", EffectCategory.Audio) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.AudioFilters.Add("vibrato=f=6:d=0.7"); }
    }

    internal sealed class PitchShiftEffect : EffectBase
    {
        public PitchShiftEffect() : base("Pitch Shift", EffectCategory.Audio) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters)
        {
            double factor = 0.75 + (parameters.Random.NextDouble() * 0.75);
            videoClip.AudioFilters.Add("asetrate=44100*" + Utilities.Invariant(factor) + ",aresample=44100");
        }
    }

    internal sealed class StutterLoopEffect : EffectBase
    {
        public StutterLoopEffect() : base("Stutter Loop", EffectCategory.Audio) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("loop=loop=2:size=12:start=0,setpts=N/FRAME_RATE/TB"); videoClip.AudioFilters.Add("aloop=loop=2:size=22050:start=0"); }
    }

    internal sealed class EarrapeModeEffect : EffectBase
    {
        public EarrapeModeEffect() : base("Earrape Mode", EffectCategory.Audio) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.AudioFilters.Add("volume=" + Scale(parameters, 1.5, 5.0) + ",alimiter=limit=0.95"); }
    }

    internal sealed class AutoTuneChaosEffect : EffectBase
    {
        public AutoTuneChaosEffect() : base("Auto-Tune Chaos", EffectCategory.Audio) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.AudioFilters.Add("rubberband=pitch=" + Scale(parameters, 0.8, 1.6)); }
    }

    internal sealed class AudioCrustEffect : EffectBase
    {
        public AudioCrustEffect() : base("Audio Crust", EffectCategory.Audio) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.AudioFilters.Add("acrusher=level_in=2:level_out=1:bits=6:mode=log"); }
    }

    internal sealed class DanceModeEffect : EffectBase
    {
        public DanceModeEffect() : base("Dance Mode", EffectCategory.Hybrid) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("reverse,hue=h=2*PI*t"); videoClip.AudioFilters.Add("volume=1.25"); audioClip.Notes.Add("Prefer a Music/Rave backing track for this segment."); }
    }

    internal sealed class SquidwardModeEffect : EffectBase
    {
        public SquidwardModeEffect() : base("Squidward Mode", EffectCategory.Hybrid) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("framestep=2,setpts=0.4*PTS"); videoClip.AudioFilters.Add("asetrate=44100*1.35,aresample=44100"); }
    }

    internal sealed class SusEffect : EffectBase
    {
        public SusEffect() : base("Sus Effect", EffectCategory.Hybrid) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("crop=iw-20:ih-20:10+10*sin(t*9):10+10*cos(t*7),scale=iw:ih"); videoClip.AudioFilters.Add("aecho=0.9:0.6:80:0.5"); }
    }

    internal sealed class MemeInjectionEffect : EffectBase
    {
        public MemeInjectionEffect() : base("Meme Injection", EffectCategory.Hybrid) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.Notes.Add("Eligible for overlay/intros/sound-fx meme injection during timeline assembly."); }
    }

    internal sealed class SentenceMixingEffect : EffectBase
    {
        public SentenceMixingEffect() : base("Sentence Mixing", EffectCategory.Hybrid) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.AudioFilters.Add("atrim=0:0.25,aloop=loop=3:size=11025:start=0"); }
    }

    internal sealed class GreenScreenPortalEffect : EffectBase
    {
        public GreenScreenPortalEffect() : base("Green Screen Portal", EffectCategory.Special) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("chromakey=0x00ff00:0.25:0.10"); }
    }

    internal sealed class ChaosTimelineEffect : EffectBase
    {
        public ChaosTimelineEffect() : base("Chaos Timeline", EffectCategory.Special) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.Notes.Add("Timeline builder may shorten, duplicate, or aggressively reorder this segment."); }
    }

    internal sealed class ClassicNostalgiaModeEffect : EffectBase
    {
        public ClassicNostalgiaModeEffect() : base("Classic Nostalgia Mode", EffectCategory.Special) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("format=gray,eq=contrast=1.25"); audioClip.Notes.Add("Use XP-era system sounds from the Sound FX library when available."); }
    }

    internal sealed class RaveModeEffect : EffectBase
    {
        public RaveModeEffect() : base("Rave Mode", EffectCategory.Special) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.VideoFilters.Add("hue=h=90*sin(t*12):s=2,tmix=frames=3"); audioClip.Notes.Add("Prefer a Rave library track."); }
    }

    internal sealed class RandomClipShuffleEffect : EffectBase
    {
        public RandomClipShuffleEffect() : base("Random Clip Shuffle", EffectCategory.Special) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.Notes.Add("Timeline builder may randomize clip order after effect selection."); }
    }

    internal sealed class SubliminalAdvertisingEffect : EffectBase
    {
        public SubliminalAdvertisingEffect() : base("Subliminal Advertising", EffectCategory.PostRender) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.PostFilters.Add("drawtext=text='BUY MORE':x=(w-text_w)/2:y=(h-text_h)/2:enable='lt(mod(t,3),0.05)':fontcolor=white"); }
    }

    internal sealed class QuarterShuffleEffect : EffectBase
    {
        public QuarterShuffleEffect() : base("Quarter Shuffle", EffectCategory.PostRender) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.PostFilters.Add("crop=iw/2:ih/2:mod(n,2)*iw/2:mod(n+1,2)*ih/2,scale=iw*2:ih*2"); }
    }

    internal sealed class GradientOverlayEffect : EffectBase
    {
        public GradientOverlayEffect() : base("Gradient Overlay", EffectCategory.PostRender) { }
        public override void Apply(VideoClip videoClip, AudioClip audioClip, EffectParameters parameters) { videoClip.PostFilters.Add("eq=saturation=1.35:brightness=0.03"); }
    }
}
