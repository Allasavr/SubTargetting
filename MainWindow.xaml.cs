using Microsoft.Win32
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SubTargetting
{
    /// <summary> 
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AppContext db;

        public MainWindow()
        {
            InitializeComponent();
            db= new AppContext();
            // Загружаем данные из реестра
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\MyApp\Notifications");
            HashSet<int> notifiedSubIds = new HashSet<int>();
            
            foreach (string valueName in registryKey.GetValueNames())
            {
                int subId = Convert.ToInt32(valueName);
                notifiedSubIds.Add(subId);
            }
            List<Sub> subs = db.Subs.ToList();
            DateTime today = DateTime.Today;
            
            foreach (Sub sub in subs)
            {
                DateTime remindDate;
                if (DateTime.TryParseExact(sub.EndDate, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out remindDate))
                {
                    if (remindDate.Date == today.AddDays(1))
                    {
                        // Напоминание за день до окончания подписки
                        MessageBox.Show($"Ваша подписка на {sub.SubName} кончится уже завтра!");
                    }
                    else if (remindDate.Date < today && !notifiedSubIds.Contains(sub.id))
                    {
                        // Уведомление о просроченной подписке, если оно еще не отправлено
                        MessageBox.Show($"Ваша подписка на {sub.SubName} уже закончилась!");
            
                        // Добавляем ID подписки в реестр
                        registryKey.SetValue(sub.id.ToString(), 1);
            
                        // Обновляем список показанных уведомлений
                        notifiedSubIds.Add(sub.id);
                    }
                }
            }
            registryKey.Close();
        }
        
        private void MyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DataTextBox.Text == "YYYY-MM-DD")
            {
                DataTextBox.Text = "";
                DataTextBox.Foreground = new SolidColorBrush(Colors.Black); 
            }
        }

        private void MyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DataTextBox.Text))
            {
                DataTextBox.Text = "YYYY-MM-DD";
                DataTextBox.Foreground = new SolidColorBrush(Colors.LightGray); 
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            AddButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFC4E8F4");
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            AddButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF95E0F9");
        }
        
        private string CalculateData(DateTime date, string s)
        {
            DateTime currentData = date;

            switch (s)
            {
                case "30 дней":
                    DateTime futuredata1 = currentData.AddDays(30);
                    string result1 = futuredata1.ToString("yyyy-MM-dd");
                    return result1;
                case "2 месяца":
                    DateTime futuredata2 = currentData.AddMonths(2);
                    string result2 = futuredata2.ToString("yyyy-MM-dd");
                    return result2;
                case "6 месяцев":
                    DateTime futuredata3 = currentData.AddMonths(6);
                    string result3 = futuredata3.ToString("yyyy-MM-dd");
                    return result3;
                case "1 год":
                    DateTime futuredata4 = currentData.AddYears(1);
                    string result4 = futuredata4.ToString("yyyy-MM-dd");
                    return result4;
                default: string result = currentData.ToString("yyyy-MM-dd");
                    return result;

            }
        }
        private void Check(string model, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(model) || model == null)
            {
                MessageBox.Show($"Недостаёт {fieldName}!");
                return;
            }
        }
        private bool Check_GroupBox(string model, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                MessageBox.Show($"Недостаёт {fieldName}!");
                return false; // Проверка не прошла
            }
            return true; // Проверка прошла
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedCategory = GetSelectedRadioButtonContent(CategoryGroupBox);
            if (!Check_GroupBox(selectedCategory, "категории подписки")) return;

            string selectedDuration = GetSelectedRadioButtonContent(DurationGroupBox);
            if (!Check_GroupBox(selectedDuration, "длительности подписки")) return;

            string selectedCurrency = GetSelectedRadioButtonContent(CurrencyGroupBox);
            if (!Check_GroupBox(selectedCurrency, "валюты платежа")) return;


            string sub = SubTextBox.Text;
            Check(sub, "названия подписки");
            string data = DataTextBox.Text;
            Check(data, "даты подписки");
            DateTime tryParse;

            if(!DateTime.TryParse(data,out tryParse))
            {
                MessageBox.Show("Введена некорректная дата\n Попробуйте ещё раз");
            }

            string payment = PaymentTextBox.Text;
            Check(payment, "суммы платежа");
            string endData=CalculateData(tryParse, selectedDuration);

            try
            {
                Sub subscript = new Sub(sub, selectedCategory, data, endData, 
                    float.Parse(payment), selectedCurrency);

                db.Subs.Add(subscript);
                db.SaveChanges();
                MessageBox.Show("Подписка успешно добавлена!");
                SubTextBox.Clear();
                DataTextBox.Clear();
                PaymentTextBox.Clear();
                ResetRadioButtonContent(CategoryGroupBox);
                ResetRadioButtonContent(DurationGroupBox);
                ResetRadioButtonContent(CurrencyGroupBox);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при добавлении подписки: {ex.Message}");
            }

        }

        private string GetSelectedRadioButtonContent(GroupBox groupbox)
        {
            foreach (var child in ((StackPanel)groupbox.Content).Children)
            {
                if (child is RadioButton radioButton && radioButton.IsChecked == true)
                {
                    return radioButton.Content.ToString();
                }
            }
            return null;
        }

        private void ResetRadioButtonContent(GroupBox groupbox)
        {
            foreach (var child in ((StackPanel)groupbox.Content).Children)
            {
                if (child is RadioButton radioButton )
                {
                    radioButton.IsChecked=false;
                }
            }
        }

        private void ExitButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState=WindowState.Minimized;
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
               DragMove();
            }
        }

        private void PaymentHistory_Click(object sender, RoutedEventArgs e)
        {
            PaymentHistoryWindow paymenthistorywin = new PaymentHistoryWindow();
            paymenthistorywin.Show();
            Hide();
        }

        private void PaymentHistory_MouseLeave_1(object sender, MouseEventArgs e)
        {
            PaymentHistory.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF95E0F9");
        }

        private void PaymentHistory_MouseEnter_1(object sender, MouseEventArgs e)
        {
            PaymentHistory.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFC4E8F4");
        }

    }
}
