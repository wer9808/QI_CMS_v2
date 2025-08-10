using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QI_CMS_v2.Models;
using QI_CMS_v2.Utils;
using QI_CMS_v2.Views;
using static QI_CMS_v2.Models.Schedule;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace QI_CMS_v2.ViewModels
{
    public class PlayableTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is PlayableItemViewModel playable)
            {
                if (playable.Playable.ContentType == PlayableItem.MediaContentType.Image)
                {
                    return ImageTemplate;
                }
                else if (playable.Playable.ContentType == PlayableItem.MediaContentType.Video)
                {
                    return VideoTemplate;
                }
                else if (playable.Playable.ContentType == PlayableItem.MediaContentType.Text)
                {
                    return TextTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }

        public class PlayableItemViewModel: ObservableObject
    {
        public PlayableItem Playable;

        public string Type
        {
            get => Playable.ContentType switch
            {
                PlayableItem.MediaContentType.Image => "이미지",
                PlayableItem.MediaContentType.Video => "비디오",
                PlayableItem.MediaContentType.Text => "텍스트",
                _ => ""
            };
        }

        public string Name
        {
            get
            {
                switch (Playable.ContentType)
                {
                    case PlayableItem.MediaContentType.Image:
                        string imageName = Path.GetFileName(((ImageContent)Playable.Content).Path);
                        return imageName.Length > 16 ? imageName.Substring(0, 13) + "..." : imageName;
                    case PlayableItem.MediaContentType.Video:
                        string videoName = Path.GetFileName(((VideoContent)Playable.Content).Path);
                        return videoName.Length > 16 ? videoName.Substring(0, 13) + "..." : videoName;
                    case PlayableItem.MediaContentType.Text:
                        string text = ((TextContent)Playable.Content).Text.ReplaceLineEndings(" ");
                        return text.Length > 16 ? text.Substring(0, 13) + "..." : text;
                    default:
                        return "";
                }
            }
        }

        private double _scaleFactor { get; set; }
        public double ScaleFactor
        {
            get => _scaleFactor;
            set
            {
                _scaleFactor = value;
                OnScaleFactorChanged();
            }
        }

        private Rect _layerArea { get; set; }
        public Rect LayerArea
        {
            get => _layerArea;
            set
            {
                _layerArea = value;
                OnLayerSizeChanged();
            }
        }

        public double X
        {
            get
            {
                return Playable.ContentType switch
                {
                    PlayableItem.MediaContentType.Image => ActualX * ScaleFactor,
                    PlayableItem.MediaContentType.Video => ActualX * ScaleFactor,
                    PlayableItem.MediaContentType.Text => ActualX,
                    _ => 0,
                };
            }
        }

        public double Y
        {
            get
            {
                return Playable.ContentType switch
                {
                    PlayableItem.MediaContentType.Image => ActualY * ScaleFactor,
                    PlayableItem.MediaContentType.Video => ActualY * ScaleFactor,
                    PlayableItem.MediaContentType.Text => ActualY,
                    _ => 0,
                };
            }
        }

        public double Width
        {
            get
            {
                return Playable.ContentType switch
                {
                    PlayableItem.MediaContentType.Image => ActualWidth * ScaleFactor,
                    PlayableItem.MediaContentType.Video => ActualWidth * ScaleFactor,
                    _ => ActualWidth,
                };
            }
        }

        public double Height
        {
            get
            {
                return Playable.ContentType switch
                {
                    PlayableItem.MediaContentType.Image => ActualHeight * ScaleFactor,
                    PlayableItem.MediaContentType.Video => ActualHeight * ScaleFactor,
                    _ => ActualHeight,
                };
            }
        }

        public Rect Bounds { get => new Rect(X, Y, Width, Height); }

        public string Source
        {
            get
            {
                return Playable.ContentType switch
                {
                    PlayableItem.MediaContentType.Image => ((ImageContent)Playable.Content).Path,
                    PlayableItem.MediaContentType.Video => ((VideoContent)Playable.Content).Path,
                    PlayableItem.MediaContentType.Text => ((TextContent)Playable.Content).Text,
                    _ => ""
                };
            }
        }

        public double ActualX
        {
            get
            {
                return Playable.ContentType switch
                {
                    PlayableItem.MediaContentType.Image => Playable.EvaluateX(LayerArea.Size),
                    PlayableItem.MediaContentType.Video => Playable.EvaluateX(LayerArea.Size),
                    _ => 0,
                };
            }
        }
        public double ActualY
        {
            get
            {
                return Playable.ContentType switch
                {
                    PlayableItem.MediaContentType.Image => Playable.EvaluateY(LayerArea.Size),
                    PlayableItem.MediaContentType.Video => Playable.EvaluateY(LayerArea.Size),
                    _ => 0,
                };
            }
        }
        public double ActualWidth
        {
            get
            {
                return Playable.ContentType switch
                {
                    PlayableItem.MediaContentType.Image => Playable.EvaluateWidth(LayerArea.Size),
                    PlayableItem.MediaContentType.Video => Playable.EvaluateWidth(LayerArea.Size),
                    _ => Playable.Width,
                };
            }
        }
        public double ActualHeight
        {
            get
            {
                return Playable.ContentType switch
                {
                    PlayableItem.MediaContentType.Image => Playable.EvaluateHeight(LayerArea.Size),
                    PlayableItem.MediaContentType.Video => Playable.EvaluateHeight(LayerArea.Size),
                    _ => Playable.Height,
                };
            }
        }

        public Rect ActualBounds { get => new Rect(ActualX, ActualY, ActualWidth, ActualHeight); }

        public PlayableItemViewModel(PlayableItem playableItem, Rect layerArea, double scaleFactor)
        {
            Playable = playableItem;
            ScaleFactor = scaleFactor;
            _layerArea = layerArea;
        }



        private void OnLayerSizeChanged()
        {
            OnPropertyChanged(nameof(LayerArea));
            OnPropertyChanged(nameof(ActualX));
            OnPropertyChanged(nameof(ActualY));
            OnPropertyChanged(nameof(ActualWidth));
            OnPropertyChanged(nameof(ActualHeight));
            OnPropertyChanged(nameof(ActualBounds));
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(Bounds));
        }

        private void OnScaleFactorChanged()
        {
            OnPropertyChanged(nameof(ScaleFactor));
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(Bounds));
        }

        public virtual void OnDetailChanged()
        {
            OnPropertyChanged(nameof(Source));
            OnPropertyChanged(nameof(ActualX));
            OnPropertyChanged(nameof(ActualY));
            OnPropertyChanged(nameof(ActualWidth));
            OnPropertyChanged(nameof(ActualHeight));
            OnPropertyChanged(nameof(ActualBounds));
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(Bounds));
        }

    }

    public class TextPlayableItemViewModel: PlayableItemViewModel
    {

        public int FontSize
        {
            get => Playable.ContentType switch
            {
                PlayableItem.MediaContentType.Text => ((TextContent)Playable.Content).FontSize,
                _ => 10,
            };
        }

        public string FontColor
        {
            get => ((TextContent)Playable.Content).FontColor;
        }

        public string VerticalAlignment
        {
            get => Playable.Content.VerticalAlignment switch
            {
                MediaContent.VerticalAlignmentType.Top => "Top",
                MediaContent.VerticalAlignmentType.Center => "Center",
                MediaContent.VerticalAlignmentType.Bottom => "Bottom",
                _ => "Center"
            };
        }

        public string HorizontalAlignment
        {
            get => Playable.Content.HorizontalAlignment switch
            {
                MediaContent.HorizontalAlignmentType.Left => "Left",
                MediaContent.HorizontalAlignmentType.Center => "Center",
                MediaContent.HorizontalAlignmentType.Right => "Right",
                _ => "Center"
            };
        }

        public TextPlayableItemViewModel(PlayableItem playableItem, Rect layerArea, double scaleFactor) : base(playableItem, layerArea, scaleFactor)
        {

        }
        public override void OnDetailChanged()
        {
            base.OnDetailChanged();
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(VerticalAlignment));
            OnPropertyChanged(nameof(HorizontalAlignment));
            OnPropertyChanged(nameof(FontSize));
            OnPropertyChanged(nameof(FontColor));
        }

    }

    public class LayerScheduleInfoViewModel : ObservableObject
    {
        private LayerSchedulerWindowViewModel _parent;
        private Schedule _schedule;

        public string RepeatType
        {
            get
            {
                switch (_schedule.Period)
                {
                    case Schedule.RepeatPeriod.Day:
                        return "매일";
                    case Schedule.RepeatPeriod.Week:
                        return "매주";
                    case Schedule.RepeatPeriod.Month:
                        return "매달";
                    case Schedule.RepeatPeriod.Year:
                        return "매년";
                    default:
                        return "";
                }
            }
        }

        private string RepeatDayOfWeek
        {
            get => DateUtils.GetKoreanDayOfWeek(_schedule.PeriodDayOfWeek);
        }

        public string RepeatPeriod
        {
            get
            {
                string startTime = _schedule.StartTime.ToString("HH:mm:ss");
                return _schedule.Period switch
                {
                    Schedule.RepeatPeriod.Day => $"{startTime}",
                    Schedule.RepeatPeriod.Week => $"{RepeatDayOfWeek}요일 {startTime}",
                    Schedule.RepeatPeriod.Month => $"{_schedule.PeriodDay}일 {startTime}",
                    Schedule.RepeatPeriod.Year => $"{_schedule.PeriodMonth}월 {_schedule.PeriodDay}일 {startTime}",
                    _ => "",
                };
            }
        }

        public string TimeInfo
        {
            get
            {
                return _schedule.Type switch
                {
                    ScheduleType.Repeat => $"{RepeatType} {RepeatPeriod}",
                    ScheduleType.Disposable => _schedule.StartDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    _ => "",
                };
            }
        }

        public bool TotalDurationReadOnly { get => !HasTotalDuration; }
        public bool HasTotalDuration
        {
            get => _schedule.HasDuration;
            set
            {
                _schedule.HasDuration = value;
                if (!_schedule.HasDuration)
                {
                    _schedule.Duration = _schedule.ItemsDuration;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalDurationReadOnly));
                OnPropertyChanged(nameof(TotalDuration));
            }
        }

        public double TotalDuration
        {
            get { return _schedule.Duration; }
            set
            {
                if (value >= 0 && HasTotalDuration)
                {
                    _schedule.Duration = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _selectedScheduleType { get; set; }
        public string SelectedScheduleType
        {
            get { return _selectedScheduleType; }
            set
            {
                ChangeScheduleType(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimeInfo));
            }
        }
        public Rect LayerArea { get => _parent.Layer.Bounds; }

        private const double MAX_SCALE_FACTOR = 1.0;
        private const double MIN_SCALE_FACTOR = 0.1;
        private const double DEFAULT_SCALE_FACTOR = 0.5;
        private const double SCALE_INCR_DELTA = 1.1;
        private const double SCALE_DECR_DELTA = 0.9;

        private double _scaleFactor = DEFAULT_SCALE_FACTOR;
        public double ScaleFactor
        {
            get => _scaleFactor;
            set
            {
                _scaleFactor = Math.Max(MIN_SCALE_FACTOR, Math.Min(MAX_SCALE_FACTOR, value));
                OnScaleFactorChanged();
            }
        }

        public double CanvasWidth
        {
            get => LayerArea.Width * ScaleFactor;
        }

        public double CanvasHeight
        {
            get => LayerArea.Height * ScaleFactor;
        }

        public string BackgroundColor
        {
            get => SelectedPlayable == null ? AppConfig.PrimaryColor.ToString() : SelectedPlayable.Playable.BackgroundColor;
        }

        public Geometry ClipGeometry
        {
            get => new RectangleGeometry(new Rect(0, 0, CanvasWidth, CanvasHeight));
        }

        private ObservableCollection<PlayableItemViewModel> _playables = [];
        public ObservableCollection<PlayableItemViewModel> Playables
        {
            get { return _playables; }
            set
            {
                _playables = value;
                OnPropertyChanged();
            }
        }


        private PlayableItemViewModel? _selectedPlayable;
        public PlayableItemViewModel? SelectedPlayable
        {
            get { return _selectedPlayable; }
            set
            {
                _selectedPlayable = value;
                if (value == null)
                {
                    SelectedPlayableDetail = null;
                }
                else
                {
                    SelectedPlayableDetail = value.Playable.ContentType switch
                    {
                        PlayableItem.MediaContentType.Image => new ImagePlayableDetailViewModel(this, value.Playable),
                        PlayableItem.MediaContentType.Video => new VideoPlayableDetailViewModel(this, value.Playable),
                        PlayableItem.MediaContentType.Text => new TextPlayableDetailViewModel(this, value.Playable),
                        _ => null
                    };
                }
                OnPropertyChanged();
            }
        }

        private object? _selectedPlayableDetail;
        public object? SelectedPlayableDetail
        {
            get { return _selectedPlayableDetail; }
            set
            {
                _selectedPlayableDetail = value;
                OnPropertyChanged();
            }
        }


        public ICommand ShowListCommand { get; set; }
        public ICommand AddImageContentCommand { get; set; }
        public ICommand AddVideoContentCommand { get; set; }
        public ICommand AddTextContentCommand { get; set; }
        public ICommand CanvasMouseWheelCommand { get; set; }

        public LayerScheduleInfoViewModel(LayerSchedulerWindowViewModel parent, Schedule schedule)
        {
            _parent = parent;
            _schedule = schedule;
            _selectedScheduleType = _schedule.Type.ToString().ToLower();
            
            UpdatePlayables();

            ShowListCommand = new RelayCommand(ShowList);
            AddImageContentCommand = new RelayCommand(AddImageContent);
            AddVideoContentCommand = new RelayCommand(AddVideoContent);
            AddTextContentCommand = new RelayCommand(AddTextContent);
            CanvasMouseWheelCommand = new RelayCommand<MouseWheelEventArgs?>(OnCanvasMouseWheel);
        }

        public void ShowList()
        {
            _parent.ShowList();
        }

        private void UpdatePlayables()
        {
            var playables = new ObservableCollection<PlayableItemViewModel>();
            foreach (var playableItem in _schedule.Items)
            {
                playables.Add(new PlayableItemViewModel(playableItem, LayerArea, ScaleFactor));
            }

            Playables = playables;
        }

        private void ChangeScheduleType(string option)
        {
            switch (option)
            {
                case "default":
                    ChangeToDefaultSchedule();
                    break;
                case "repeat":
                    ChangeToRepeatSchedule();
                    break;
                case "disposable":
                    ChangeToDisposableSchedule();
                    break;
                default:
                    break;
            }
        }

        private void ChangeToDefaultSchedule()
        {
            UpdateLayerSchedules(Schedule.ScheduleType.Default);
            _selectedScheduleType = _schedule.Type.ToString().ToLower();
        }

        private void ChangeToRepeatSchedule()
        {
            var repeatTimeSelectionDialog = new RepeatTimeSelectionDialog();
            repeatTimeSelectionDialog.ShowDialog();
            if (repeatTimeSelectionDialog.DialogResult == true)
            {
                if (repeatTimeSelectionDialog.RepeatType == Schedule.RepeatPeriod.Week)
                {
                    _schedule.PeriodDayOfWeek = repeatTimeSelectionDialog.RepeatDayOfWeek;
                }
                else if (repeatTimeSelectionDialog.RepeatType == Schedule.RepeatPeriod.Month)
                {
                    _schedule.PeriodDay = repeatTimeSelectionDialog.SelectedDay;
                }
                else if (repeatTimeSelectionDialog.RepeatType == Schedule.RepeatPeriod.Year)
                {
                    _schedule.PeriodMonth = repeatTimeSelectionDialog.SelectedMonth;
                    _schedule.PeriodDay = repeatTimeSelectionDialog.SelectedDay;
                }
                var startTime = repeatTimeSelectionDialog.SelectedTime;
                _schedule.StartTime = startTime;
                _schedule.Period = repeatTimeSelectionDialog.RepeatType;
                UpdateLayerSchedules(ScheduleType.Repeat);
            }
            _selectedScheduleType = _schedule.Type.ToString().ToLower();
        }

        private void ChangeToDisposableSchedule()
        {
            var calendarDialog = new CalendarDialog(_schedule.Type == ScheduleType.Disposable ? _schedule.StartDateTime : null);
            calendarDialog.ShowDialog();
            if (calendarDialog.DialogResult == true)
            {
                var startDateTime = calendarDialog.SelectedDateTime;
                _schedule.StartDateTime = startDateTime;
                UpdateLayerSchedules(Schedule.ScheduleType.Disposable);
            }
            _selectedScheduleType = _schedule.Type.ToString().ToLower();
        }

        private void UpdateLayerSchedules(Schedule.ScheduleType scheduleType)
        {
            if (_schedule.Type == Schedule.ScheduleType.Disposable)
            {
                _parent.Layer.DisposableSchedules.Remove(_schedule);
            }
            else if (_schedule.Type == Schedule.ScheduleType.Repeat)
            {
                _parent.Layer.RepeatSchedules.Remove(_schedule);
            }
            else
            {
                _parent.Layer.DefaultSchedules.Remove(_schedule);
            }

            _schedule.Type = scheduleType;

            if (_schedule.Type == Schedule.ScheduleType.Disposable)
            {
                _parent.Layer.DisposableSchedules.Add(_schedule);
            }
            else if (_schedule.Type == Schedule.ScheduleType.Repeat)
            {
                _parent.Layer.RepeatSchedules.Add(_schedule);
            }
            else
            {
                _parent.Layer.DefaultSchedules.Add(_schedule);
            }
        }
        private void OnScaleFactorChanged()
        {
            OnPropertyChanged(nameof(ScaleFactor));

            foreach (var playable in Playables)
            {
                playable.ScaleFactor = ScaleFactor;
            }

            OnPropertyChanged(nameof(CanvasWidth));
            OnPropertyChanged(nameof(CanvasHeight));
            OnPropertyChanged(nameof(ClipGeometry));
        }

        private void AddImageContent()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "이미지 선택",
                Filter = "이미지 파일 (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
                var imageContent = new ImageContent(path);
                var playableItem = new PlayableItem(imageContent);
                var playableViewModel = new PlayableItemViewModel(playableItem, LayerArea, ScaleFactor);

                _schedule.AddPlayable(playableItem);
                Playables.Add(playableViewModel);
                SelectedPlayable = playableViewModel;

                OnTotalDurationChanged();
            }
        }

        private void AddVideoContent()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "비디오 선택",
                Filter = "비디오 파일 (*.mp4;*.avi;*.mkv;*.mov)|*.mp4;*.avi;*.mkv;*.mov",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
                var videoContent = new VideoContent(path);
                var playableItem = new PlayableItem(videoContent);
                var playableViewModel = new PlayableItemViewModel(playableItem, LayerArea, ScaleFactor);

                _schedule.AddPlayable(playableItem);
                Playables.Add(playableViewModel);
                SelectedPlayable = playableViewModel;

                OnTotalDurationChanged();
            }
        }

        private void AddTextContent()
        {
            var textContent = new TextContent("새로운 텍스트");
            var playableItem = new PlayableItem(textContent);
            var playableViewModel = new TextPlayableItemViewModel(playableItem, LayerArea, ScaleFactor);

            _schedule.AddPlayable(playableItem);
            Playables.Add(playableViewModel);
            SelectedPlayable = playableViewModel;

            OnTotalDurationChanged();
        }

        public void OnTotalDurationChanged()
        {
            if (_schedule.HasDuration)
            {
                _schedule.Duration = TotalDuration;
            }
            else
            {
                _schedule.Duration = _schedule.ItemsDuration;
            }
            OnPropertyChanged(nameof(TotalDuration));
        }

        public void OnPlayableDetailChanged()
        {
            SelectedPlayable?.OnDetailChanged();

            OnPropertyChanged(nameof(BackgroundColor));

            OnTotalDurationChanged();
        }

        /*
         * 캔버스에서의 마우스 휠 이벤트
         * 휠 이벤트에 따라 ScaleFactor를 조정해서 캔버스 크기를 변경
         * 마우스 커서가 위치한 지점을 기준으로 확대하도록 작성
         * 
         * ! Issue: 확대할수록 커서 지점에서 벗어나는 문제 발생
         */
        private void OnCanvasMouseWheel(MouseWheelEventArgs? e)
        {
            if (e == null) return;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                DependencyObject? originalSource = e.OriginalSource as DependencyObject;
                Canvas? canvas = UIHelper.FindParent<Canvas>(originalSource);
                if (canvas == null) return;
                var scrollViewer = UIHelper.FindParent<ScrollViewer>(canvas);
                if (scrollViewer == null) return;

                var canvasMousePos = e.GetPosition(canvas);
                var scrollViewerMousePos = e.GetPosition(scrollViewer);

                double zoomFactor = e.Delta > 0 ? SCALE_INCR_DELTA : SCALE_DECR_DELTA;
                double oldScale = ScaleFactor;
                double newScale = ScaleFactor * zoomFactor;

                ScaleFactor = newScale;

                double offsetX = (scrollViewer.HorizontalOffset + scrollViewerMousePos.X) * (newScale / oldScale) - scrollViewerMousePos.X;
                double offsetY = (scrollViewer.VerticalOffset + scrollViewerMousePos.Y) * (newScale / oldScale) - scrollViewerMousePos.Y;

                scrollViewer.ScrollToHorizontalOffset(offsetX);
                scrollViewer.ScrollToVerticalOffset(offsetY);

                e.Handled = true;
            }
        }

        internal void UpdateLayerSize()
        {
            foreach (var playable in Playables)
            {
                playable.LayerArea = LayerArea;
            }
            OnPropertyChanged(nameof(ClipGeometry));
            OnPropertyChanged(nameof(CanvasWidth));
            OnPropertyChanged(nameof(CanvasHeight));
        }
    }
}
