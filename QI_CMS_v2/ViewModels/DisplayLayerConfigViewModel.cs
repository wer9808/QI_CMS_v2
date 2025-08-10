using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QI_CMS_v2.Models;
using QI_CMS_v2.Utils;
using QI_CMS_v2.Views;

namespace QI_CMS_v2.ViewModels
{
    public class DisplayRectViewModel: ObservableObject
    {
        public readonly DisplayInfo InfoModel;

        public DisplayRectViewModel(DisplayInfo info, double scaleFactor)
        {
            this.InfoModel = info;
            this.ScaleFactor = scaleFactor;
        }

        public string Name { get => InfoModel.Name.Split("\\").Last() ?? "Unknown"; }
        public string PositionDescription { get => $"X:{ActualX} Y:{ActualY}"; }
        public string SizeDescription { get => $"{ActualWidth}x{ActualHeight}"; }

        public static Brush DefaultBackgroundColor = Brushes.Black;
        public static Brush HighlightBackgroundColor = new BrushConverter().ConvertFromString("#443BA9FD") as Brush;
        public static Brush DefaultBorderBrush = new BrushConverter().ConvertFromString("#111111") as Brush;

        private Brush _backgroundColor = DefaultBackgroundColor;
        public Brush BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }

        private Brush _borderBrush = DefaultBorderBrush;
        public Brush BorderBrush
        {
            get => _borderBrush;
            set
            {
                _borderBrush = value;
                OnPropertyChanged();
            }
        }

        private double _scaleFactor {  get; set; }
        public double ScaleFactor
        {
            get => _scaleFactor;
            set
            {
                _scaleFactor = value;
                OnScaleFactorChanged();
            }
        }

        private Rect _totalDisplayArea { get => DisplayManager.Instance.TotalDisplayArea; }

        public double X
        {
            get => (InfoModel.X - _totalDisplayArea.X) * ScaleFactor;
        }

        public double Y
        {
            get => (InfoModel.Y - _totalDisplayArea.Y) * ScaleFactor;
        }

        public double Width
        {
            get => InfoModel.Width * ScaleFactor;
        }

        public double Height
        {
            get => InfoModel.Height * ScaleFactor;
        }

        public Rect Bounds { get => new Rect(X, Y, Width, Height); }

        public double ActualX
        {
            get => InfoModel.X;
        }
        public double ActualY
        {
            get => InfoModel.Y;
        }
        public double ActualWidth
        {
            get => InfoModel.PixelWidth;
        }
        public double ActualHeight
        {
            get => InfoModel.PixelHeight;
        }

        public Rect ActualBounds { get => new Rect(ActualX, ActualY, ActualWidth, ActualHeight); }

        private void OnScaleFactorChanged()
        {
            OnPropertyChanged(nameof(ScaleFactor));
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(Bounds));
        }

    }

    public class LayerRectViewModel: ObservableObject
    {

        public DisplayLayer LayerModel;
        public LayerSchedulerWindowViewModel? Scheduler;

        public LayerRectViewModel(DisplayLayer displayLayer, double scaleFactor)
        {
            this.LayerModel = displayLayer;
            this.ScaleFactor = scaleFactor;
        }

        public string Name { get => LayerModel.Name; }
        public int Id { get => LayerModel.Id; }

        public static Brush DefaultBackgroundColor = new BrushConverter().ConvertFromString("#103BA9FD") as Brush;
        public static Brush HighlightBackgroundColor = new BrushConverter().ConvertFromString("#443BA9FD") as Brush;
        public static Brush DefaultBorderBrush = new BrushConverter().ConvertFromString("#FF008CE6") as Brush;

        private Brush _backgroundColor = DefaultBackgroundColor;
        public Brush BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }

        private Brush _borderBrush = DefaultBorderBrush;
        public Brush BorderBrush
        {
            get => _borderBrush;
            set
            {
                _borderBrush = value;
                OnPropertyChanged();
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

        private Rect _totalDisplayArea { get => DisplayManager.Instance.TotalDisplayArea; }

        public double X
        {
            get => (LayerModel.X - _totalDisplayArea.X) * ScaleFactor;
        }

        public double Y
        {
            get => (LayerModel.Y - _totalDisplayArea.Y) * ScaleFactor;
        }

        public double Width
        {
            get => LayerModel.Width * ScaleFactor;
        }

        public double Height
        {
            get => LayerModel.Height * ScaleFactor;
        }

        public Rect Bounds { get => new Rect(X, Y, Width, Height); }

        private double _actualX
        {
            get => LayerModel.X;
            set
            {
                LayerModel.X = value;
            }
        }
        public double ActualX
        {
            get => _actualX;
            set
            {
                ChangeActualX(value);
            }
        }

        private double _actualY
        {
            get => LayerModel.Y;
            set
            {
                LayerModel.Y = value;
            }
        }
        public double ActualY
        {
            get => _actualY;
            set
            {
                ChangeActualY(value);
            }
        }

        private double _actualWidth
        {
            get => LayerModel.Width;
            set
            {
                LayerModel.Width = value;
            }
        }
        public double ActualWidth
        {
            get => _actualWidth;
            set
            {
                ChangeActualWidth(value);
            }
        }

        private double _actualHeight
        {
            get => LayerModel.Height;
            set
            {
                LayerModel.Height = value;
            }
        }
        public double ActualHeight
        {
            get => _actualHeight;
            set
            {
                ChangeActualHeight(value);
            }
        }
        public Rect ActualBounds { get => new Rect(ActualX, ActualY, ActualWidth, ActualHeight); }

        private void OnScaleFactorChanged()
        {
            OnPropertyChanged(nameof(ScaleFactor));
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(Bounds));
        }

        private bool IsValidX(double actualX)
        {
            return (actualX >= _totalDisplayArea.X) && (actualX + ActualWidth <= _totalDisplayArea.Right);
        }
        private bool IsValidY(double actualY)
        {
            return (actualY >= _totalDisplayArea.Y) && (actualY + ActualHeight <= _totalDisplayArea.Bottom);
        }
        private bool IsValidWidth(double actualWidth)
        {
            return (actualWidth >= 0) && (ActualX + actualWidth <= _totalDisplayArea.Right);
        }
        private bool IsValidHeight(double actualHeight)
        {
            return (actualHeight >= 0) && (ActualY + actualHeight <= _totalDisplayArea.Bottom);
        }

        public void ChangeActualX(double value)
        {
            if (IsValidX(value))
            {
                _actualX = value;
                OnPropertyChanged(nameof(X));
                OnPropertyChanged(nameof(ActualX));
                OnPropertyChanged(nameof(ActualBounds));
            }
        }

        public void ChangeActualY(double value)
        {
            if (IsValidY(value))
            {
                _actualY = value;
                OnPropertyChanged(nameof(Y));
                OnPropertyChanged(nameof(ActualY));
                OnPropertyChanged(nameof(ActualBounds));
            }
        }

        public void ChangeActualWidth(double value)
        {
            if (IsValidWidth(value)) 
            {
                _actualWidth = value;
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(ActualWidth));
                OnPropertyChanged(nameof(ActualBounds));
            }
        }

        public void ChangeActualHeight(double value)
        {
            if (IsValidHeight(value))
            {
                _actualHeight = value;
                OnPropertyChanged(nameof(Height));
                OnPropertyChanged(nameof(ActualHeight));
                OnPropertyChanged(nameof(ActualBounds));
            }
        }

    }

    public class DisplayLayerConfigViewModel: ObservableObject
    {

        private const double MAX_SCALE_FACTOR = 1.0;
        private const double MIN_SCALE_FACTOR = 0.1;
        private const double DEFAULT_SCALE_FACTOR = 0.3;
        private const double SCALE_INCR_DELTA = 1.1;
        private const double SCALE_DECR_DELTA = 0.9;

        public string Title { get; set; } = "화면설정";
        public Rect TotalDisplayArea { get => DisplayManager.Instance.TotalDisplayArea; }

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
            get => TotalDisplayArea.Width * ScaleFactor;
        }

        public double CanvasHeight
        {
            get => TotalDisplayArea.Height * ScaleFactor;
        }

        public enum DisplayCanvasItemSelectionMode { Display, Layer }
        private DisplayCanvasItemSelectionMode _itemSelectionMode = DisplayCanvasItemSelectionMode.Layer;
        public DisplayCanvasItemSelectionMode ItemSelectionMode
        {
            get => _itemSelectionMode;
            set
            {
                _itemSelectionMode = value;
                ChangeItemSelectionMode();
            }
        }

        public enum DisplayCanvasControlState
        {
            None, LayerAlignment, FitSize, LayerMove
        }

        private DisplayCanvasControlState _controlState = DisplayCanvasControlState.None;
        public DisplayCanvasControlState ControlState
        {
            get => _controlState;
            set
            {
                _controlState = value;
                OnPropertyChanged();
            }
        }

        public enum LayerAlignmentType
        {
            None, Left, Right, Top, Bottom, HorCenter, VerCenter
        }
        private LayerAlignmentType _layerAlignment = LayerAlignmentType.None;
        public LayerAlignmentType LayerAlignment
        {
            get => _layerAlignment;
            set
            {
                _layerAlignment = value;
                OnPropertyChanged();
            }
        }

        public enum LayerFitSizeType
        {
            None, FullSize, FullWidth, FullHeight, DisplaySize, DisplayWidth, DisplayHeight
        }
        private LayerFitSizeType _layerFitSize = LayerFitSizeType.None;
        public LayerFitSizeType LayerFitSize
        {
            get => _layerFitSize;
            set
            {
                _layerFitSize = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<DisplayRectViewModel> _displayRects = [];
        public ObservableCollection<DisplayRectViewModel> DisplayRects
        {
            get { return _displayRects; }
            set
            {
                _displayRects = value;
                OnPropertyChanged();
            }
        }

        private DisplayRectViewModel? _selectedDisplayRect;
        public DisplayRectViewModel? SelectedDisplayRect
        {
            get { return _selectedDisplayRect; }
            set
            {
                _selectedDisplayRect = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<LayerRectViewModel> _LayerRects = [];
        public ObservableCollection<LayerRectViewModel> LayerRects
        {
            get { return _LayerRects; }
            set
            {
                _LayerRects = value;
                OnPropertyChanged();
            }
        }


        private LayerRectViewModel? _selectedLayerRect;
        public LayerRectViewModel? SelectedLayerRect
        {
            get { return _selectedLayerRect; }
            set
            {
                if (_selectedLayerRect != null)
                {
                    _selectedLayerRect.BackgroundColor = LayerRectViewModel.DefaultBackgroundColor;
                }
                if (value != null)
                {
                    value.BackgroundColor = LayerRectViewModel.HighlightBackgroundColor;
                }
                _selectedLayerRect = value;
                OnPropertyChanged();
            }
        }

        private LayerSchedulerWindow? _layerSchedulerWindow;
        private LayerSchedulerWindowViewModel? _layerScheduler;

        private Point _prevMousePos;

        private bool _isPlaying = false;
        public bool IsPlaying { 
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged(nameof(PlayState));
            }
        }
        public string PlayState { get => _isPlaying ? "정지" : "재생"; }
        private bool _isRunCompleted = true;
        public bool IsRunCompleted
        {
            get => _isRunCompleted;
            set
            {
                _isRunCompleted = value;
                OnPropertyChanged();
            }
        }

        public ICommand RunCommand { get; set; }
        public ICommand AddLayerCommand { get; set; }
        public ICommand RemoveLayerCommand { get; set; }
        public ICommand ChangeLayerZCommand { get; set; }
        public ICommand CanvasMouseDownCommand { get; set; }
        public ICommand CanvasMouseMoveCommand { get; set; }
        public ICommand CanvasMouseUpCommand { get; set; }
        public ICommand CanvasMouseLeaveCommand { get; set; }
        public ICommand CanvasMouseWheelCommand { get; set; }
        public ICommand LayerHorAlignCommand { get; set; }
        public ICommand LayerVerAlignCommand { get; set; }
        public ICommand LayerFitSizeCommand { get; set; }
        public ICommand OpenLayerSchedulerCommand { get; set; }

        public DisplayLayerConfigViewModel()
        {
            UpdateDisplayRects();
            RunCommand = new RelayCommand(Run);
            AddLayerCommand = new RelayCommand<object>(AddLayer);
            RemoveLayerCommand = new RelayCommand<object>(RemoveLayer);
            ChangeLayerZCommand = new RelayCommand<string>(ChangeLayerZ);
            CanvasMouseDownCommand = new RelayCommand<MouseButtonEventArgs?>(OnCanvasMouseDown);
            CanvasMouseMoveCommand = new RelayCommand<MouseEventArgs?>(OnCanvasMouseMove);
            CanvasMouseUpCommand = new RelayCommand<MouseButtonEventArgs?>(OnCanvasMouseUp);
            CanvasMouseLeaveCommand = new RelayCommand<MouseEventArgs?>(OnCanvasMouseUp);
            CanvasMouseWheelCommand = new RelayCommand<MouseWheelEventArgs?>(OnCanvasMouseWheel);
            LayerHorAlignCommand = new RelayCommand<string>(SetHorAlignMode);
            LayerVerAlignCommand = new RelayCommand<string>(SetVerAlignMode);
            LayerFitSizeCommand = new RelayCommand<string>(SetLayerFitSizeMode);
            OpenLayerSchedulerCommand = new RelayCommand(OpenLayerScheduler);
        }

        private void OnScaleFactorChanged()
        {
            OnPropertyChanged(nameof(ScaleFactor));
            foreach (var displayRect in DisplayRects)
            {
                displayRect.ScaleFactor = ScaleFactor;
            }
            foreach (var layerRect in LayerRects)
            {
                layerRect.ScaleFactor = ScaleFactor;
            }
            OnPropertyChanged(nameof(CanvasWidth));
            OnPropertyChanged(nameof(CanvasHeight));
        }

        private void OnLayerSizeChanged()
        {
            if (_layerScheduler != null)
            {
                _layerScheduler.UpdateLayerSize();
            }
        }

        private void RemoveLayer(object? obj)
        {
            if (SelectedLayerRect != null)
            {
                DisplayLayerManager.Instance.Remove(SelectedLayerRect.Id);
                if (_layerScheduler != null && _layerSchedulerWindow != null && _layerScheduler.Layer == SelectedLayerRect.LayerModel)
                {
                    _layerSchedulerWindow.Close();
                }
                LayerRects.Remove(SelectedLayerRect);
            }
        }

        private void AddLayer(object? obj)
        {
            var displayLayer = DisplayLayerManager.Instance.Create();
            var LayerRect = new LayerRectViewModel(displayLayer, ScaleFactor);
            LayerRects.Add(LayerRect);
        }

        private void ChangeLayerZ(string direction)
        {
            if (SelectedLayerRect == null) return;

            switch (direction)
            {
                case "up":
                    DisplayLayerManager.Instance.SendBackward(SelectedLayerRect.Id);
                    break;
                case "down":
                    DisplayLayerManager.Instance.BringForward(SelectedLayerRect.Id);
                    break;
                default:
                    return;
            }

            UpdateLayerRects();
        }

        public void UpdateLayerRects()
        {
            var layersByZ = DisplayLayerManager.Instance.LayersByZ.Values.ToList();
            var layerRects = new ObservableCollection<LayerRectViewModel>();
            var selectedLayerRect = SelectedLayerRect;
            foreach (var layer in layersByZ)
            {
                var layerRect = new LayerRectViewModel(layer, ScaleFactor);
                layerRects.Add(layerRect);
            }
            LayerRects = layerRects;
            if (selectedLayerRect != null)
            {
                SelectedLayerRect = LayerRects.FirstOrDefault(layerRect => layerRect.Id == selectedLayerRect.Id);
            }
        }


        public void UpdateDisplayRects()
        {
            var displayInfos = DisplayManager.Instance.Displays;
            var displayRects = new ObservableCollection<DisplayRectViewModel>();
            foreach (var displayInfo in displayInfos) 
            {
                var displayRect = new DisplayRectViewModel(displayInfo, ScaleFactor);

                displayRects.Add(displayRect);
            }

            DisplayRects = displayRects;
        }

        private void ChangeItemSelectionMode()
        {
            switch (ItemSelectionMode)
            {
                case DisplayCanvasItemSelectionMode.Layer:
                    foreach (var layerRect in LayerRects)
                    {
                        if (SelectedLayerRect != null && SelectedLayerRect == layerRect)
                        {
                            layerRect.BackgroundColor = LayerRectViewModel.HighlightBackgroundColor;
                        }
                        else
                        {
                            layerRect.BackgroundColor = LayerRectViewModel.DefaultBackgroundColor;
                        }
                        layerRect.BorderBrush = LayerRectViewModel.DefaultBorderBrush;
                    }
                    foreach (var displayRect in DisplayRects)
                    {
                        displayRect.BackgroundColor = DisplayRectViewModel.DefaultBackgroundColor;
                    }
                    break;
                case DisplayCanvasItemSelectionMode.Display:
                    foreach (var layerRect in LayerRects)
                    {
                        layerRect.BackgroundColor = Brushes.Transparent;
                        layerRect.BorderBrush = Brushes.Transparent;
                    }
                    break;
            }
            OnPropertyChanged(nameof(ItemSelectionMode));
        }

        /*
         * 캔버스에서의 마우스 다운 이벤트
         * 
         * 뷰모델의 ItemSelectionMode에 따라 다르게 동작
         * 
         * # Layer Mode
         * 마우스 위치에 LayerRect가 있는지 검사. 있을 경우 해당 LayerRect를 선택
         * 
         */
        private void OnCanvasMouseDown(MouseButtonEventArgs? e)
        {
            if (e == null) return;
            DependencyObject? originalSource = e.OriginalSource as DependencyObject;
            Canvas? canvas = UIHelper.FindParent<Canvas>(originalSource);
            if (canvas == null) return;

            var canvasMousePos = e.GetPosition(canvas);

            if (ControlState == DisplayCanvasControlState.None)
            {
                if (ItemSelectionMode == DisplayCanvasItemSelectionMode.Layer)
                {
                    if (SelectedLayerRect == null || !SelectedLayerRect.Bounds.Contains(canvasMousePos))
                    {
                        SelectedLayerRect = LayerRects.LastOrDefault(layerRect => layerRect.Bounds.Contains(canvasMousePos));
                    }
                    StartMoveLayerWithMouse(canvasMousePos);
                }
            }
            else if (ControlState == DisplayCanvasControlState.LayerAlignment)
            {
                SelectedDisplayRect = DisplayRects.LastOrDefault(displayRect => displayRect.Bounds.Contains(canvasMousePos));
                AlignLayer();
            }
            else if (ControlState == DisplayCanvasControlState.FitSize)
            {
                SelectedDisplayRect = DisplayRects.LastOrDefault(displayRect => displayRect.Bounds.Contains(canvasMousePos));
                FitLayerSizeToDisplay();
            }
        }

        private void OnCanvasMouseMove(MouseEventArgs? e)
        {
            if (e == null) return;
            DependencyObject? originalSource = e.OriginalSource as DependencyObject;
            Canvas? canvas = UIHelper.FindParent<Canvas>(originalSource);
            if (canvas == null) return;

            var canvasMousePos = e.GetPosition(canvas);

            if (ItemSelectionMode == DisplayCanvasItemSelectionMode.Layer)
            {
                if (ControlState == DisplayCanvasControlState.LayerMove)
                {
                    MoveLayerWithMouse(canvasMousePos);
                }
            }
            else if (ItemSelectionMode == DisplayCanvasItemSelectionMode.Display)
            {
                foreach (var displayRect in DisplayRects)
                {
                    if (displayRect.Bounds.Contains(canvasMousePos))
                    {
                        displayRect.BackgroundColor = DisplayRectViewModel.HighlightBackgroundColor;
                    }
                    else
                    {
                        displayRect.BackgroundColor = DisplayRectViewModel.DefaultBackgroundColor;
                    }
                }
            }

        }

        private void OnCanvasMouseUp(MouseEventArgs? e)
        {
            if (e == null) return;

            if (ControlState == DisplayCanvasControlState.LayerMove)
            {
                EndMoveLayerWithMouse();
            }
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

                Debug.WriteLine($"CMP: {canvasMousePos.X}, {canvasMousePos.Y}");
                Debug.WriteLine($"SVMP: {scrollViewerMousePos.X}, {scrollViewerMousePos.Y}");

                double zoomFactor = e.Delta > 0 ? SCALE_INCR_DELTA : SCALE_DECR_DELTA;
                double oldScale = ScaleFactor;
                double newScale = ScaleFactor * zoomFactor;

                double relativeX = canvasMousePos.X / CanvasWidth;
                double relativeY = canvasMousePos.Y / CanvasHeight;

                ScaleFactor = newScale;

                double offsetX = (scrollViewer.HorizontalOffset + scrollViewerMousePos.X) * (newScale / oldScale) - scrollViewerMousePos.X;
                double offsetY = (scrollViewer.VerticalOffset + scrollViewerMousePos.Y) * (newScale / oldScale) - scrollViewerMousePos.Y;

                scrollViewer.ScrollToHorizontalOffset(offsetX);
                scrollViewer.ScrollToVerticalOffset(offsetY);

                e.Handled = true;
            }
        }

        private void SetHorAlignMode(string alignment)
        {
            if (SelectedLayerRect == null) return;
            alignment = alignment.ToLower();
            switch (alignment)
            {
                case "left":
                    LayerAlignment = LayerAlignmentType.Left;
                    ItemSelectionMode = DisplayCanvasItemSelectionMode.Display;
                    ControlState = DisplayCanvasControlState.LayerAlignment;
                    break;
                case "right":
                    LayerAlignment = LayerAlignmentType.Right;
                    ItemSelectionMode = DisplayCanvasItemSelectionMode.Display;
                    ControlState = DisplayCanvasControlState.LayerAlignment;
                    break;
                case "center":
                    LayerAlignment = LayerAlignmentType.HorCenter;
                    ItemSelectionMode = DisplayCanvasItemSelectionMode.Display;
                    ControlState = DisplayCanvasControlState.LayerAlignment;
                    break;
                default:
                    LayerAlignment = LayerAlignmentType.None;
                    break;
            }
        }

        private void SetVerAlignMode(string alignment)
        {
            if (SelectedLayerRect == null) return;
            ItemSelectionMode = DisplayCanvasItemSelectionMode.Display;
            ControlState = DisplayCanvasControlState.LayerAlignment;
            alignment = alignment.ToLower();
            switch (alignment)
            {
                case "top":
                    LayerAlignment = LayerAlignmentType.Top;
                    break;
                case "bottom":
                    LayerAlignment = LayerAlignmentType.Bottom;
                    break;
                case "center":
                    LayerAlignment = LayerAlignmentType.VerCenter;
                    break;
                default:
                    LayerAlignment = LayerAlignmentType.None;
                    break;
            }
        }

        private void SetLayerFitSizeMode(string type)
        {
            if (SelectedLayerRect == null) return;
            type = type.ToLower();
            switch (type)
            {
                case "fullsize":
                    LayerFitSize = LayerFitSizeType.FullSize;
                    FitLayerSizeToDisplay();
                    break;
                case "fullwidth":
                    LayerFitSize = LayerFitSizeType.FullWidth;
                    FitLayerSizeToDisplay();
                    break;
                case "fullheight":
                    LayerFitSize = LayerFitSizeType.FullHeight;
                    FitLayerSizeToDisplay();
                    break;
                case "displaysize":
                    ItemSelectionMode = DisplayCanvasItemSelectionMode.Display;
                    ControlState = DisplayCanvasControlState.FitSize;
                    LayerFitSize = LayerFitSizeType.DisplaySize;
                    break;
                case "displaywidth":
                    ItemSelectionMode = DisplayCanvasItemSelectionMode.Display;
                    ControlState = DisplayCanvasControlState.FitSize;
                    LayerFitSize = LayerFitSizeType.DisplayWidth;
                    break;
                case "displayheight":
                    ItemSelectionMode = DisplayCanvasItemSelectionMode.Display;
                    ControlState = DisplayCanvasControlState.FitSize;
                    LayerFitSize = LayerFitSizeType.DisplayHeight;
                    break;
                default:
                    LayerFitSize = LayerFitSizeType.None;
                    break;
            }
        }

        private void AlignLayer()
        {
            if (SelectedLayerRect != null && SelectedDisplayRect != null)
            {
                var displayBounds = SelectedDisplayRect.ActualBounds;
                var layerBounds = SelectedLayerRect.ActualBounds;

                bool canHorAlign = displayBounds.Width >= layerBounds.Width;
                bool canVerAlign = displayBounds.Height >= layerBounds.Height;

                switch (LayerAlignment)
                {
                    case LayerAlignmentType.Left:
                        if (canHorAlign) SelectedLayerRect.ActualX = displayBounds.X;
                        break;
                    case LayerAlignmentType.Right:
                        if (canHorAlign) SelectedLayerRect.ActualX = displayBounds.Right - layerBounds.Width;
                        break;
                    case LayerAlignmentType.Top:
                        if (canVerAlign) SelectedLayerRect.ActualY = displayBounds.Y;
                        break;
                    case LayerAlignmentType.Bottom:
                        if (canVerAlign) SelectedLayerRect.ActualY = displayBounds.Bottom - layerBounds.Height;
                        break;
                    case LayerAlignmentType.HorCenter:
                        if (canHorAlign) SelectedLayerRect.ActualX = displayBounds.X + (displayBounds.Width - layerBounds.Width) * 0.5;
                        break;
                    case LayerAlignmentType.VerCenter:
                        if (canVerAlign) SelectedLayerRect.ActualY = displayBounds.Y + (displayBounds.Height - layerBounds.Height) * 0.5;
                        break;
                    default:
                        break;
                }
            }
            ControlState = DisplayCanvasControlState.None;
            LayerAlignment = LayerAlignmentType.None;
            ItemSelectionMode = DisplayCanvasItemSelectionMode.Layer;
        }

        private void FitLayerSizeToDisplay()
        {

            if (SelectedLayerRect != null && SelectedDisplayRect != null)
            {
                var displayBounds = SelectedDisplayRect.ActualBounds;
                var layerBounds = SelectedLayerRect.ActualBounds;

                bool fromRight = layerBounds.X + displayBounds.Width > TotalDisplayArea.Right;
                bool fromBottom = layerBounds.Y + displayBounds.Height > TotalDisplayArea.Bottom;

                switch (LayerFitSize)
                {
                    case LayerFitSizeType.FullSize:
                        SelectedLayerRect.ActualX = TotalDisplayArea.X;
                        SelectedLayerRect.ActualY = TotalDisplayArea.Y;
                        SelectedLayerRect.ActualWidth = TotalDisplayArea.Width;
                        SelectedLayerRect.ActualHeight = TotalDisplayArea.Height;
                        break;
                    case LayerFitSizeType.FullWidth:
                        SelectedLayerRect.ActualX = TotalDisplayArea.X;
                        SelectedLayerRect.ActualWidth = TotalDisplayArea.Width;
                        break;
                    case LayerFitSizeType.FullHeight:
                        SelectedLayerRect.ActualY = TotalDisplayArea.Y;
                        SelectedLayerRect.ActualHeight = TotalDisplayArea.Height;
                        break;
                    case LayerFitSizeType.DisplaySize:
                        if (fromRight)
                        {
                            SelectedLayerRect.ActualX = displayBounds.X;
                            SelectedLayerRect.ActualWidth = displayBounds.Width;
                        }
                        else
                        {
                            SelectedLayerRect.ActualWidth = displayBounds.Width;
                            SelectedLayerRect.ActualX = displayBounds.X;
                        }

                        if (fromBottom)
                        {
                            SelectedLayerRect.ActualY = displayBounds.Y;
                            SelectedLayerRect.ActualHeight = displayBounds.Height;
                        }
                        else
                        {
                            SelectedLayerRect.ActualHeight = displayBounds.Height;
                            SelectedLayerRect.ActualY = displayBounds.Y;
                        }
                        break;
                    case LayerFitSizeType.DisplayWidth:
                        if (fromRight)
                        {
                            SelectedLayerRect.ActualX = TotalDisplayArea.Right - displayBounds.Width;
                        }
                        SelectedLayerRect.ActualWidth = displayBounds.Width;
                        break;
                    case LayerFitSizeType.DisplayHeight:
                        if (fromBottom)
                        {
                            SelectedLayerRect.ActualY = TotalDisplayArea.Bottom - displayBounds.Height;
                        }
                        SelectedLayerRect.ActualHeight = displayBounds.Height;
                        break;
                    default:
                        break;
                }
            }
            ControlState = DisplayCanvasControlState.None;
            LayerFitSize = LayerFitSizeType.None;
            ItemSelectionMode = DisplayCanvasItemSelectionMode.Layer;
            OnLayerSizeChanged();
        }

        private void StartMoveLayerWithMouse(Point mousePos)
        {
            if (SelectedLayerRect != null)
            {
                ControlState = DisplayCanvasControlState.LayerMove;
                Mouse.OverrideCursor = Cursors.SizeAll;
                _prevMousePos = mousePos;
            }
        }

        private void MoveLayerWithMouse(Point mousePos)
        {

            double deltaX = mousePos.X - _prevMousePos.X;
            double deltaY = mousePos.Y - _prevMousePos.Y;

            if (SelectedLayerRect != null)
            {
                double newX = SelectedLayerRect.ActualX + deltaX / ScaleFactor;
                double newY = SelectedLayerRect.ActualY + deltaY / ScaleFactor;

                newX = Math.Max(TotalDisplayArea.X, Math.Min(TotalDisplayArea.Right - SelectedLayerRect.ActualWidth, newX));
                newY = Math.Max(TotalDisplayArea.Y, Math.Min(TotalDisplayArea.Bottom - SelectedLayerRect.ActualHeight, newY));

                SelectedLayerRect.ActualX = newX;
                SelectedLayerRect.ActualY = newY;
            }

            _prevMousePos = mousePos;
        }

        private void EndMoveLayerWithMouse()
        {
            ControlState = DisplayCanvasControlState.None;
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void OpenLayerScheduler()
        {
            if (SelectedLayerRect == null) return;

            if (_layerSchedulerWindow == null)
            {
                _layerScheduler = new LayerSchedulerWindowViewModel(SelectedLayerRect.LayerModel);
                _layerSchedulerWindow = new LayerSchedulerWindow()
                {
                    DataContext = _layerScheduler,
                };
                _layerSchedulerWindow.Closed += (s, e) =>
                {
                    _layerScheduler = null;
                    _layerSchedulerWindow.DataContext = null;
                    _layerSchedulerWindow = null;
                };
                _layerSchedulerWindow.Show();
            }
            else
            {
                _layerScheduler = new LayerSchedulerWindowViewModel(SelectedLayerRect.LayerModel);
                _layerSchedulerWindow.DataContext = _layerScheduler;
                _layerSchedulerWindow.Activate();
            }
        }

        private void Run()
        {
            IsRunCompleted = false;
            if (IsPlaying)
            {
                Stop();
            }
            else
            {
                Play();
            }
            IsRunCompleted = true;
        }

        private void Play()
        {
            PlayScheduleManager.Instance.Play(DisplayLayerManager.Instance.Layers);
            IsPlaying = true;
        }

        private void Stop()
        {
            PlayScheduleManager.Instance.Stop();
            IsPlaying = false;
        }

    }
}
