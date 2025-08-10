using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QI_CMS_v2.Models;
using QI_CMS_v2.Utils;
using System.Windows.Input;

namespace QI_CMS_v2.ViewModels
{
    public class LayerScheduleListItemViewModel : ObservableObject
    {
        public Schedule CurrentSchedule { get; set; }

        public string CreatedAt { get => CurrentSchedule.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"); }
        public double Duration { get => CurrentSchedule.Duration; }
        public string RepeatType
        {
            get
            {
                switch (CurrentSchedule.Period)
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
            get => DateUtils.GetKoreanDayOfWeek(CurrentSchedule.PeriodDayOfWeek);
        }

        public string StartTime
        {
            get
            {
                if (CurrentSchedule.Type == Schedule.ScheduleType.Repeat)
                {
                    return CurrentSchedule.StartTime.ToString("HH:mm:ss");
                }
                else if (CurrentSchedule.Type == Schedule.ScheduleType.Disposable)
                {
                    return CurrentSchedule.StartDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    return "";
                }
            }
        }

        public string RepeatPeriod
        {
            get
            {
                return CurrentSchedule.Period switch
                {
                    Schedule.RepeatPeriod.Day => $"{StartTime}",
                    Schedule.RepeatPeriod.Week => $"{RepeatDayOfWeek}요일 {StartTime}",
                    Schedule.RepeatPeriod.Month => $"{CurrentSchedule.PeriodDay}일 {StartTime}",
                    Schedule.RepeatPeriod.Year => $"{CurrentSchedule.PeriodMonth}월 {CurrentSchedule.PeriodDay}일 {StartTime}",
                    _ => "",
                };
            }
        }

        public LayerScheduleListItemViewModel(Schedule schedule)
        {
            CurrentSchedule = schedule;
        }
    }

    public class LayerScheduleListViewModel : ObservableObject
    {

        private LayerSchedulerWindowViewModel _parent;
        private DisplayLayer _layer;
        public string Name { get => _layer.Name; }

        public ObservableCollection<LayerScheduleListItemViewModel> DefaultItems { get; set; } = [];
        public ObservableCollection<LayerScheduleListItemViewModel> RepeatItems { get; set; } = [];
        public ObservableCollection<LayerScheduleListItemViewModel> DisposableItems { get; set; } = [];

        public LayerScheduleListItemViewModel? SelectedSchedule
        {
            get { return null; }
            set
            {
                ShowDetail(value);
            }
        }

        public ICommand AddScheduleCommand { get; set; }
        public LayerScheduleListViewModel(LayerSchedulerWindowViewModel parent, DisplayLayer layer)
        {
            _parent = parent;
            _layer = layer;
            UpdateSchedule();
            AddScheduleCommand = new RelayCommand(AddSchedule);
        }

        public void UpdateSchedule()
        {
            foreach (var schedule in _layer.DefaultSchedules)
            {
                DefaultItems.Add(new LayerScheduleListItemViewModel(schedule));
            }
            foreach (var schedule in _layer.RepeatSchedules)
            {
                RepeatItems.Add(new LayerScheduleListItemViewModel(schedule));
            }
            foreach (var schedule in _layer.DisposableSchedules)
            {
                DisposableItems.Add(new LayerScheduleListItemViewModel(schedule));
            }
        }

        public void ShowDetail(LayerScheduleListItemViewModel? selectedItem)
        {
            if (selectedItem == null) return;
            _parent.ShowDetail(selectedItem.CurrentSchedule);
        }

        public void AddSchedule()
        {
            var schedule = new Schedule();
            _layer.DefaultSchedules.Add(schedule);
            _parent.ShowDetail(schedule);
        }

    }

}
