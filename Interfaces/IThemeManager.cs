using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Interfaces
{
    public interface IThemeManager
    {
        #region LoginView
        SolidColorBrush StackPanelLogoColor { get; set; }
        SolidColorBrush TxtWelcomeColor { get; set; }
        #endregion
        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>The color of background.</value>


        /// <summary>
        /// Gets or sets the color of the Text Box background.
        /// </summary>
        /// <value>The color of the Text Box background.</value>
        Color TextBoxBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the Text Box TextColor.
        /// </summary>
        /// <value>The color of the Text Box TextColor.</value>
        Color TextBoxTextColor { get; set; }

        /// <summary>
        /// Gets or sets the size of the font normal.
        /// </summary>
        /// <value>The size of the font normal.</value>
        string FontNormalSize { get; set; }

        /// <summary>
        /// Gets or sets the size of the font small.
        /// </summary>
        /// <value>The size of the font small.</value>
        string FontSmallSize { get; set; }

        /// <summary>
        /// Gets or sets the size of the font medium.
        /// </summary>
        /// <value>The size of the font medium.</value>
        string FontMediumSize { get; set; }

        /// <summary>
        /// Gets or sets the size of the font large.
        /// </summary>
        /// <value>The size of the font large.</value>
        string FontLargeSize { get; set; }

    }
}
