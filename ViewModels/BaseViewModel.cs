using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Interfaces;

namespace WorkStatus.ViewModels
{
   public class BaseViewModel
    {
        protected IThemeManager _themeManager;
        protected IService _service;
        public BaseViewModel(IThemeManager themeManager, IService service)
        {
            _themeManager = themeManager;
            _service = service;
        }

       // public Color AppBackground { get { return _themeManager.BackgroundColor; } }
        public Color TextBoxBackgroundColor { get { return _themeManager.TextBoxBackgroundColor; } }
        public Color TextBoxTextColor { get { return _themeManager.TextBoxTextColor; } }
        //public int FontSize { get { return _themeManager.FontNormalSize; } }
        //public int FontSizeXSmall { get { return _themeManager.FontSmallSize; } }
        //public int FontSizeMedium { get { return _themeManager.FontMediumSize; } }
        //public int FontSizeLarge { get { return _themeManager.FontLargeSize; } }
    }
}
