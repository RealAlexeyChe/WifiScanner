using System;
using System.Diagnostics;
using System.Threading;
using System.Device.Wifi;
using System.Collections;
using System.Device.Gpio;
using nanoFramework.Hardware.Esp32;
namespace WifiScanner
{
    public class Program
    {
        public static string MySsid = "Galaxy A51 2122"; // Успанавливаем SSID, который будет открывать замок
        public static GpioPin red; // Пин красного светодиода
        public static GpioPin green; // Пин зелёного светодиода
        public static GpioPin currentLed; // Текущий светодиод
        public static bool toggle; // Надо ли мигать
        public static Thread blink; // Поток моргания
        private static GpioController GpioController { get; } = new GpioController();
        public static void Main()
        {


            var GpioController = new GpioController();

            
            // Перевод пинов светодиода в режим вывода
            red = GpioController.OpenPin(21, PinMode.Output); 
            green = GpioController.OpenPin(19, PinMode.Output);

            currentLed = green;

            
            WifiAdapter wifiAdapter = WifiAdapter.FindAllAdapters()[0]; // Из списка всех адаптеров выбрать первый
            wifiAdapter.AvailableNetworksChanged += OnNetworksChanged; // 




            // Запускаем поток обновления списка сетей
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(10_000); // Ждём 10 секунд
                    Console.WriteLine("\n\n\nscanning");
                    wifiAdapter.ScanAsync(); // Сканируем сети
                     
                }
            }).Start();

            // Запускаем цикл моргания светодиодом
            blink = new Thread(() =>
            {
                while (true)
                {
                    if (toggle) currentLed.Toggle(); // Переключить светодиод, если нам нужно им мигать
                    Thread.Sleep(2_000); // Ждём 2 секунды
                }
            });
            blink.Start();

            // Заканчивает инициализацию и остонавливаем основной поток 
            Thread.Sleep(Timeout.Infinite);

            
        }

        // Меняем цвет на красный
        private static void ChangeLedToRed()
        {
            blink.Suspend(); // Останавливаем     

            currentLed.Write(PinValue.Low);
            currentLed = red;
            toggle = true;
            currentLed.Write(PinValue.High);
            blink.Resume();
            
        }

        // Меняем цвет на зелёный
        private static void ChangeLedToGreen()
        {
            blink.Suspend(); 
            currentLed.Write(PinValue.Low);
            currentLed = green;
            toggle = false;
            currentLed.Write(PinValue.High);
            blink.Resume();

        }

        private static bool IsNewtorkFound(WifiAvailableNetwork[] networks)
        {

            Console.WriteLine("///Available networks: ");
            foreach (var item in networks) // Для всех найденных сетей
            {
                Console.WriteLine(item.Ssid);
                if (item.Ssid == MySsid) // Если имя совпадает
                {
                    Console.WriteLine(item.Ssid);
                    Console.WriteLine("match!");
                    ChangeLedToGreen(); // Переключить цвет светодиода на зелёный
                    return true;
                }
                
            }
            Console.WriteLine("///");
            Console.WriteLine("no match");
            ChangeLedToRed(); // Если нет, переключить на красный
            return false;
        }

        private static void OnNetworksChanged(WifiAdapter sender, object e) // При изменении списка досупных сетей
        {

            WifiNetworkReport report = sender.NetworkReport;
            IsNewtorkFound(report.AvailableNetworks);  // Проверяем если название сети совпадает с нужным
        }
    }
}
