using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Draw_Balloon_net 
{
    public class BaloonSettingViewModel : ViewModelBase
    {
        #region Constructor
        public BaloonSettingViewModel()
        {
            // set value for controls in dialog.
            restoreSetting();

            setInforLayer();
        }

        #endregion


        #region State of selecting entities
        private bool _isSelectedAny = false;
        public bool IsSelectedAny
        {
            get
            {
                return _isSelectedAny;
            }

            set
            {
                if (_isSelectedAny != value)
                {
                    _isSelectedAny = value;
                    notifyPropertyChanged("IsSelectedAny");
                }
            }
        }


        private bool _isSelectedAll = true;
        public bool IsSelectedAll
        {
            get
            {
                return _isSelectedAll;
            }

            set
            {
                if (_isSelectedAll != value)
                {
                    _isSelectedAll = value;
                    notifyPropertyChanged("IsSelectedAll");
                }
            }
        }
        #endregion


        #region Property of Circle
        private string _text = "";
        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                if (_text != value)
                {
                    _text = value;
                    notifyPropertyChanged("Text");
                }
            }

        }


        private double _circleDiameter = 0;
        public double CircleDiameter
        {
            get
            {
                return _circleDiameter;
            }

            set
            {
                if (_circleDiameter != value)
                {
                    _circleDiameter = value;
                    notifyPropertyChanged("CircleDiameter");
                }
            }
        }
        #endregion


        #region Color
        private ObservableCollection<string> _color = new ObservableCollection<string>() {"Red", "Yellow", "Green", "Cyan", "Blue", "Magenta", "White", "Gray"};
        public ObservableCollection<string> Color
        {
            get
            {
                return _color;
            }
        }

        private string _selectedTextColor = "Red";
        public string SelectedTextColor
        {
            get
            {
                return _selectedTextColor;
            }

            set
            {
                if (_selectedTextColor != value)
                {
                    _selectedTextColor = value;
                    notifyPropertyChanged("SelectedTextColor");
                }                
            }
        }


        private string _selectedLineColor = "Red";
        public string SelectedLineColor
        {
            get
            {
                return _selectedLineColor;
            }

            set
            {
                if (_selectedLineColor != value)
                {
                    _selectedLineColor = value;
                    notifyPropertyChanged("SelectedLineColor");
                }
            }
        }


        private string _selectedCircleColor = "Red";
        public string SelectedCircleColor
        {
            get
            {
                return _selectedCircleColor;
            }

            set
            {
                if (_selectedCircleColor != value)
                {
                    _selectedCircleColor = value;
                    notifyPropertyChanged("SelectedCircleColor");
                }
            }
        }

        #endregion


        #region Layer
        private ObservableCollection<string> _textLayer = null;
        public ObservableCollection<string> TextLayer
        {
            get
            {
                return _textLayer;
            }

            set
            {
                if (_textLayer != value)
                {
                    _textLayer = value;
                    notifyPropertyChanged("TextLayer");
                }
            }
        }


        private ObservableCollection<string> _lineLayer = null;
        public ObservableCollection<string> LineLayer
        {
            get
            {
                return _lineLayer;
            }

            set
            {
                if (_lineLayer != value)
                {
                    _lineLayer = value;
                    notifyPropertyChanged("LineLayer");
                }
            }
        }


        private ObservableCollection<string> _circleLayer = null;
        public ObservableCollection<string> CircleLayer
        {
            get
            {
                return _circleLayer;
            }

            set
            {
                if (_circleLayer != value)
                {
                    _circleLayer = value;
                    notifyPropertyChanged("CircleLayer");
                }
            }
        }


        private string _selectedTextLayer = "";
        public string SelectedTextLayer
        {
            get
            {
                return _selectedTextLayer;
            }

            set
            {
                if (_selectedTextLayer != value)
                {
                    _selectedTextLayer = value;
                    notifyPropertyChanged("SelectedTextLayer");
                }
            }
        }


        private string _selectedLineLayer = "";
        public string SelectedLineLayer
        {
            get
            {
                return _selectedLineLayer;
            }

            set
            {
                if (_selectedLineLayer != value)
                {
                    _selectedLineLayer = value;
                    notifyPropertyChanged("SelectedLineLayer");
                }
            }
        }


        private string _selectedCircleLayer = "";
            public string SelectedCircleLayer
        {
            get
            {
                return _selectedCircleLayer;
            }

            set
            {
                if (_selectedCircleLayer != value)
                {
                    _selectedCircleLayer = value;
                    notifyPropertyChanged("SelectedCircleLayer");
                }
            }
        }
        #endregion


        #region Commands
        #region OK Command
        private RelayCommand _okCommand = null;
        public RelayCommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(p => savePropertiesIntoSetting());
                }

                return _okCommand;
            }
        }

        private void savePropertiesIntoSetting()
        {
            // save informations to setting object.
            updateSetting();

            // reflect setting to AutoCAD.
            reflectSettingToAutoCAD();

            CloseAction();
        }
        #endregion


        #region Cancel Command
        private RelayCommand _cancelCommand = null;
        public RelayCommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(p => disablePropertiesIntoSetting());
                }

                return _cancelCommand;
            }
        }

        private void disablePropertiesIntoSetting()
        {
            // erase data on dialog.
            restoreSetting();

            // close dialog.
            CloseAction();
        }
        #endregion


        #region Close dialog
        public Action CloseAction { get; set; }

        #endregion

        #endregion


        #region Update and Restore data in Settings
        private void updateSetting()
        {
            Settings setting = Settings.getInstance();

            setting.Text = Text;
            setting.Diameter = CircleDiameter;

            setting.ColorText = SelectedTextColor;
            setting.ColorLine = SelectedLineColor;
            setting.ColorCircle = SelectedCircleColor;

            setting.IndexColorText = setting.convertStringColorToIndex(SelectedTextColor);
            setting.IndexColorLine = setting.convertStringColorToIndex(SelectedLineColor);
            setting.IndexColorCircle = setting.convertStringColorToIndex(SelectedCircleColor);

            setInforLayer();

            setting.IsSelectedAny = IsSelectedAny;
            setting.IsSelectedAll = IsSelectedAll;                        
        }


        private void reflectSettingToAutoCAD()
        {
            if (IsSelectedAny)
            {
                ImplementationDatabase.updateValueForSelectedElements();
            }
            else
            {
                ImplementationDatabase.updateValueForAllOfElements();
            }            
        }


        private void restoreSetting()
        {
            Settings setting = Settings.getInstance();

            Text = setting.Text;
            CircleDiameter = setting.Diameter;

            SelectedTextColor = setting.ColorText;
            SelectedLineColor = setting.ColorLine;
            SelectedCircleColor = setting.ColorCircle;

            IsSelectedAny = setting.IsSelectedAny;
            IsSelectedAll = setting.IsSelectedAll;
        }


        private void setInforLayer()
        {
            // set information for Layer.
            ObservableCollection<string> obserCollect = ImplementationDatabase.getLayers();
            TextLayer = obserCollect;
            LineLayer = obserCollect;
            CircleLayer = obserCollect;
        }
        #endregion
    }
}
