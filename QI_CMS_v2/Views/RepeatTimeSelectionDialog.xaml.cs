using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Input;
using QI_CMS_v2.Models;
using QI_CMS_v2.Utils;

namespace QI_CMS_v2.Views
{
    /// <summary>
    /// RepeatTimeSelectionDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RepeatTimeSelectionDialog : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        private string _selectedPeriod;
        public Schedule.RepeatPeriod RepeatType;
        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                _selectedPeriod = value;
                RepeatType = ConvertStringToPeriod(value);
                UpdateComboBox();
                OnPropertyChanged();
            }
        }

        public Visibility DayOfWeekVisibility
        {
            get => RepeatType == Schedule.RepeatPeriod.Week ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility MonthVisibility
        {
            get => RepeatType == Schedule.RepeatPeriod.Year ? Visibility.Visible : Visibility.Collapsed;
        }
        
        public Visibility DayVisibility
        {
            get => RepeatType == Schedule.RepeatPeriod.Month || RepeatType == Schedule.RepeatPeriod.Year ? Visibility.Visible : Visibility.Collapsed;
        }

        private string _selectedDayOfWeek;
        public DayOfWeek RepeatDayOfWeek;
        public string SelectedDayOfWeek
        {
            get => _selectedDayOfWeek;
            set
            {
                _selectedDayOfWeek = value;
                RepeatDayOfWeek = DateUtils.GetDayOfWeekFromKorean(value);
                OnPropertyChanged();
            }
        }

        private int _selectedMonth = 1;
        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                _selectedMonth = value;
                FillDays(value);
                OnPropertyChanged();
            }
        }

        private int _selectedDay = 1;
        public int SelectedDay
        {
            get => _selectedDay;
            set
            {
                _selectedDay = value;
                OnPropertyChanged();
            }
        }


        private int _selectedHour;
        public int SelectedHour
        {
            get => _selectedHour;
            set
            {
                _selectedHour = value;
                OnPropertyChanged();
            }
        }

        private int _selectedMinute;
        public int SelectedMinute
        {
            get => _selectedMinute;
            set
            {
                _selectedMinute = value;
                OnPropertyChanged();
            }
        }

        private int _selectedSecond;
        public int SelectedSecond
        {
            get => _selectedSecond;
            set
            {
                _selectedSecond = value;
                OnPropertyChanged();
            }
        }

        public TimeOnly SelectedTime { get => new TimeOnly(SelectedHour, SelectedMinute, SelectedSecond); }

        public ObservableCollection<string> Periods { get; } = ["일", "주", "달", "년"];
        public ObservableCollection<string> DayOfWeeks { get; } = new ObservableCollection<string>();
        public ObservableCollection<int> Months { get; } = new ObservableCollection<int>();
        public ObservableCollection<int> Days { get; } = new ObservableCollection<int>();
        public ObservableCollection<int> Hours { get; } = new ObservableCollection<int>();
        public ObservableCollection<int> Minutes { get; } = new ObservableCollection<int>();
        public ObservableCollection<int> Seconds { get; } = new ObservableCollection<int>();
        public ICommand ConfirmCommand { get; set; }

        public RepeatTimeSelectionDialog()
        {
            InitializeComponent();
            DataContext = this;

            SelectedPeriod = Periods[0];

            FillDayOfWeeks();
            FillMonths();

            // 시간 선택 목록 초기화
            for (int i = 0; i < 24; i++) Hours.Add(i);
            for (int i = 0; i < 60; i++)
            {
                Minutes.Add(i);
                Seconds.Add(i);
            }

            ConfirmCommand = new RelayCommand(Confirm);
        }

        private Schedule.RepeatPeriod ConvertStringToPeriod(string value)
        {
            return value switch
            {
                "일" => Schedule.RepeatPeriod.Day,
                "주" => Schedule.RepeatPeriod.Week,
                "달" => Schedule.RepeatPeriod.Month,
                "년" => Schedule.RepeatPeriod.Year,
                _ => Schedule.RepeatPeriod.None,
            };
        }

        private void FillDayOfWeeks()
        {
            DayOfWeeks.Clear();
            foreach (var dayOfWeek in DateUtils.DAY_OF_WEEKS)
            {
                DayOfWeeks.Add(DateUtils.GetKoreanDayOfWeek(dayOfWeek));
            }
            SelectedDayOfWeek = DayOfWeeks[0];
        }

        private void FillMonths()
        {
            Months.Clear();
            for (int i = 1; i <= 12; i++)
            {
                Months.Add(i);
            }
            SelectedMonth = Months[0];
        }

        private void FillDays(int month = 0)
        {
            Days.Clear();
            
            int[] thirties = [4, 6, 9, 11];

            int endDay = 31;
            if (month == 2) 
                endDay = 29;
            else if (thirties.Any(x => x == month)) 
                endDay = 30;

            for (int i = 1; i <= endDay; i++)
            {
                Days.Add(i);
            }

            SelectedDay = 1;
        }

        private void UpdateComboBox()
        {
            if (RepeatType == Schedule.RepeatPeriod.Month)
            {
                FillDays();
            }
            else if (RepeatType == Schedule.RepeatPeriod.Year)
            {
                FillDays(SelectedMonth);
            }
            OnPropertyChanged(nameof(DayOfWeekVisibility));
            OnPropertyChanged(nameof(MonthVisibility));
            OnPropertyChanged(nameof(DayVisibility));
        }
        private void Confirm()
        {
            DialogResult = true;
            Close();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
