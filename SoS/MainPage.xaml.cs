using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SoS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private const int LED_PIN = 5;

        private GpioPin pin;
        private GpioPinValue pinValue;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
        private int dotPeriod = 1;
        public MainPage()
        {
            this.InitializeComponent();

            DelayText.Text = dotPeriod.ToString() + " ms";

            InitGPIO();
        }
        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();
            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                pin = null;
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }
            pin = gpio.OpenPin(LED_PIN);
            pinValue = GpioPinValue.High;
            pin.SetDriveMode(GpioPinDriveMode.Output);
            pin.Write(pinValue);
            GpioStatus.Text = "GPIO pin initialized correctly.";
        }
        private void TurnLedOn()
        {
            pinValue = GpioPinValue.Low;
            pin.Write(pinValue);
        }
        private void TurnLedOff()
        {
            pinValue = GpioPinValue.High;
            pin.Write(pinValue);
        }
        private void EndSequence()
        {
            LED.Fill = grayBrush;
            TurnLedOff();
        }
        private async void SendSequenceAsync()
        {
            //Send "S", 3 dots
            await SendCharAsync(dotPeriod, 1);

            //INTER Character Gap
            await SendInterGapAsync(dotPeriod);

            //Send "O", 3 dashes (3*dotPeriod)
            await SendCharAsync(dotPeriod, 3);

            //INTER Character Gap
            await SendInterGapAsync(dotPeriod);

            //Send "S"
            await SendCharAsync(dotPeriod, 1);

            EndSequence();
        }
        private async Task SendCharAsync(int _dotInterval, int _multiplier)
        {
            for(int i = 0; i < 3; i++)
            { 
                // S = . . . 
                LED.Fill = redBrush;
                TurnLedOn();
                await Task.Delay(_dotInterval * _multiplier);
                //INTRA character gap
                if(i < 2) //On the 3rd time through we don't need this as the INTER character gap will take over
                { 
                    LED.Fill = grayBrush;
                    TurnLedOff();
                    await Task.Delay(_dotInterval);
                }
            }
        }
        private async Task SendInterGapAsync(int _dotInterval)
        {
            LED.Fill = grayBrush;
            TurnLedOff();
            await Task.Delay(_dotInterval*3*3); //3*dotInterval is a dash. Inter character gap is 3*Dash, thus 3*3*dotInterval
        }
        private void start_Click(object sender, RoutedEventArgs e)
        {
            SendSequenceAsync();
        }

    }

}
