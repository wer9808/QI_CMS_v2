using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QI_CMS_v2.Models;
using QI_CMS_v2.Utils;
using QI_CMS_v2.Views;
using static QI_CMS_v2.Models.Schedule;

namespace QI_CMS_v2.ViewModels
{

    public class LayerSchedulerWindowViewModel : ObservableObject
    {
        public DisplayLayer Layer;

        private object _currentViewModel;
        public object CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public LayerSchedulerWindowViewModel(DisplayLayer layer)
        {
            Layer = layer;
            CurrentViewModel = new LayerScheduleListViewModel(this, Layer);
        }

        public void ChangeLayer(DisplayLayer layer)
        {
            Layer = layer;
            CurrentViewModel = new LayerScheduleListViewModel(this, Layer);
        }

        public void ShowList()
        {
            CurrentViewModel = new LayerScheduleListViewModel(this, Layer);
        }

        public void ShowDetail(Schedule schedule)
        {
            CurrentViewModel = new LayerScheduleInfoViewModel(this, schedule);
        }

        internal void UpdateLayerSize()
        {
            if (CurrentViewModel is LayerScheduleInfoViewModel infoViewModel)
            {
                infoViewModel.UpdateLayerSize();
            }
        }
    }
}
