using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace YTPDeluxe
{
    public sealed class MediaLibrary
    {
        public List<string> Materials { get; set; }
        public List<string> Transitions { get; set; }
        public List<string> Intros { get; set; }
        public List<string> Outros { get; set; }
        public List<string> Overlays { get; set; }
        public List<string> SoundFx { get; set; }
        public List<string> Music { get; set; }
        public List<string> Rave { get; set; }
        public List<string> Vocoder { get; set; }

        public MediaLibrary()
        {
            Materials = new List<string>();
            Transitions = new List<string>();
            Intros = new List<string>();
            Outros = new List<string>();
            Overlays = new List<string>();
            SoundFx = new List<string>();
            Music = new List<string>();
            Rave = new List<string>();
            Vocoder = new List<string>();
        }
    }

    public sealed class VideoClip
    {
        public string SourcePath { get; set; }
        public double StartSeconds { get; set; }
        public double DurationSeconds { get; set; }
        public List<string> VideoFilters { get; private set; }
        public List<string> AudioFilters { get; private set; }
        public List<string> PostFilters { get; private set; }
        public List<string> Notes { get; private set; }

        public VideoClip()
        {
            VideoFilters = new List<string>();
            AudioFilters = new List<string>();
            PostFilters = new List<string>();
            Notes = new List<string>();
        }
    }

    public sealed class AudioClip
    {
        public string SourcePath { get; set; }
        public List<string> Notes { get; private set; }

        public AudioClip()
        {
            Notes = new List<string>();
        }
    }

    public sealed class TimelineSegment
    {
        public VideoClip Video { get; set; }
        public AudioClip Audio { get; set; }
        public List<string> AppliedEffects { get; private set; }
        public string RenderedPath { get; set; }

        public TimelineSegment()
        {
            AppliedEffects = new List<string>();
        }
    }

    public sealed class RenderSettings
    {
        public string FFmpegPath { get; set; }
        public string OutputPath { get; set; }
        public int ClipCount { get; set; }
        public int MinClipSeconds { get; set; }
        public int MaxClipSeconds { get; set; }
        public int StackLevel { get; set; }
        public bool ChaosTimeline { get; set; }
        public bool AutoSourceSwitcher { get; set; }
        public bool CleanupTempFiles { get; set; }

        public RenderSettings()
        {
            FFmpegPath = Utilities.GetConfiguredFFmpegPath();
            OutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "YTPDeluxe_Output.mp4");
            ClipCount = 12;
            MinClipSeconds = 1;
            MaxClipSeconds = 5;
            StackLevel = 3;
            ChaosTimeline = true;
            AutoSourceSwitcher = true;
            CleanupTempFiles = true;
        }
    }

    public sealed class GeneratorConfig
    {
        public MediaLibrary Library { get; set; }
        public RenderSettings Settings { get; set; }
        public List<EffectSetting> Effects { get; set; }

        public GeneratorConfig()
        {
            Library = new MediaLibrary();
            Settings = new RenderSettings();
            Effects = new List<EffectSetting>();
        }

        public static GeneratorConfig CreateDeluxeCompletePreset()
        {
            GeneratorConfig config = new GeneratorConfig();
            IList<IEffect> effects = EffectsFactory.GetAllEffects();
            for (int i = 0; i < effects.Count; i++)
            {
                config.Effects.Add(new EffectSetting
                {
                    Name = effects[i].Name,
                    Enabled = true,
                    Probability = effects[i].Category == EffectCategory.PostRender ? 25 : 70,
                    Intensity = effects[i].Category == EffectCategory.Audio ? 65 : 75
                });
            }

            config.Settings.StackLevel = 4;
            config.Settings.ChaosTimeline = true;
            config.Settings.AutoSourceSwitcher = true;
            return config;
        }
    }

    public sealed class YTPGenerator
    {
        private readonly Random random;

        public YTPGenerator()
        {
            random = new Random();
        }

        public event EventHandler<GeneratorLogEventArgs> Log;
        public event EventHandler<GeneratorProgressEventArgs> ProgressChanged;

        public void Render(GeneratorConfig config, BackgroundWorker worker)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            if (config.Library == null || config.Library.Materials.Count == 0)
                throw new InvalidOperationException("At least one video material is required.");
            if (config.Settings == null)
                config.Settings = new RenderSettings();

            string tempDirectory = Utilities.CreateTempDirectory();
            try
            {
                LogMessage("Temporary workspace: " + tempDirectory);
                List<TimelineSegment> timeline = BuildTimeline(config, tempDirectory);
                RenderSegments(config, timeline, tempDirectory, worker);
                ConcatenateSegments(config, timeline, tempDirectory);
                ApplyPostRenderEffects(config, tempDirectory);
                LogMessage("Render complete: " + config.Settings.OutputPath);
                ReportProgress(100, "Done");
            }
            finally
            {
                if (config.Settings.CleanupTempFiles)
                {
                    try { Directory.Delete(tempDirectory, true); }
                    catch { LogMessage("Could not delete temporary folder: " + tempDirectory); }
                }
            }
        }

        public List<TimelineSegment> BuildTimeline(GeneratorConfig config, string tempDirectory)
        {
            List<TimelineSegment> timeline = new List<TimelineSegment>();
            int count = Math.Max(1, config.Settings.ClipCount);
            string lastSource = null;

            for (int i = 0; i < count; i++)
            {
                string source = PickSource(config.Library.Materials, lastSource, config.Settings.AutoSourceSwitcher);
                lastSource = source;

                VideoClip video = new VideoClip();
                video.SourcePath = source;
                video.StartSeconds = random.Next(0, 30);
                video.DurationSeconds = random.Next(Math.Max(1, config.Settings.MinClipSeconds), Math.Max(config.Settings.MinClipSeconds + 1, config.Settings.MaxClipSeconds + 1));

                AudioClip audio = new AudioClip();
                audio.SourcePath = source;

                TimelineSegment segment = new TimelineSegment();
                segment.Video = video;
                segment.Audio = audio;
                ApplyRandomEffects(config, segment, tempDirectory);
                timeline.Add(segment);
            }

            if (config.Settings.ChaosTimeline || HasEnabled(config, "Chaos Timeline") || HasEnabled(config, "Random Clip Shuffle"))
                Utilities.Shuffle(timeline, random);

            InsertLibraryClips(config.Library.Intros, timeline, true);
            InsertLibraryClips(config.Library.Outros, timeline, false);
            return timeline;
        }

        private void RenderSegments(GeneratorConfig config, List<TimelineSegment> timeline, string tempDirectory, BackgroundWorker worker)
        {
            for (int i = 0; i < timeline.Count; i++)
            {
                if (worker != null && worker.CancellationPending)
                    throw new OperationCanceledException("Render cancelled.");

                TimelineSegment segment = timeline[i];
                segment.RenderedPath = Path.Combine(tempDirectory, "segment_" + i.ToString("000") + ".mp4");
                string args = BuildSegmentArguments(segment, segment.RenderedPath);

                LogMessage("Rendering segment " + (i + 1) + "/" + timeline.Count + ": " + Path.GetFileName(segment.Video.SourcePath));
                int exitCode = Utilities.RunProcess(config.Settings.FFmpegPath, args, tempDirectory, OnFFmpegOutput);
                if (exitCode != 0)
                    throw new InvalidOperationException("FFmpeg failed while rendering segment " + (i + 1) + ". Exit code: " + exitCode);

                ReportProgress((int)((i + 1) * 80.0 / timeline.Count), "Rendered segment " + (i + 1));
            }
        }

        private void ConcatenateSegments(GeneratorConfig config, List<TimelineSegment> timeline, string tempDirectory)
        {
            List<string> paths = new List<string>();
            for (int i = 0; i < timeline.Count; i++)
                paths.Add(timeline[i].RenderedPath);

            string concatFile = Path.Combine(tempDirectory, "concat.txt");
            string joinedOutput = Path.Combine(tempDirectory, "joined.mp4");
            Utilities.WriteConcatFile(concatFile, paths);

            LogMessage("Concatenating final timeline...");
            string args = "-y -f concat -safe 0 -i " + Utilities.Quote(concatFile) + " -c copy " + Utilities.Quote(joinedOutput);
            int exitCode = Utilities.RunProcess(config.Settings.FFmpegPath, args, tempDirectory, OnFFmpegOutput);
            if (exitCode != 0)
                throw new InvalidOperationException("FFmpeg failed while concatenating segments. Exit code: " + exitCode);

            File.Copy(joinedOutput, config.Settings.OutputPath, true);
            ReportProgress(90, "Timeline joined");
        }

        private void ApplyPostRenderEffects(GeneratorConfig config, string tempDirectory)
        {
            List<string> postFilters = new List<string>();
            IList<IEffect> effects = EffectsFactory.GetAllEffects();
            for (int i = 0; i < config.Effects.Count; i++)
            {
                EffectSetting setting = config.Effects[i];
                IEffect effect = EffectsFactory.GetEffect(setting.Name);
                if (effect != null && effect.Category == EffectCategory.PostRender && setting.Enabled && random.Next(100) < setting.Probability)
                {
                    VideoClip video = new VideoClip();
                    AudioClip audio = new AudioClip();
                    effect.Apply(video, audio, new EffectParameters { Intensity = setting.Intensity, Probability = setting.Probability, Random = random, Library = config.Library, TempDirectory = tempDirectory });
                    postFilters.AddRange(video.PostFilters);
                    LogMessage("Queued post effect: " + effect.Name);
                }
            }

            if (postFilters.Count == 0)
                return;

            string source = config.Settings.OutputPath;
            string postOutput = Path.Combine(tempDirectory, "post.mp4");
            string args = "-y -i " + Utilities.Quote(source) + " -vf " + Utilities.Quote(String.Join(",", postFilters.ToArray())) + " -c:v libx264 -pix_fmt yuv420p -c:a copy " + Utilities.Quote(postOutput);
            int exitCode = Utilities.RunProcess(config.Settings.FFmpegPath, args, tempDirectory, OnFFmpegOutput);
            if (exitCode != 0)
                throw new InvalidOperationException("FFmpeg failed while applying post-render effects. Exit code: " + exitCode);

            File.Copy(postOutput, config.Settings.OutputPath, true);
            ReportProgress(96, "Post-render effects applied");
        }

        private string BuildSegmentArguments(TimelineSegment segment, string outputPath)
        {
            StringBuilder args = new StringBuilder();
            args.Append("-y -ss ");
            args.Append(Utilities.Invariant(segment.Video.StartSeconds));
            args.Append(" -t ");
            args.Append(Utilities.Invariant(segment.Video.DurationSeconds));
            args.Append(" -i ");
            args.Append(Utilities.Quote(segment.Video.SourcePath));

            string videoFilters = segment.Video.VideoFilters.Count == 0 ? "scale=640:480:force_original_aspect_ratio=decrease,pad=640:480:(ow-iw)/2:(oh-ih)/2,setsar=1" : String.Join(",", segment.Video.VideoFilters.ToArray()) + ",scale=640:480:force_original_aspect_ratio=decrease,pad=640:480:(ow-iw)/2:(oh-ih)/2,setsar=1";
            string audioFilters = segment.Video.AudioFilters.Count == 0 ? "anull" : String.Join(",", segment.Video.AudioFilters.ToArray());

            args.Append(" -vf ");
            args.Append(Utilities.Quote(videoFilters));
            args.Append(" -af ");
            args.Append(Utilities.Quote(audioFilters));
            args.Append(" -c:v libx264 -preset veryfast -pix_fmt yuv420p -c:a aac -ar 44100 -ac 2 -shortest ");
            args.Append(Utilities.Quote(outputPath));
            return args.ToString();
        }

        private void ApplyRandomEffects(GeneratorConfig config, TimelineSegment segment, string tempDirectory)
        {
            List<EffectSetting> candidates = new List<EffectSetting>();
            for (int i = 0; i < config.Effects.Count; i++)
            {
                if (config.Effects[i].Enabled && EffectsFactory.GetEffect(config.Effects[i].Name) != null)
                    candidates.Add(config.Effects[i]);
            }

            Utilities.Shuffle(candidates, random);
            int applied = 0;
            int desired = Math.Max(1, config.Settings.StackLevel);
            for (int i = 0; i < candidates.Count && applied < desired; i++)
            {
                EffectSetting setting = candidates[i];
                IEffect effect = EffectsFactory.GetEffect(setting.Name);
                if (effect.Category == EffectCategory.PostRender)
                    continue;
                if (random.Next(100) >= setting.Probability)
                    continue;

                effect.Apply(segment.Video, segment.Audio, new EffectParameters { Probability = setting.Probability, Intensity = setting.Intensity, Random = random, Library = config.Library, TempDirectory = tempDirectory });
                segment.AppliedEffects.Add(effect.Name);
                applied++;
            }

            if (segment.AppliedEffects.Count > 0)
                LogMessage("Applied stack: " + String.Join(", ", segment.AppliedEffects.ToArray()));
        }

        private string PickSource(IList<string> materials, string lastSource, bool switchSources)
        {
            if (!switchSources || materials.Count < 2)
                return Utilities.PickRandom(materials, random);

            string picked = Utilities.PickRandom(materials, random);
            int guard = 0;
            while (picked == lastSource && guard < 10)
            {
                picked = Utilities.PickRandom(materials, random);
                guard++;
            }
            return picked;
        }

        private void InsertLibraryClips(IList<string> clips, List<TimelineSegment> timeline, bool atBeginning)
        {
            if (clips == null || clips.Count == 0)
                return;

            string source = Utilities.PickRandom(clips, random);
            TimelineSegment segment = new TimelineSegment();
            segment.Video = new VideoClip { SourcePath = source, StartSeconds = 0, DurationSeconds = 4 };
            segment.Audio = new AudioClip { SourcePath = source };
            segment.AppliedEffects.Add(atBeginning ? "Intro" : "Outro");
            if (atBeginning)
                timeline.Insert(0, segment);
            else
                timeline.Add(segment);
        }

        private bool HasEnabled(GeneratorConfig config, string name)
        {
            for (int i = 0; i < config.Effects.Count; i++)
            {
                if (config.Effects[i].Enabled && String.Equals(config.Effects[i].Name, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private void OnFFmpegOutput(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
                LogMessage(e.Data);
        }

        private void LogMessage(string message)
        {
            EventHandler<GeneratorLogEventArgs> handler = Log;
            if (handler != null)
                handler(this, new GeneratorLogEventArgs(message));
        }

        private void ReportProgress(int percent, string status)
        {
            EventHandler<GeneratorProgressEventArgs> handler = ProgressChanged;
            if (handler != null)
                handler(this, new GeneratorProgressEventArgs(percent, status));
        }
    }

    public sealed class GeneratorLogEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public GeneratorLogEventArgs(string message) { Message = message; }
    }

    public sealed class GeneratorProgressEventArgs : EventArgs
    {
        public int Progress { get; private set; }
        public string Status { get; private set; }
        public GeneratorProgressEventArgs(int progress, string status) { Progress = progress; Status = status; }
    }
}
