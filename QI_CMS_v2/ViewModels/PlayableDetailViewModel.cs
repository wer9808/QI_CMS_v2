using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QI_CMS_v2.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QI_CMS_v2.ViewModels
{

    public class ImagePlayableDetailViewModel : ObservableObject
    {
        private LayerScheduleInfoViewModel _parent;
        public PlayableItem Playable;

        private ImageContent _content { get => (ImageContent)Playable.Content; }

        public double Duration
        {
            get => Playable.Duration;
            set
            {
                if (value >= 0)
                {
                    Playable.Duration = value;
                    OnPropertyChanged();
                    _parent.OnPlayableDetailChanged();
                }
            }
        }

        public string VerticalAlignment
        {
            get => _content.VerticalAlignment.ToString();
            set
            {
                if (Enum.TryParse(value, out MediaContent.VerticalAlignmentType verticalAlignment))
                {
                    _content.VerticalAlignment = verticalAlignment;
                    OnPropertyChanged();
                    _parent.OnPlayableDetailChanged();
                }
            }
        }

        public string HorizontalAlignment
        {
            get => _content.HorizontalAlignment.ToString();
            set
            {
                if (Enum.TryParse(value, out MediaContent.HorizontalAlignmentType horizontalAlignment))
                {
                    _content.HorizontalAlignment = horizontalAlignment;
                    OnPropertyChanged();
                    _parent.OnPlayableDetailChanged();
                }
            }
        }

        public ObservableCollection<CustomButtonViewModel<MediaContent.ScaleMethodType>> ScaleMethodButtons { get; set; } = [];
        public ObservableCollection<CustomButtonViewModel<MediaContent.VerticalAlignmentType>> VerticalAlignmentButtons { get; set; } = [];
        public ObservableCollection<CustomButtonViewModel<MediaContent.HorizontalAlignmentType>> HorizontalAlignmentButtons { get; set; } = [];

        private CustomButtonViewModel<MediaContent.ScaleMethodType> _selectedScaleMethod;
        public CustomButtonViewModel<MediaContent.ScaleMethodType> SelectedScaleMethod
        {
            get
            {
                return _selectedScaleMethod;
            }
            set
            {
                _selectedScaleMethod = value;
                foreach (var button in ScaleMethodButtons)
                {
                    if (button == value)
                    {
                        button.BackgroundColor = AppConfig.SecondaryColor;
                    }
                    else
                    {
                        button.BackgroundColor = AppConfig.PrimaryColor;
                    }
                }

                Playable.Content.ScaleMethod = value!.Value;
                OnPropertyChanged();
                _parent.OnPlayableDetailChanged();
            }
        }

        private CustomButtonViewModel<MediaContent.VerticalAlignmentType> _selectedVerticalAlignment;
        public CustomButtonViewModel<MediaContent.VerticalAlignmentType> SelectedVerticalAlignment
        {
            get
            {
                return _selectedVerticalAlignment;
            }
            set
            {
                _selectedVerticalAlignment = value;
                foreach (var button in VerticalAlignmentButtons)
                {
                    if (button == value)
                    {
                        button.BackgroundColor = AppConfig.SecondaryColor;
                    }
                    else
                    {
                        button.BackgroundColor = AppConfig.PrimaryColor;
                    }
                }
                Playable.Content.VerticalAlignment = value!.Value;
                OnPropertyChanged();
                _parent.OnPlayableDetailChanged();
            }
        }

        private CustomButtonViewModel<MediaContent.HorizontalAlignmentType> _selectedHorizontalAlignment;
        public CustomButtonViewModel<MediaContent.HorizontalAlignmentType> SelectedHorizontalAlignment
        {
            get
            {
                return _selectedHorizontalAlignment;
            }
            set
            {
                _selectedHorizontalAlignment = value;
                foreach (var button in HorizontalAlignmentButtons)
                {
                    if (button == value)
                    {
                        button.BackgroundColor = AppConfig.SecondaryColor;
                    }
                    else
                    {
                        button.BackgroundColor = AppConfig.PrimaryColor;
                    }
                }
                Playable.Content.HorizontalAlignment = value!.Value;
                OnPropertyChanged();
                _parent.OnPlayableDetailChanged();
            }
        }


        public ICommand ChangeScaleMethodCommand { get; set; }
        public ICommand ChangeVerAlignCommand { get; set; }
        public ICommand ChangeHorAlignCommand { get; set; }

        public ImagePlayableDetailViewModel(LayerScheduleInfoViewModel parent, PlayableItem playable)
        {
            _parent = parent;
            Playable = playable;

            CreateToggleButtonViewModels();

            SelectedScaleMethod = ScaleMethodButtons.First(b => b.Value == Playable.Content.ScaleMethod);
            SelectedVerticalAlignment = VerticalAlignmentButtons.First(b => b.Value == Playable.Content.VerticalAlignment);
            SelectedHorizontalAlignment = HorizontalAlignmentButtons.First(b => b.Value == Playable.Content.HorizontalAlignment);

            ChangeScaleMethodCommand = new RelayCommand<CustomButtonViewModel<MediaContent.ScaleMethodType>>(ChangeScaleMethod);
            ChangeVerAlignCommand = new RelayCommand<CustomButtonViewModel<MediaContent.VerticalAlignmentType>>(ChangeVerticalAlignment);
            ChangeHorAlignCommand = new RelayCommand<CustomButtonViewModel<MediaContent.HorizontalAlignmentType>>(ChangeHorizontalAlignment);
        }

        private void CreateToggleButtonViewModels()
        {
            ScaleMethodButtons = new ObservableCollection<CustomButtonViewModel<MediaContent.ScaleMethodType>>();
            VerticalAlignmentButtons = new ObservableCollection<CustomButtonViewModel<MediaContent.VerticalAlignmentType>>();
            HorizontalAlignmentButtons = new ObservableCollection<CustomButtonViewModel<MediaContent.HorizontalAlignmentType>>();

            ScaleMethodButtons.Add(new CustomButtonViewModel<MediaContent.ScaleMethodType>(MediaContent.ScaleMethodType.None, "원본 크기"));
            ScaleMethodButtons.Add(new CustomButtonViewModel<MediaContent.ScaleMethodType>(MediaContent.ScaleMethodType.KeepRatio, "원본 비율"));
            ScaleMethodButtons.Add(new CustomButtonViewModel<MediaContent.ScaleMethodType>(MediaContent.ScaleMethodType.FitWidth, "너비 맞춤"));
            ScaleMethodButtons.Add(new CustomButtonViewModel<MediaContent.ScaleMethodType>(MediaContent.ScaleMethodType.FitHeight, "높이 맞춤"));
            ScaleMethodButtons.Add(new CustomButtonViewModel<MediaContent.ScaleMethodType>(MediaContent.ScaleMethodType.Stretch, "레이어 맞춤"));

            VerticalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.VerticalAlignmentType>(MediaContent.VerticalAlignmentType.Top, "위쪽"));
            VerticalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.VerticalAlignmentType>(MediaContent.VerticalAlignmentType.Center, "중앙"));
            VerticalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.VerticalAlignmentType>(MediaContent.VerticalAlignmentType.Bottom, "아래쪽"));

            HorizontalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.HorizontalAlignmentType>(MediaContent.HorizontalAlignmentType.Left, "왼쪽"));
            HorizontalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.HorizontalAlignmentType>(MediaContent.HorizontalAlignmentType.Center, "중앙"));
            HorizontalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.HorizontalAlignmentType>(MediaContent.HorizontalAlignmentType.Right, "오른쪽"));
        }


        private void ChangeScaleMethod(CustomButtonViewModel<MediaContent.ScaleMethodType> buttonViewModel)
        {
            SelectedScaleMethod = buttonViewModel;
        }

        private void ChangeVerticalAlignment(CustomButtonViewModel<MediaContent.VerticalAlignmentType> buttonViewModel)
        {
            SelectedVerticalAlignment = buttonViewModel;
        }

        private void ChangeHorizontalAlignment(CustomButtonViewModel<MediaContent.HorizontalAlignmentType> buttonViewModel)
        {
            SelectedHorizontalAlignment = buttonViewModel;
        }

    }


    public class VideoPlayableDetailViewModel : ObservableObject
    {
        private LayerScheduleInfoViewModel _parent;
        public PlayableItem Playable;

        private VideoContent _content { get => (VideoContent)Playable.Content; }

        public double Duration
        {
            get => Playable.Duration;
            set
            {
                if (value >= 0)
                {
                    Playable.Duration = value;
                    OnPropertyChanged();
                    _parent.OnPlayableDetailChanged();
                }
            }
        }

        public string VerticalAlignment
        {
            get => _content.VerticalAlignment.ToString();
            set
            {
                if (Enum.TryParse(value, out MediaContent.VerticalAlignmentType verticalAlignment))
                {
                    _content.VerticalAlignment = verticalAlignment;
                    OnPropertyChanged();
                    _parent.OnPlayableDetailChanged();
                }
            }
        }

        public string HorizontalAlignment
        {
            get => _content.HorizontalAlignment.ToString();
            set
            {
                if (Enum.TryParse(value, out MediaContent.HorizontalAlignmentType horizontalAlignment))
                {
                    _content.HorizontalAlignment = horizontalAlignment;
                    OnPropertyChanged();
                    _parent.OnPlayableDetailChanged();
                }
            }
        }

        public ObservableCollection<CustomButtonViewModel<MediaContent.ScaleMethodType>> ScaleMethodButtons { get; set; } = [];
        public ObservableCollection<CustomButtonViewModel<MediaContent.VerticalAlignmentType>> VerticalAlignmentButtons { get; set; } = [];
        public ObservableCollection<CustomButtonViewModel<MediaContent.HorizontalAlignmentType>> HorizontalAlignmentButtons { get; set; } = [];

        private CustomButtonViewModel<MediaContent.ScaleMethodType> _selectedScaleMethod;
        public CustomButtonViewModel<MediaContent.ScaleMethodType> SelectedScaleMethod
        {
            get
            {
                return _selectedScaleMethod;
            }
            set
            {
                _selectedScaleMethod = value;
                foreach (var button in ScaleMethodButtons)
                {
                    if (button == value)
                    {
                        button.BackgroundColor = AppConfig.SecondaryColor;
                    }
                    else
                    {
                        button.BackgroundColor = AppConfig.PrimaryColor;
                    }
                }

                Playable.Content.ScaleMethod = value!.Value;
                OnPropertyChanged();
                _parent.OnPlayableDetailChanged();
            }
        }

        private CustomButtonViewModel<MediaContent.VerticalAlignmentType> _selectedVerticalAlignment;
        public CustomButtonViewModel<MediaContent.VerticalAlignmentType> SelectedVerticalAlignment
        {
            get
            {
                return _selectedVerticalAlignment;
            }
            set
            {
                _selectedVerticalAlignment = value;
                foreach (var button in VerticalAlignmentButtons)
                {
                    if (button == value)
                    {
                        button.BackgroundColor = AppConfig.SecondaryColor;
                    }
                    else
                    {
                        button.BackgroundColor = AppConfig.PrimaryColor;
                    }
                }
                Playable.Content.VerticalAlignment = value!.Value;
                OnPropertyChanged();
                _parent.OnPlayableDetailChanged();
            }
        }

        private CustomButtonViewModel<MediaContent.HorizontalAlignmentType> _selectedHorizontalAlignment;
        public CustomButtonViewModel<MediaContent.HorizontalAlignmentType> SelectedHorizontalAlignment
        {
            get
            {
                return _selectedHorizontalAlignment;
            }
            set
            {
                _selectedHorizontalAlignment = value;
                foreach (var button in HorizontalAlignmentButtons)
                {
                    if (button == value)
                    {
                        button.BackgroundColor = AppConfig.SecondaryColor;
                    }
                    else
                    {
                        button.BackgroundColor = AppConfig.PrimaryColor;
                    }
                }
                Playable.Content.HorizontalAlignment = value!.Value;
                OnPropertyChanged();
                _parent.OnPlayableDetailChanged();
            }
        }


        public ICommand ChangeScaleMethodCommand { get; set; }
        public ICommand ChangeVerAlignCommand { get; set; }
        public ICommand ChangeHorAlignCommand { get; set; }

        public VideoPlayableDetailViewModel(LayerScheduleInfoViewModel parent, PlayableItem playable)
        {
            _parent = parent;
            Playable = playable;

            CreateToggleButtonViewModels();

            SelectedScaleMethod = ScaleMethodButtons.First(b => b.Value == Playable.Content.ScaleMethod);
            SelectedVerticalAlignment = VerticalAlignmentButtons.First(b => b.Value == Playable.Content.VerticalAlignment);
            SelectedHorizontalAlignment = HorizontalAlignmentButtons.First(b => b.Value == Playable.Content.HorizontalAlignment);

            ChangeScaleMethodCommand = new RelayCommand<CustomButtonViewModel<MediaContent.ScaleMethodType>>(ChangeScaleMethod);
            ChangeVerAlignCommand = new RelayCommand<CustomButtonViewModel<MediaContent.VerticalAlignmentType>>(ChangeVerticalAlignment);
            ChangeHorAlignCommand = new RelayCommand<CustomButtonViewModel<MediaContent.HorizontalAlignmentType>>(ChangeHorizontalAlignment);
        }

        private void CreateToggleButtonViewModels()
        {
            ScaleMethodButtons = new ObservableCollection<CustomButtonViewModel<MediaContent.ScaleMethodType>>();
            VerticalAlignmentButtons = new ObservableCollection<CustomButtonViewModel<MediaContent.VerticalAlignmentType>>();
            HorizontalAlignmentButtons = new ObservableCollection<CustomButtonViewModel<MediaContent.HorizontalAlignmentType>>();

            ScaleMethodButtons.Add(new CustomButtonViewModel<MediaContent.ScaleMethodType>(MediaContent.ScaleMethodType.None, "원본 크기"));
            ScaleMethodButtons.Add(new CustomButtonViewModel<MediaContent.ScaleMethodType>(MediaContent.ScaleMethodType.KeepRatio, "원본 비율"));
            ScaleMethodButtons.Add(new CustomButtonViewModel<MediaContent.ScaleMethodType>(MediaContent.ScaleMethodType.FitWidth, "너비 맞춤"));
            ScaleMethodButtons.Add(new CustomButtonViewModel<MediaContent.ScaleMethodType>(MediaContent.ScaleMethodType.FitHeight, "높이 맞춤"));
            ScaleMethodButtons.Add(new CustomButtonViewModel<MediaContent.ScaleMethodType>(MediaContent.ScaleMethodType.Stretch, "레이어 맞춤"));

            VerticalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.VerticalAlignmentType>(MediaContent.VerticalAlignmentType.Top, "위쪽"));
            VerticalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.VerticalAlignmentType>(MediaContent.VerticalAlignmentType.Center, "중앙"));
            VerticalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.VerticalAlignmentType>(MediaContent.VerticalAlignmentType.Bottom, "아래쪽"));

            HorizontalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.HorizontalAlignmentType>(MediaContent.HorizontalAlignmentType.Left, "왼쪽"));
            HorizontalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.HorizontalAlignmentType>(MediaContent.HorizontalAlignmentType.Center, "중앙"));
            HorizontalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.HorizontalAlignmentType>(MediaContent.HorizontalAlignmentType.Right, "오른쪽"));
        }


        private void ChangeScaleMethod(CustomButtonViewModel<MediaContent.ScaleMethodType> buttonViewModel)
        {
            SelectedScaleMethod = buttonViewModel;
        }

        private void ChangeVerticalAlignment(CustomButtonViewModel<MediaContent.VerticalAlignmentType> buttonViewModel)
        {
            SelectedVerticalAlignment = buttonViewModel;
        }

        private void ChangeHorizontalAlignment(CustomButtonViewModel<MediaContent.HorizontalAlignmentType> buttonViewModel)
        {
            SelectedHorizontalAlignment = buttonViewModel;
        }

    }

    public class TextPlayableDetailViewModel: ObservableObject
    {

        private LayerScheduleInfoViewModel _parent;
        public PlayableItem Playable;

        private TextContent _content { get => (TextContent)Playable.Content; }

        public double Duration
        {
            get => Playable.Duration;
            set
            {
                if (value >= 0)
                {
                    Playable.Duration = value;
                    OnPropertyChanged();
                    _parent.OnPlayableDetailChanged();
                }
            }
        }

        public string Text
        {
            get => _content.Text;
            set
            {
                _content.Text = value;
                OnPropertyChanged();
                _parent.OnPlayableDetailChanged();
            }
        }

        public int FontSize
        {
            get => _content.FontSize;
            set
            {
                if (value >= 0)
                {
                    _content.FontSize = value;
                    OnPropertyChanged();
                    _parent.OnPlayableDetailChanged();
                }
            }
        }

        public string FontColor
        {
            get => _content.FontColor;
            set
            {
                if (value.StartsWith("#") && (value.Length == 9 || value.Length == 7))
                {
                    _content.FontColor = value;
                    OnPropertyChanged();
                    _parent.OnPlayableDetailChanged();
                }
            }
        }

        public string VerticalAlignment
        {
            get => _content.VerticalAlignment.ToString();
            set
            {
                if (Enum.TryParse(value, out MediaContent.VerticalAlignmentType verticalAlignment))
                {
                    _content.VerticalAlignment = verticalAlignment;
                    OnPropertyChanged();
                    _parent.OnPlayableDetailChanged();
                }
            }
        }

        public string HorizontalAlignment
        {
            get => _content.HorizontalAlignment.ToString();
            set
            {
                if (Enum.TryParse(value, out MediaContent.HorizontalAlignmentType horizontalAlignment))
                {
                    _content.HorizontalAlignment = horizontalAlignment;
                    OnPropertyChanged();
                    _parent.OnPlayableDetailChanged();
                }
            }
        }

        public ObservableCollection<CustomButtonViewModel<MediaContent.VerticalAlignmentType>> VerticalAlignmentButtons { get; set; } = [];
        public ObservableCollection<CustomButtonViewModel<MediaContent.HorizontalAlignmentType>> HorizontalAlignmentButtons { get; set; } = [];

        private CustomButtonViewModel<MediaContent.VerticalAlignmentType> _selectedVerticalAlignment;
        public CustomButtonViewModel<MediaContent.VerticalAlignmentType> SelectedVerticalAlignment
        {
            get
            {
                return _selectedVerticalAlignment;
            }
            set
            {
                _selectedVerticalAlignment = value;
                foreach (var button in VerticalAlignmentButtons)
                {
                    if (button == value)
                    {
                        button.BackgroundColor = AppConfig.SecondaryColor;
                    }
                    else
                    {
                        button.BackgroundColor = AppConfig.PrimaryColor;
                    }
                }
                Playable.Content.VerticalAlignment = value!.Value;
                OnPropertyChanged();
                _parent.OnPlayableDetailChanged();
            }
        }

        private CustomButtonViewModel<MediaContent.HorizontalAlignmentType> _selectedHorizontalAlignment;
        public CustomButtonViewModel<MediaContent.HorizontalAlignmentType> SelectedHorizontalAlignment
        {
            get
            {
                return _selectedHorizontalAlignment;
            }
            set
            {
                _selectedHorizontalAlignment = value;
                foreach (var button in HorizontalAlignmentButtons)
                {
                    if (button == value)
                    {
                        button.BackgroundColor = AppConfig.SecondaryColor;
                    }
                    else
                    {
                        button.BackgroundColor = AppConfig.PrimaryColor;
                    }
                }
                Playable.Content.HorizontalAlignment = value!.Value;
                OnPropertyChanged();
                _parent.OnPlayableDetailChanged();
            }
        }


        public ICommand ChangeVerAlignCommand { get; set; }
        public ICommand ChangeHorAlignCommand { get; set; }

        public TextPlayableDetailViewModel(LayerScheduleInfoViewModel parent, PlayableItem playable)
        {
            _parent = parent;
            Playable = playable;

            CreateToggleButtonViewModels();

            SelectedVerticalAlignment = VerticalAlignmentButtons.First(b => b.Value == Playable.Content.VerticalAlignment);
            SelectedHorizontalAlignment = HorizontalAlignmentButtons.First(b => b.Value == Playable.Content.HorizontalAlignment);

            ChangeVerAlignCommand = new RelayCommand<CustomButtonViewModel<MediaContent.VerticalAlignmentType>>(ChangeVerticalAlignment);
            ChangeHorAlignCommand = new RelayCommand<CustomButtonViewModel<MediaContent.HorizontalAlignmentType>>(ChangeHorizontalAlignment);
        }

        private void CreateToggleButtonViewModels()
        {
            VerticalAlignmentButtons = new ObservableCollection<CustomButtonViewModel<MediaContent.VerticalAlignmentType>>();
            HorizontalAlignmentButtons = new ObservableCollection<CustomButtonViewModel<MediaContent.HorizontalAlignmentType>>();

            VerticalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.VerticalAlignmentType>(MediaContent.VerticalAlignmentType.Top, "위쪽"));
            VerticalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.VerticalAlignmentType>(MediaContent.VerticalAlignmentType.Center, "중앙"));
            VerticalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.VerticalAlignmentType>(MediaContent.VerticalAlignmentType.Bottom, "아래쪽"));

            HorizontalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.HorizontalAlignmentType>(MediaContent.HorizontalAlignmentType.Left, "왼쪽"));
            HorizontalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.HorizontalAlignmentType>(MediaContent.HorizontalAlignmentType.Center, "중앙"));
            HorizontalAlignmentButtons.Add(new CustomButtonViewModel<MediaContent.HorizontalAlignmentType>(MediaContent.HorizontalAlignmentType.Right, "오른쪽"));
        }

        private void ChangeVerticalAlignment(CustomButtonViewModel<MediaContent.VerticalAlignmentType> buttonViewModel)
        {
            SelectedVerticalAlignment = buttonViewModel;
        }

        private void ChangeHorizontalAlignment(CustomButtonViewModel<MediaContent.HorizontalAlignmentType> buttonViewModel)
        {
            SelectedHorizontalAlignment = buttonViewModel;
        }

    }
}
