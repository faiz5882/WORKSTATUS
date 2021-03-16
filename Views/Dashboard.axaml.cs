using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using WorkStatus.Models;
using WorkStatus.ViewModels;

namespace WorkStatus.Views
{
    public class Dashboard : Window
    {
        public DashboardViewModel _dashboardVM;
        public Dashboard()
        {
            InitializeComponent();
            _dashboardVM = new DashboardViewModel();
            this.DataContext = _dashboardVM;
           // var dg1 = this.FindControl<DataGrid>("dataGrid1");
          //  dg1.IsReadOnly = true;
          //  var collectionView1 = new DataGridCollectionView(Countries.All);
            //dg1.Items = collectionView1;
#if DEBUG
            this.AttachDevTools();
#endif
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
