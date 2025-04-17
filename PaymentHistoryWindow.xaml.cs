using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Net.Http;
using System.Xml;
using System.Globalization;
namespace SubTargetting
{
    /// <summary>
    /// Логика взаимодействия для PaymentHistoryWindow.xaml
    /// </summary>
    public partial class PaymentHistoryWindow : Window
    {
        AppContext db;
        string value = EnterCur.enterCurrency;

        private static readonly HttpClient httpClient = new HttpClient();


        public PaymentHistoryWindow()
        {
            InitializeComponent();
            db = new AppContext();
            Loaded += OnWindowLoaded;
        }


        private async Task<Dictionary<string, decimal>> GetCurrencyRatesAsync()
        {
            string url = "https://www.cbr.ru/scripts/XML_daily.asp";
            var currencyRates = new Dictionary<string, decimal>();
            string[] currencies = { "USD", "EUR", "CNY" };

            try
            {
                string response = await httpClient.GetStringAsync(url);

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);
                foreach (var currencyCode in currencies)
                {
                    XmlNode node = xmlDoc.SelectSingleNode($"//Valute[CharCode='{currencyCode}']/Value");
                    if (node != null)
                    {
                        try
                        {
                            decimal rate = decimal.Parse(node.InnerText.Replace(',', '.'), CultureInfo.InvariantCulture);
                            currencyRates[currencyCode] = rate;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при парсинге курса валюты {currencyCode}: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Курс валюты {currencyCode} не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении данных от API: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return currencyRates;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var currencyRates = await GetCurrencyRatesAsync();
                var subs = db.Subs.ToList();

                var result = subs.Select(sub =>
                {
                    decimal exchangeRate = 1;

                    if (sub.PayType.ToUpper() == "USD")
                    {
                        exchangeRate = currencyRates.ContainsKey("USD") ? currencyRates["USD"] : 1;

                    }
                    else if (sub.PayType.ToUpper() == "EUR")
                    {
                        exchangeRate = currencyRates.ContainsKey("EUR") ? currencyRates["EUR"] : 1;

                    }
                    else if (sub.PayType.ToUpper() == "CNY")
                    {
                        exchangeRate = currencyRates.ContainsKey("CNY") ? currencyRates["CNY"] : 1;

                    }

                    float payment;
                    if (sub.Payment is float floatPayment)
                    {
                        payment = floatPayment;
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка: Невозможно преобразовать Payment '{sub.Payment}' в число.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }

                    decimal paymentInRubles = (decimal)payment * exchangeRate;

                    string paymentInMainCurS;
                    if (value.ToUpper() == "RUB")
                    {
                        // Если целевая валюта — рубли, оставляем значение без изменений
                        paymentInMainCurS = Convert.ToString(Math.Round(paymentInRubles, 2)) + " ₽";
                    }
                    else if (value.ToUpper() == "EUR")
                    {
                        if (currencyRates.ContainsKey("EUR"))
                        {
                            decimal targetExchangeRate = currencyRates["EUR"];
                            paymentInMainCurS = Convert.ToString(Math.Round(paymentInRubles / targetExchangeRate, 2)) + " €";
                        }
                        else
                        {
                            MessageBox.Show($"Курс валюты EUR не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return null;
                        }
                    }
                    else if (value.ToUpper() == "USD")
                    {
                        if (currencyRates.ContainsKey("USD"))
                        {
                            decimal targetExchangeRate = currencyRates["USD"];
                            paymentInMainCurS = Convert.ToString(Math.Round(paymentInRubles / targetExchangeRate, 2)) + " $";
                        }
                        else
                        {
                            MessageBox.Show($"Курс валюты USD не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return null;
                        }
                    }
                    else if (value.ToUpper() == "CNY")
                    {
                        if (currencyRates.ContainsKey("CNY"))
                        {
                            decimal targetExchangeRate = currencyRates["CNY"];
                            paymentInMainCurS = Convert.ToString(Math.Round(paymentInRubles / targetExchangeRate, 2)) + " ¥";
                        }
                        else
                        {
                            MessageBox.Show($"Курс валюты CNY не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return null;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Курс валюты {value} не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null; // Пропускаем объект с некорректной валютой
                    }
                    return new SubWithRubles
                    {
                        Name = sub.SubName,
                        Type = sub.SubType,
                        End = sub.EndDate.ToString(),
                        Pay = payment,
                        PType = sub.PayType,
                        MainCurrency = paymentInMainCurS
                    };
                }).Where(item => item != null).ToList();

                PayHis.ItemsSource = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        public class SubWithRubles
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string End { get; set; }
            public float Pay { get; set; }
            public string PType { get; set; }
            public string MainCurrency { get; set; }
        }

        private void MinButton2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ExitButton2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ReturnButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow returnwin = new MainWindow();
            returnwin.Show();
            this.Close();
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
