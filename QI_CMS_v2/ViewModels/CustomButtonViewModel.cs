using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace QI_CMS_v2.ViewModels
{
    public class CustomButtonViewModel<T>: ObservableObject
    {

        public T Value { get; set; }
        public string Text { get; set; }
        public Action<T> OnClick { get; set; }

        private Brush _backgroundColor { get; set; } = AppConfig.PrimaryColor;
        public Brush BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }

        private Brush _foregroundColor { get; set; } = Brushes.White;
        public Brush ForegroundColor
        {
            get => _foregroundColor;
            set
            {
                _foregroundColor = value;
                OnPropertyChanged();
            }
        }


        public CustomButtonViewModel(T value, string text)
        {
            Value = value;
            Text = text;
            OnClick = (v) => { };
        }

        

        public override string ToString()
        {
            return Text;
        }

    }
}
