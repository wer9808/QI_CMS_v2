using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QI_CMS_v2.Utils;

namespace QI_CMS_v2.Models
{
    public interface IFileMedia
    {
        string Path { get; set; }
    }

    public class MediaContent
    {
        public enum ScaleMethodType
        {
            None, KeepRatio, FitWidth, FitHeight, Stretch,
        }

        public enum VerticalAlignmentType
        {
            Top, Center, Bottom
        }

        public enum HorizontalAlignmentType
        {
            Left, Center, Right
        }

        public ScaleMethodType ScaleMethod = ScaleMethodType.KeepRatio;
        public VerticalAlignmentType VerticalAlignment = VerticalAlignmentType.Top;
        public HorizontalAlignmentType HorizontalAlignment = HorizontalAlignmentType.Left;

        private double _width = 0;
        public virtual double Width
        {
            get => _width;
            set
            {
                if (value > 0) _width = value;
            }
        }

        private double _height = 0;
        public virtual double Height
        {
            get => _height;
            set
            {
                if (value > 0) _height = value;
            }
        }

        public virtual bool CanPlay()
        {
            return false;
        }

    }

    public class TextContent: MediaContent
    {
        public string Text { get; set; }
        public int FontSize { get; set; } = 12;
        public string FontFamily { get; set; } = "Arial";
        public string FontWeight { get; set; } = "Normal";
        public string FontColor { get; set; } = "#FFFFFF";

        private FormattedText _formattedText
        {
            get
            {
                return new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(FontFamily), FontSize, Brushes.Black, 1.0);
            }
        }
        public override double Width { get => _formattedText.Width; }
        public override double Height { get => _formattedText.Height; }

        public TextContent(string text)
        {
            Text = text;
        }

        public override bool CanPlay()
        {
            return !string.IsNullOrEmpty(Text);
        }

    }

    public class ImageContent: MediaContent, IFileMedia
    {

        public class ImageInfo
        {
            public double Width { get; set; }
            public double Height { get; set; }
        }

        public string Path { get; set; }
        public ImageInfo? Info { get; set; }

        public override double Width { get => Info?.Width ?? 0; }
        public override double Height { get => Info?.Height ?? 0; }

        public ImageContent(string path)
        {
            Path = path;
            if (File.Exists(Path))
            {
                using (var stream = File.OpenRead(Path))
                {
                    var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    var frame = decoder.Frames[0]; // 첫 번째 프레임 가져오기
                    Info = new ImageInfo
                    {
                        Width = frame.PixelWidth,  // 원본 픽셀 크기 가져오기
                        Height = frame.PixelHeight
                    };
                }
            }
        }

        public override bool CanPlay()
        {
            return File.Exists(Path);
        }

    }

    public class VideoContent : MediaContent, IFileMedia
    {
        public class VideoInfo
        {
            public double Width { get; set; }
            public double Height { get; set; }
            public double Duration { get; set; }
            public int FrameCount { get; set; }
            public double FrameRate { get; set; }

            public bool IsValid()
            {
                if (Width <= 0 || Height <= 0) return false;
                if (Duration <= 0) return false;

                return true;
            }
        }

        public string Path { get; set; }
        public VideoInfo? Info { get; set; }
        public override double Width { get => Info?.Width ?? 0; }
        public override double Height { get => Info?.Height ?? 0; }

        public VideoContent(string path)
        {
            Path = path;
            if (File.Exists(path))
            {
                Info = GetVideoInfo(path);
            }
        }

        public override bool CanPlay()
        {
            return File.Exists(Path) && Info != null && Info.IsValid();
        }

        public static VideoInfo? GetVideoInfo(string videoPath)
        {
            string ffprobePath = AppConfig.FFPROBE_FILE_PATH; // ffprobe.exe 경로 설정
            string arguments = $"-v quiet -print_format json -show_streams -show_format \"{videoPath}\"";

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    return ParseVideoInfo(output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"오류 발생: {ex.Message}");
                return null;
            }
        }


        private static VideoInfo? ParseVideoInfo(string jsonOutput)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonOutput);
                var root = doc.RootElement;

                double width = 0, height = 0, duration = 0, frameRate = 0;
                int frameCount = 0;

                if (root.TryGetProperty("streams", out var streams))
                {
                    foreach (var stream in streams.EnumerateArray())
                    {
                        if (stream.TryGetProperty("codec_type", out var codecType) &&
                            codecType.GetString() == "video")
                        {
                            if (stream.TryGetProperty("width", out var w))
                            {
                                width = JsonUtils.GetSafeDouble(w);
                            }
                            if (stream.TryGetProperty("height", out var h))
                            {
                                height = JsonUtils.GetSafeDouble(h);
                            }
                            if (stream.TryGetProperty("nb_frames", out var frames))
                            {
                                frameCount = JsonUtils.GetSafeInt(frames);
                            }
                            if (stream.TryGetProperty("r_frame_rate", out var rFrameRate))
                            {
                                string? fpsStr = rFrameRate.GetString(); // 예: "30/1"
                                if (!string.IsNullOrEmpty(fpsStr) && fpsStr.Contains('/'))
                                {
                                    string[] parts = fpsStr.Split('/');
                                    if (double.TryParse(parts[0], out double num) &&
                                        double.TryParse(parts[1], out double denom) && denom != 0)
                                    {
                                        frameRate = num / denom;
                                    }
                                }
                            }
                        }
                    }
                }

                if (root.TryGetProperty("format", out var format))
                {
                    duration = JsonUtils.GetSafeDouble(format.GetProperty("duration"));
                }

                // duration이 없을 경우 계산
                if (duration == 0 && frameCount > 0 && frameRate > 0)
                {
                    duration = frameCount / frameRate;
                }

                return new VideoInfo()
                {
                    Width = width,
                    Height = height,
                    FrameCount = frameCount,
                    FrameRate = frameRate,
                    Duration = duration
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON 파싱 오류: {ex.Message}");
                return null;
            }
        }

    }

    public class PlayableItem
    {

        public enum MediaContentType
        {
            Image, Video, Text
        }

        public MediaContentType ContentType { get; set; }
        public MediaContent Content { get; set; }
        public double Duration { get; set; } = 5.0;
        public string BackgroundColor { get; set; } = "#FF000000";

        public PlayableItem(TextContent textContent)
        {
            ContentType = MediaContentType.Text;
            Content = textContent;
        }

        public PlayableItem(ImageContent imageContent)
        {
            ContentType = MediaContentType.Image;
            Content = imageContent;
        }

        public PlayableItem(VideoContent videoContent)
        {
            ContentType = MediaContentType.Video;
            Content = videoContent;
            if (videoContent.Info != null)
            {
                Duration = videoContent.Info.Duration;
            }
        }

        public PlayableItem(string textContent)
        {
            ContentType = MediaContentType.Text;
            Content = new MediaContent();
        }

        public double EvaluateX(Size layerSize)
        {
            switch (Content.HorizontalAlignment)
            {
                case MediaContent.HorizontalAlignmentType.Left:
                    return 0;
                case MediaContent.HorizontalAlignmentType.Center:
                    return (layerSize.Width - EvaluateWidth(layerSize)) / 2;
                case MediaContent.HorizontalAlignmentType.Right:
                    return layerSize.Width - EvaluateWidth(layerSize);
                default:
                    return 0;
            }
        }

        public double EvaluateY(Size layerSize)
        {
            switch (Content.VerticalAlignment)
            {
                case MediaContent.VerticalAlignmentType.Top:
                    return 0;
                case MediaContent.VerticalAlignmentType.Center:
                    return (layerSize.Height - EvaluateHeight(layerSize)) / 2;
                case MediaContent.VerticalAlignmentType.Bottom:
                    return layerSize.Height - EvaluateHeight(layerSize);
                default:
                    return 0;
            }
        }

        public double EvaluateWidth(Size layerSize)
        {
            switch (Content.ScaleMethod)
            {
                case MediaContent.ScaleMethodType.KeepRatio:
                    double ratio = Width / Height;
                    double layerRatio = layerSize.Width / layerSize.Height;
                    if (ratio > layerRatio)
                    {
                        return layerSize.Width;
                    }
                    else
                    {
                        return Width * layerSize.Height / Height;
                    }
                case MediaContent.ScaleMethodType.FitWidth:
                    return layerSize.Width;
                case MediaContent.ScaleMethodType.FitHeight:
                    return Width * layerSize.Height / Height;
                case MediaContent.ScaleMethodType.Stretch:
                    return layerSize.Width;
                default:
                    return Width;
            }
        }
        public double EvaluateHeight(Size layerSize)
        {
            switch (Content.ScaleMethod)
            {
                case MediaContent.ScaleMethodType.KeepRatio:
                    double ratio = Width / Height;
                    double layerRatio = layerSize.Width / layerSize.Height;
                    if (ratio > layerRatio)
                    {
                        return Height * layerSize.Width / Width;
                    }
                    else
                    {
                        return layerSize.Height;
                    }
                case MediaContent.ScaleMethodType.FitWidth:
                    return Height * layerSize.Width / Width; 
                case MediaContent.ScaleMethodType.FitHeight:
                    return layerSize.Height;
                case MediaContent.ScaleMethodType.Stretch:
                    return layerSize.Height;
                default:
                    return Height;
            }
        }

        public double Width
        {
            get
            {
                return Content.Width;
            }
        }

        public double Height
        {
            get
            {
                return Content.Height;
            }
        }

    }

    public class Playlist
    {
        public List<PlayableItem> Items { get; set; } = [];

        public double TotalDuration { get => Items.Sum(m => m.Duration); }
    }

    /*
     * PlayableItem을 일정 시간에 재생할 수 있도록 스케줄링하는 클래스
     * 
     * 재생 스케줄 사이에는 Period의 우선순위에 따라 재생할 수 있도록 함
     * Priority 순서 : Disposable > Repeat > Default
     * 
     * Disposable끼리 겹치는 경우
     * 더 시작 시간이 늦는 스케줄이 화면에 우선 출력됨
     * 
     * Repeat끼리 겹치는 경우
     * Year > Month > Week > Day 순으로 우선순위를 둠
     * Period가 같은 경우 시작 시간이 늦는 스케줄이 화면에 우선 출력됨
     * 
     * 새로운 스케줄을 재생할 경우, 재생 중이던 스케줄은 스택에 저장됨
     * 스케줄 재생이 끝나면, 스택에서 Pop하여 기존에 재생 중이던 스케줄이 있는지 확인
     * 재생 중이던 스케줄이 있다면, 해당 스케줄의 재생 종료 시간이 지났는지 확인하고 안 지났으면 재생
     * 없다면, 다음 Default 스케줄을 재생
     */
    public class Schedule
    {
        public enum ScheduleType
        {
            Default, Repeat, Disposable
        }

        public List<PlayableItem> Items { get; set; } = [];
        public double ItemsDuration { get => Items.Sum(m => m.Duration); }
        public double Duration { get; set; }

        public ScheduleType Type { get; set; } = ScheduleType.Default;
        public bool HasDuration { get; set; } = false;

        public enum RepeatPeriod
        {
            None, Day, Week, Month, Year
        }
        public DateTime StartDateTime { get; set; }
        public RepeatPeriod Period { get; set; } = RepeatPeriod.None;
        public TimeOnly StartTime { get; set; }
        public DayOfWeek PeriodDayOfWeek { get; set; }
        public int PeriodDay { get; set; }
        public int PeriodMonth { get; set; }
        public DateTime CreatedAt { get; set; }

        public Schedule(double? duration = null)
        {
            if (duration != null)
            {
                HasDuration = true;
                Duration = (double)duration;
            }
            else
            {
                HasDuration = false;
                Duration = ItemsDuration;
            }

            CreatedAt = DateTime.Now;
        }

        public Schedule(TimeOnly startTime, double? duration = null)
        {
            Type = ScheduleType.Repeat;
            Period = RepeatPeriod.Day;
            StartTime = startTime;
            if (duration != null)
            {
                HasDuration = true;
                Duration = (double)duration;
            }
            else
            {
                HasDuration = false;
                Duration = ItemsDuration;
            }

            CreatedAt = DateTime.Now;
        }

        public Schedule(DayOfWeek dayOfWeek, TimeOnly startTime, double? duration = null)
        {
            Type = ScheduleType.Repeat;
            Period = RepeatPeriod.Week;
            StartTime = startTime;
            PeriodDayOfWeek = dayOfWeek;
            if (duration != null)
            {
                HasDuration = true;
                Duration = (double)duration;
            }
            else
            {
                HasDuration = false;
                Duration = ItemsDuration;
            }
            CreatedAt = DateTime.Now;
        }

        public Schedule(int periodDay, TimeOnly startTime, double? duration = null)
        {
            Type = ScheduleType.Repeat;
            Period = RepeatPeriod.Day;
            StartTime = startTime;
            PeriodDay = periodDay;
            if (duration != null)
            {
                HasDuration = true;
                Duration = (double)duration;
            }
            else
            {
                HasDuration = false;
                Duration = ItemsDuration;
            }
            CreatedAt = DateTime.Now;
        }

        public Schedule(int periodDay, int periodMonth, TimeOnly startTime, double? duration = null)
        {
            Type = ScheduleType.Repeat;
            Period = RepeatPeriod.Day;
            StartTime = startTime;
            PeriodDay = periodDay;
            PeriodMonth = periodMonth;
            if (duration != null)
            {
                HasDuration = true;
                Duration = (double)duration;
            }
            else
            {
                HasDuration = false;
                Duration = ItemsDuration;
            }
            CreatedAt = DateTime.Now;
        }

        public Schedule(DateTime startTime, double? duration = null)
        {
            Type = ScheduleType.Disposable;
            Period = RepeatPeriod.None;
            StartDateTime = startTime;
            if (duration != null)
            {
                HasDuration = true;
                Duration = (double)duration;
            }
            else
            {
                HasDuration = false;
                Duration = ItemsDuration;
            }
            CreatedAt = DateTime.Now;
        }

        public static int GetSchedulePriority(Schedule schedule)
        {
            const int priorityMax = 10;
            int nice = priorityMax;
            if (schedule.Type == ScheduleType.Disposable) nice = 0;
            else if (schedule.Type == ScheduleType.Repeat)
            {
                nice = schedule.Period switch
                {
                    RepeatPeriod.Year => 1,
                    RepeatPeriod.Month => 2,
                    RepeatPeriod.Week => 3,
                    RepeatPeriod.Day => 4,
                    _ => priorityMax,
                };
            }

            return priorityMax - nice;
        }

        public int GetSchedulePriority()
        {
            const int priorityMax = 10;
            int nice = priorityMax;
            if (Type == ScheduleType.Disposable) nice = 0;
            else if (Type == ScheduleType.Repeat)
            {
                nice = Period switch
                {
                    RepeatPeriod.Year => 1,
                    RepeatPeriod.Month => 2,
                    RepeatPeriod.Week => 3,
                    RepeatPeriod.Day => 4,
                    _ => priorityMax,
                };
            }

            return priorityMax - nice;
        }

        public bool IsTimeToPlay(DateTime curDateTime)
        {
            int curMonth = curDateTime.Month;
            int curDay = curDateTime.Day;
            var curDayOfWeek = curDateTime.DayOfWeek;
            var curTime = TimeOnly.FromDateTime(curDateTime);

            if (Type == ScheduleType.Repeat)
            {
                if (Period == RepeatPeriod.Year)
                {
                    var startDateTime = new DateTime(curDateTime.Year, PeriodMonth, PeriodDay, StartTime.Hour, StartTime.Minute, StartTime.Second);
                    var endDateTime = startDateTime + TimeSpan.FromSeconds(Duration);
                    return startDateTime <= curDateTime && curDateTime <= endDateTime;
                }
                else if (Period == RepeatPeriod.Month)
                {
                    var startDateTime = new DateTime(curDateTime.Year, curMonth, PeriodDay, StartTime.Hour, StartTime.Minute, StartTime.Second);
                    var endDateTime = startDateTime + TimeSpan.FromSeconds(Duration);
                    return startDateTime <= curDateTime && curDateTime <= endDateTime;
                }
                else if (Period == RepeatPeriod.Week)
                {
                    var startDateTime = new DateTime(curDateTime.Year, curMonth, curDay, StartTime.Hour, StartTime.Minute, StartTime.Second);
                    var endDateTime = startDateTime + TimeSpan.FromSeconds(Duration);
                    return curDayOfWeek == PeriodDayOfWeek && startDateTime <= curDateTime && curDateTime <= endDateTime;
                }
                else
                {
                    var startDateTime = new DateTime(curDateTime.Year, curMonth, curDay, StartTime.Hour, StartTime.Minute, StartTime.Second);
                    var endDateTime = startDateTime + TimeSpan.FromSeconds(Duration);
                    return startDateTime <= curDateTime && curDateTime <= endDateTime;
                }
            }
            else if (Type == ScheduleType.Disposable)
            {
                var endDateTime = StartDateTime + TimeSpan.FromSeconds(Duration);
                return StartDateTime <= curDateTime && curDateTime <= endDateTime;
            }
            else
            {
                return false;
            }
        }

        public void AddPlayable(PlayableItem playable)
        {
            Items.Add(playable);
            if (!HasDuration)
            {
                Duration += playable.Duration;
            }
        }

        public void RemovePlayable(PlayableItem playable) 
        {
            Items.Remove(playable);
            if (HasDuration)
            {
                Duration -= playable.Duration;
            }
        }

    }

}
