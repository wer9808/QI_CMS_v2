using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QI_CMS_v2.ViewModels
{

    public class MainWindowViewModel: ObservableObject
    {

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

        public ObservableCollection<object> Tabs { get; set; } = [];

        public ICommand SelectTabCommand { get; set; }

        public MainWindowViewModel()
        {
            var displayCanvas = new DisplayLayerConfigViewModel();
            var displayState = new DisplayStateViewModel();

            Tabs.Add(displayCanvas);
            Tabs.Add(displayState);

            CurrentViewModel = Tabs[0];
            SelectTabCommand = new RelayCommand<object>(tab => CurrentViewModel = tab);
        }

    }
}
