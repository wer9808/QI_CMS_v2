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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace QI_CMS_v2.Views
{
    /// <summary>
    /// CalendarDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CalendarDialog : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;


        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                _selectedDate = value;
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

        public DateTime SelectedDateTime => new DateTime(
            SelectedDate?.Year ?? DateTime.Now.Year,
            SelectedDate?.Month ?? DateTime.Now.Month,
            SelectedDate?.Day ?? DateTime.Now.Day,
            SelectedHour, SelectedMinute, SelectedSecond);

        public ObservableCollection<int> Hours { get; } = new ObservableCollection<int>();
        public ObservableCollection<int> Minutes { get; } = new ObservableCollection<int>();
        public ObservableCollection<int> Seconds { get; } = new ObservableCollection<int>();


        public ICommand ConfirmCommand { get; }

        public CalendarDialog(DateTime? prevDateTime = null)
        {
            InitializeComponent();
            DataContext = this;

            // 시간 선택 목록 초기화
            for (int i = 0; i < 24; i++) Hours.Add(i);
            for (int i = 0; i < 60; i++)
            {
                Minutes.Add(i);
                Seconds.Add(i);
            }

            if (prevDateTime == null)
            {
                // 기본 선택값
                SelectedDate = DateTime.Today;
                SelectedHour = DateTime.Now.Hour;
                SelectedMinute = DateTime.Now.Minute;
                SelectedSecond = DateTime.Now.Second;
            }
            else
            {
                SelectedDate = prevDateTime.Value.Date;
                SelectedHour = prevDateTime.Value.Hour;
                SelectedMinute = prevDateTime.Value.Minute;
                SelectedSecond = prevDateTime.Value.Second;
            }

            ConfirmCommand = new RelayCommand(Confirm);
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
