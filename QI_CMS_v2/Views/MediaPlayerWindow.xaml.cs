using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using QI_CMS_v2.Models;
using Vortice.Direct2D1;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DirectWrite;
using Vortice.DXGI;
using Vortice.MediaFoundation;
using static QI_CMS_v2.Utils.DataStructure;
using FeatureLevel = Vortice.Direct3D.FeatureLevel;

namespace QI_CMS_v2.Views
{
    public class ScheduleItem: ActualScheduleItem
    {
        public bool IsLoaded { get; set; } = false;
        public bool IsPlaying { get; set; } = false;

        public ScheduleItem(ActualScheduleItem actualScheduleItem)
        {
            base.Start = actualScheduleItem.Start;
            base.StartPosition = actualScheduleItem.StartPosition;
            base.Duration = actualScheduleItem.Duration;
            
            base.X = actualScheduleItem.X;
            base.Y = actualScheduleItem.Y;
            base.Width = actualScheduleItem.Width;
            base.Height = actualScheduleItem.Height;

            base.Type = actualScheduleItem.Type;
            base.Content = actualScheduleItem.Content;
            base.BackgroundColor = actualScheduleItem.BackgroundColor;
        }

    }

    /// <summary>
    /// MediaPlayerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MediaPlayerWindow : Window
    {
        private LayerScheduler _scheduler;
        private Rect Bounds { get => _scheduler.Layer.Bounds; }

        private ConsumerFirstQueue<ScheduleItem> _items;
        private CancellationTokenSource _cancellationTokenSource;
        private Mutex _itemLock;
        private ScheduleItem _currentItem;
        private ScheduleItem _nextItem;

        private bool _ready = false;
        private bool _isPlaying = false;

        private DispatcherTimer _dispatcherTimer;
        private MediaElement _currentVideoPlayer;
        private MediaElement _nextVideoPlayer;

        private BrushConverter _brushConverter = new BrushConverter();

        public MediaPlayerWindow(LayerScheduler scheduler)
        {
            InitializeComponent();
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            _scheduler = scheduler;
            Left = Bounds.Left;
            Top = Bounds.Top;
            Width = Bounds.Width;
            Height = Bounds.Height;

            _cancellationTokenSource = new CancellationTokenSource();
            _items = new ConsumerFirstQueue<ScheduleItem>();

            _currentVideoPlayer = FirstMediaPlayerVideo;
            _nextVideoPlayer = SecondMediaPlayerVideo;

            // 시간 체크 타이머 시작
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = TimeSpan.FromMicroseconds(100);
            _dispatcherTimer.Tick += CheckTime;
            _dispatcherTimer.Start();
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (!_ready) return;


        }

        public void PlayCurrentItem()
        {
            Debug.WriteLine($"Current Item : {_currentItem.Start} - {_currentItem.End}");
            switch (_currentItem.Type)
            {
                case PlayableItem.MediaContentType.Text:
                    ShowText();
                    break;
                case PlayableItem.MediaContentType.Image:
                    ShowImage();
                    break;
                case PlayableItem.MediaContentType.Video:
                    ShowVideo();
                    break;
                default:
                    break;
            }
            PrepareNextVideo();
        }

        private void ShowText()
        {
            var brushConverter = new BrushConverter();
            Background = _brushConverter.ConvertFromString(_currentItem.BackgroundColor) as Brush;

            var textContent = (TextContent) _currentItem.Content;

            MediaPlayerText.FontSize = textContent.FontSize;
            MediaPlayerText.FontFamily = new FontFamily(textContent.FontFamily);
            MediaPlayerText.Foreground = _brushConverter.ConvertFromString(textContent.FontColor) as Brush;

            MediaPlayerText.Text = textContent.Text;

            MediaPlayerText.Width = _currentItem.Width;
            MediaPlayerText.Height = _currentItem.Height;
            Canvas.SetLeft(MediaPlayerText, _currentItem.X);
            Canvas.SetTop(MediaPlayerText, _currentItem.Y);

            ChangeMediaVisibility();
        }

        private void ChangeVideoPlayer()
        {
            if (_currentVideoPlayer == FirstMediaPlayerVideo)
            {
                _currentVideoPlayer = SecondMediaPlayerVideo;
                _nextVideoPlayer = FirstMediaPlayerVideo;
            }
            else
            {
                _currentVideoPlayer = FirstMediaPlayerVideo;
                _nextVideoPlayer = SecondMediaPlayerVideo;
            }
        }

        private void ShowVideo()
        {
            var brushConverter = new BrushConverter();
            Background = _brushConverter.ConvertFromString(_currentItem.BackgroundColor) as Brush;

            _nextVideoPlayer.Width = _currentItem.Width;
            _nextVideoPlayer.Height = _currentItem.Height;
            Canvas.SetLeft(_nextVideoPlayer, _currentItem.X);
            Canvas.SetTop(_nextVideoPlayer, _currentItem.Y);

            ChangeVideoPlayer();

            _currentVideoPlayer.Play();

            ChangeMediaVisibility();
        }

        private void ShowImage()
        {
            var brushConverter = new BrushConverter();
            Background = _brushConverter.ConvertFromString(_currentItem.BackgroundColor) as Brush;

            var imageContent = (ImageContent)_currentItem.Content;

            MediaPlayerImage.Width = _currentItem.Width;
            MediaPlayerImage.Height = _currentItem.Height;
            Canvas.SetLeft(MediaPlayerImage, _currentItem.X);
            Canvas.SetTop(MediaPlayerImage, _currentItem.Y);

            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.UriSource = new Uri(imageContent.Path);
            imageSource.EndInit();
            MediaPlayerImage.Source = imageSource;

            ChangeMediaVisibility();
        }

        private void ChangeMediaVisibility()
        {
            if (_currentItem.Type == PlayableItem.MediaContentType.Image)
            {
                MediaPlayerText.Visibility = Visibility.Collapsed;
                MediaPlayerImage.Visibility = Visibility.Visible;
                _nextVideoPlayer.Visibility = Visibility.Collapsed;
                _currentVideoPlayer.Visibility = Visibility.Collapsed;
            }
            else if (_currentItem.Type == PlayableItem.MediaContentType.Video)
            {
                MediaPlayerText.Visibility = Visibility.Collapsed;
                MediaPlayerImage.Visibility = Visibility.Collapsed;
                _currentVideoPlayer.Visibility = Visibility.Visible;
                _nextVideoPlayer.Visibility = Visibility.Collapsed;
                _nextVideoPlayer.Stop();
            }
            else if (_currentItem.Type == PlayableItem.MediaContentType.Text)
            {
                MediaPlayerText.Visibility = Visibility.Visible;
                MediaPlayerImage.Visibility = Visibility.Collapsed;
                _nextVideoPlayer.Visibility = Visibility.Collapsed;
                _currentVideoPlayer.Visibility = Visibility.Collapsed;
            }
            else
            {
                MediaPlayerText.Visibility = Visibility.Collapsed;
                MediaPlayerImage.Visibility = Visibility.Collapsed;
                _nextVideoPlayer.Visibility = Visibility.Collapsed;
                _currentVideoPlayer.Visibility = Visibility.Collapsed;
            }
        }

        private void MediaPlayerVideo_Loaded(object sender, RoutedEventArgs e)
        {
            var mediaElement = (MediaElement)sender;

            mediaElement.Play();
            mediaElement.Pause();
        }
        

        public void PrepareNextVideo()
        {
            if (_nextItem != null && _nextItem.IsLoaded)
            {
                if (_nextItem.Type == PlayableItem.MediaContentType.Video)
                {
                    var videoContent = (VideoContent) _nextItem.Content;
                    _nextVideoPlayer.Source = new Uri(videoContent.Path);
                }
            }
        }
        public void PrepareCurrentVideo()
        {
            if (_currentItem.Type == PlayableItem.MediaContentType.Video)
            {
                var videoContent = (VideoContent)_currentItem.Content;
                _nextVideoPlayer.Source = new Uri(videoContent.Path);
            }
        }

        public async void Play()
        {
            try
            {
                // 재생 준비
                (_currentItem, bool success) = await _items.DequeueAsync();
                await GetNextItem();
                PrepareCurrentVideo();
                _ready = true;
            }
            catch
            {
                Dispose();
            }
        }

        // 다음 아이템을 현재 아이템으로 할당
        // 다음 재생할 아이템을 큐에서 가져옴.
        private async Task GetNextItem()
        {
            try
            {
                (_nextItem, var success) = await _items.DequeueAsync(cancellationToken: _cancellationTokenSource.Token);
                if (success)
                {
                    _nextItem.IsLoaded = true;
                    if (_items.Count < 5)
                    {
                        _scheduler.Enqueue(1);
                    }
                }
            }
            catch (OperationCanceledException e)
            {

            }
        }

        // 일정 간격마다 현재 미디어가 시작할 시간인지 체크
        // 미디어가 재생 중인 경우 재생 시간이 끝났는지 체크
        public void CheckTime(object sender, System.EventArgs e)
        {
            if (!_ready) return;
            var now = DateTime.Now;

            // 재생 중인 경우
            if (_isPlaying)
            {
                // 미디어 재생 시간이 지났을 때
                if (now >= _currentItem.End)
                {
                    // 미디어 재생 종료
                    _isPlaying = false;

                    // 현재 미디어가 아직도 로딩 안 됐으면 취소
                    if (!_currentItem.IsLoaded) _cancellationTokenSource.Cancel();

                    // 다음 미디어로 전환
                    _currentItem = _nextItem;

                    // 다음 재생될 미디어 비동기 로드
                    _cancellationTokenSource = new CancellationTokenSource();
                    GetNextItem();
                }
            }

            // 재생 중이 아닌 경우
            if (!_isPlaying)
            {
                // 현재 미디어 시작 시간이 지났을 때
                if (_currentItem.Start <= now && now < _currentItem.End)
                {
                    // 1초 이상 차이 나면 보정
                    if (_currentItem.Start + TimeSpan.FromSeconds(1) <= now)
                    {
                        var gap = _currentItem.End - now;
                        _currentItem.StartPosition += now - _currentItem.Start;
                        _currentItem.Start = now;
                    }
                    PlayCurrentItem();
                    _isPlaying = true;
                }
                else if (now >= _currentItem.End)
                {
                    // 다음 미디어로 전환
                    _currentItem = _nextItem;

                    // 다음 재생될 미디어 비동기 로드
                    _cancellationTokenSource = new CancellationTokenSource();
                    GetNextItem();
                }
            }
        }

        public void Enqueue(List<ActualScheduleItem> items)
        {
            foreach (var item in items)
            {
                bool success = _items.Enqueue(new ScheduleItem(item));
            }
        }

        public void Add(List<ActualScheduleItem> items)
        {
            if (items.Count > 0)
            {
                Task.Run(() => Enqueue(items));
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            _dispatcherTimer.Stop();
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            Close();
        }
    }
}
