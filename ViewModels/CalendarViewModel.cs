using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using MealPrepHelper.Data;

namespace MealPrepHelper.ViewModels
{
    public class CalendarViewModel : ViewModelBase
    {
        // --- EXISTUJÍCÍ KÓD ---
        private DateTime _currentMonth;
        private string _monthTitle = string.Empty;

        public string MonthTitle
        {
            get => _monthTitle;
            set => this.RaiseAndSetIfChanged(ref _monthTitle, value);
        }

        public ObservableCollection<DayViewModel> Days { get; } = new();

        public ReactiveCommand<Unit, Unit> NextMonthCommand { get; }
        public ReactiveCommand<Unit, Unit> PrevMonthCommand { get; }


        // --- NOVÉ: POPUP LOGIKA ---
        
        // Viditelnost okna
        private bool _isPopupVisible;
        public bool IsPopupVisible
        {
            get => _isPopupVisible;
            set => this.RaiseAndSetIfChanged(ref _isPopupVisible, value);
        }

        // Který den jsme vybrali (pro nadpis popupu)
        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => this.RaiseAndSetIfChanged(ref _selectedDate, value);
        }

        public ReactiveCommand<DayViewModel, Unit> OpenPopupCommand { get; }
        public ReactiveCommand<Unit, Unit> ClosePopupCommand { get; }


        public CalendarViewModel()
        {
            _currentMonth = DateTime.Today;

            NextMonthCommand = ReactiveCommand.Create(() => ChangeMonth(1));
            PrevMonthCommand = ReactiveCommand.Create(() => ChangeMonth(-1));

            // NOVÉ: Příkaz pro otevření (klik na den)
            OpenPopupCommand = ReactiveCommand.Create<DayViewModel>(day =>
            {
                SelectedDate = day.Date;
                IsPopupVisible = true; // Zobrazí popup
            });

            // NOVÉ: Příkaz pro zavření (křížek nebo klik mimo)
            ClosePopupCommand = ReactiveCommand.Create(() =>
            {
                IsPopupVisible = false; // Skryje popup
            });

            ReloadCalendar();
        }

        // ... Zbytek metod (ChangeMonth, ReloadCalendar) zůstává stejný ...
        private void ChangeMonth(int add)
        {
            _currentMonth = _currentMonth.AddMonths(add);
            ReloadCalendar();
        }

        public void ReloadCalendar()
        {
            // ... Váš existující kód pro načítání kalendáře ...
            // (Pro stručnost ho sem nekopíruji celý, nechte ho tak, jak je v minulé verzi)
            
            Days.Clear();
            MonthTitle = _currentMonth.ToString("MMMM yyyy");

            var firstDayOfMonth = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
            int dayOfWeek = (int)firstDayOfMonth.DayOfWeek; 
            int daysToSubtract = (dayOfWeek == 0) ? 6 : dayOfWeek - 1; 

            var startDate = firstDayOfMonth.AddDays(-daysToSubtract);
            var endDate = startDate.AddDays(42); 

            using (var db = new AppDbContext())
            {
                var busyDates = db.PlanItems
                    .Where(p => p.ScheduledFor >= startDate && p.ScheduledFor < endDate)
                    .Select(p => p.ScheduledFor.Date)
                    .Distinct()
                    .ToList();

                for (int i = 0; i < 42; i++)
                {
                    var date = startDate.AddDays(i);
                    var dayVm = new DayViewModel
                    {
                        Date = date,
                        IsCurrentMonth = date.Month == _currentMonth.Month,
                        HasPlan = busyDates.Contains(date.Date)
                    };
                    Days.Add(dayVm);
                }
            }
        }
    }

    // Pomocná třída (stejná jako minule)
    public class DayViewModel : ViewModelBase
    {
        public DateTime Date { get; set; }
        public string DayNumber => Date.Day.ToString();

        private bool _hasPlan;
        public bool HasPlan 
        { 
            get => _hasPlan; 
            set => this.RaiseAndSetIfChanged(ref _hasPlan, value); 
        }

        public bool IsCurrentMonth { get; set; }
        public string TextColor => IsCurrentMonth ? "Black" : "#CCCCCC";
        public string DotColor => "#4CAF50"; 
    }
}