using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Interfaces;

namespace WorkStatus.Configuration
{
    public class ThemeManager : IThemeManager
    {
        private SolidColorBrush _stackPanelLogoColor = "#1665D8".ToSolidColorBrush();
        public SolidColorBrush StackPanelLogoColor
        {
            get { return _stackPanelLogoColor; }
            set { _stackPanelLogoColor = value; }
        }

        private SolidColorBrush _txtWelcomeColor = "#FFFFFF".ToSolidColorBrush();
        public SolidColorBrush TxtWelcomeColor
        {
            get { return _txtWelcomeColor; }
            set { _txtWelcomeColor = value; }
        }

        private Color _textBoxBackgroundColor;
        public Color TextBoxBackgroundColor
        {
            get { return _textBoxBackgroundColor; }
            set { _textBoxBackgroundColor = value; }
        }

        private Color _textBoxTextColor = Avalonia.Media.Color.Parse("#1665D8");
        public Color TextBoxTextColor
        {
            get
            {
                return _textBoxTextColor;
            }
            set
            {
                _textBoxTextColor = value;
            }
        }
        private SolidColorBrush _onSiteColor = "#0AA7FF".ToSolidColorBrush();
        public SolidColorBrush OnSiteColor
        {
            get { return _onSiteColor; }
            set { _onSiteColor = value; }
        }
        private SolidColorBrush _offSiteColor = "#F5BE2F".ToSolidColorBrush();
        public SolidColorBrush OffSiteColor
        {
            get { return _offSiteColor; }
            set { _offSiteColor = value; }
        }
        /// <summary>
        /// The size of the font normal.
        /// </summary>
        private string _fontNormalSize = "13";
        public string FontNormalSize
        {
            get
            {
                return _fontNormalSize;
            }
            set
            {
                _fontNormalSize = value;
            }
        }

        /// <summary>
        /// The size of the font small.
        /// </summary>
        private string _fontSmallSize = "12";
        public string FontSmallSize
        {
            get
            {
                return _fontSmallSize;
            }
            set
            {
                _fontSmallSize = value;
            }
        }

        /// <summary>
        /// The size of the font medium.
        /// </summary>
        private string _fontMediumSize = "14";
        public string FontMediumSize
        {
            get
            {
                return _fontMediumSize;
            }
            set
            {
                _fontMediumSize = value;
            }
        }

        /// <summary>
        /// The size of the font large.
        /// </summary>
        private string _fontLargeSize = "15";
        public string FontLargeSize
        {
            get
            {
                return _fontLargeSize;
            }
            set
            {
                _fontLargeSize = value;
            }
        }

    ///// New Design
    ///
    private SolidColorBrush _newStackPanelLogoColor = "#3E436E".ToSolidColorBrush();
    public SolidColorBrush NewStackPanelLogoColor
    {
        get { return _newStackPanelLogoColor; }
        set { _newStackPanelLogoColor = value; }
    }

    private SolidColorBrush _newTextColor = "#4A4C55".ToSolidColorBrush();
    public SolidColorBrush NewTextColor
    {
        get { return _newTextColor; }
        set { _newTextColor = value; }
    }

    private SolidColorBrush _newBlueTextColor = "#4461D7".ToSolidColorBrush();
    public SolidColorBrush NewBlueTextColor
    {
        get { return _newBlueTextColor; }
        set { _newBlueTextColor = value; }
    }

}
}
