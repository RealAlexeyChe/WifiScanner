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
        public static string MySsid = "Galaxy A51 2122"; // ������������� SSID, ������� ����� ��������� �����
        public static GpioPin red; // ��� �������� ����������
        public static GpioPin green; // ��� ������� ����������
        public static GpioPin currentLed; // ������� ���������
        public static bool toggle; // ���� �� ������
        public static Thread blink; // ����� ��������
        private static GpioController GpioController { get; } = new GpioController();
        public static void Main()
        {


            var GpioController = new GpioController();

            
            // ������� ����� ���������� � ����� ������
            red = GpioController.OpenPin(21, PinMode.Output); 
            green = GpioController.OpenPin(19, PinMode.Output);

            currentLed = green;

            
            WifiAdapter wifiAdapter = WifiAdapter.FindAllAdapters()[0]; // �� ������ ���� ��������� ������� ������
            wifiAdapter.AvailableNetworksChanged += OnNetworksChanged; // 




            // ��������� ����� ���������� ������ �����
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(10_000); // ��� 10 ������
                    Console.WriteLine("\n\n\nscanning");
                    wifiAdapter.ScanAsync(); // ��������� ����
                     
                }
            }).Start();

            // ��������� ���� �������� �����������
            blink = new Thread(() =>
            {
                while (true)
                {
                    if (toggle) currentLed.Toggle(); // ����������� ���������, ���� ��� ����� �� ������
                    Thread.Sleep(2_000); // ��� 2 �������
                }
            });
            blink.Start();

            // ����������� ������������� � ������������� �������� ����� 
            Thread.Sleep(Timeout.Infinite);

            
        }

        // ������ ���� �� �������
        private static void ChangeLedToRed()
        {
            blink.Suspend(); // �������������     

            currentLed.Write(PinValue.Low);
            currentLed = red;
            toggle = true;
            currentLed.Write(PinValue.High);
            blink.Resume();
            
        }

        // ������ ���� �� ������
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
            foreach (var item in networks) // ��� ���� ��������� �����
            {
                Console.WriteLine(item.Ssid);
                if (item.Ssid == MySsid) // ���� ��� ���������
                {
                    Console.WriteLine(item.Ssid);
                    Console.WriteLine("match!");
                    ChangeLedToGreen(); // ����������� ���� ���������� �� ������
                    return true;
                }
                
            }
            Console.WriteLine("///");
            Console.WriteLine("no match");
            ChangeLedToRed(); // ���� ���, ����������� �� �������
            return false;
        }

        private static void OnNetworksChanged(WifiAdapter sender, object e) // ��� ��������� ������ �������� �����
        {

            WifiNetworkReport report = sender.NetworkReport;
            IsNewtorkFound(report.AvailableNetworks);  // ��������� ���� �������� ���� ��������� � ������
        }
    }
}
