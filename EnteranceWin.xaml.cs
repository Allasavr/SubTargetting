using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SubTargetting
{
    /// <summary>
    /// Логика взаимодействия для EnteranceWin.xaml
    /// </summary>
    public partial class EnteranceWin : Window
    {
        public EnteranceWin()
        {
            InitializeComponent();
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            StartButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF95E0F9");
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            StartButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFC4E8F4");
        }

        private void MinButton1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState=WindowState.Minimized;
        }

        private void ExitButton1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string enterCurrency = "";
            foreach (var child in ((StackPanel)CurrencyStartGroupBox.Content).Children)
            {
                if (child is RadioButton radioButton && radioButton.IsChecked == true)
                {
                    enterCurrency = radioButton.Content.ToString().Trim();
                }
            }

            if (string.IsNullOrEmpty(enterCurrency))
            {
                MessageBox.Show("Выберите основную валюту!");
            }
            else
            {
                EnterCur.enterCurrency = enterCurrency;
                MainWindow mainw = new MainWindow();
                mainw.Show();
                Hide();
            }

        }

    }
}
