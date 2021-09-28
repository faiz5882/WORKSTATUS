using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using System;
using WorkStatus.Utility;
using WorkStatus.ViewModels;

namespace WorkStatus.Views
{
    public partial class AddOrEditToDo : Window
    {
        public AddOrEditToDoViewModel _addOrEditToDoVM;
        public static AddOrEditToDo AddorEditToDoInstance { get; private set; }

        public AddOrEditToDo()
        {
            InitializeComponent();
            try
            {
                _addOrEditToDoVM = new AddOrEditToDoViewModel(this);
                this.DataContext = _addOrEditToDoVM;
                AddorEditToDoInstance = this;
                var cstartdate = this.FindControl<CalendarDatePicker>("cdpStartdate");
                cstartdate.DisplayDateStart = DateTime.Today;
                cstartdate.SelectedDateFormat = CalendarDatePickerFormat.Custom;
                cstartdate.CustomDateFormatString = "yyyy-MM-dd";
                cstartdate.SelectedDateChanged += Cstartdate_SelectedDateChanged;
                var cenddate = this.FindControl<CalendarDatePicker>("cdpEnddate");
                cenddate.DisplayDateStart = DateTime.Today;
                cenddate.CustomDateFormatString = "yyyy-MM-dd";
                cenddate.SelectedDateChanged += Cenddate_SelectedDateChanged;
                if (Common.Storage.EditToDoId != 0 && !string.IsNullOrEmpty(Convert.ToString(Common.Storage.EdittodoData.startDate)))
                {
                    _addOrEditToDoVM.StartDateAddOrEditTodo = Common.Storage.EdittodoData.startDate;
                    _addOrEditToDoVM.EndDateAddOrEditTodo = Common.Storage.EdittodoData.endDate;
                }
#if DEBUG
                this.AttachDevTools();
#endif
            }
            catch (Exception ex)
            {

            }
        }
        private void Cstartdate_SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            try
            {
                var v = this.FindControl<CalendarDatePicker>("cdpStartdate").SelectedDate;
                if (v != null)
                {
                    DateTime d = (DateTime)v;
                    var m = d.ToString("yyyy-MM-dd");
                    _addOrEditToDoVM.StartDateAddOrEditTodo = m;
                }
                else
                {
                    _addOrEditToDoVM.StartDateAddOrEditTodo = "Start Date";
                }
            }

            catch (Exception ex)
            {

            }

        }
        private void Cenddate_SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            var v = this.FindControl<CalendarDatePicker>("cdpEnddate").SelectedDate;
            if (v != null)
            {
                DateTime d = (DateTime)v;
                var m = d.ToString("yyyy-MM-dd");
                _addOrEditToDoVM.EndDateAddOrEditTodo = m;
            }
            else
            {
                _addOrEditToDoVM.EndDateAddOrEditTodo = "End Date";
            }
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
