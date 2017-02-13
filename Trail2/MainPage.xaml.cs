using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;


//Camera
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.UI.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Trail2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }
        //Webcam variables
        private MediaCapture MediaCap;
        private bool IsInPictureCaptureMode = true;
        //Censor variables
        GpioPin _LedPIN, _CensorPIN;
        GpioController _LedController, _CensorController;
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _LedController = GpioController.GetDefault();
            _CensorController = GpioController.GetDefault();
            _LedPIN = _LedController.OpenPin(4);
            _CensorPIN = _CensorController.OpenPin(26);
            _LedPIN.SetDriveMode(GpioPinDriveMode.Output);
            _CensorPIN.SetDriveMode(GpioPinDriveMode.Input);
            _CensorPIN.DebounceTimeout = new TimeSpan(0, 0, 0, 0, 500);
            InitilizeWebcam();

            _CensorPIN.ValueChanged += _CensorPIN_ValueChanged;
        }

        private async void _CensorPIN_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (_LedPIN.Read() == GpioPinValue.Low)
            {
                _LedPIN.Write(GpioPinValue.High);
                IsInPictureCaptureMode = false;

            }
            else
            {
                _LedPIN.Write(GpioPinValue.Low);

                await TakePicture();

            }

        }
        private async void InitilizeWebcam(object sender = null, RoutedEventArgs e = null)
        {

            //initialize the WebCam via MediaCapture object
            MediaCap = new MediaCapture();
            await MediaCap.InitializeAsync();

            //StorageFile picture = await TakePicture();
            //AppStatus.Text = "Camera initialized...Waiting for MOTION";

        }

        public async Task<StorageFile> TakePicture()
        {

            //captureImage is our Xaml image control (to preview the picture onscreen)
            CaptureImage.Source = null;

            //gets a reference to the file we're about to write a picture into
            StorageFile photoFile = await KnownFolders.CameraRoll.CreateFileAsync(
                "RaspPiSecurityPic.jpg", CreationCollisionOption.GenerateUniqueName);

            //use the MediaCapture object to stream captured photo to a file
            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
            await MediaCap.CapturePhotoToStorageFileAsync(imageProperties, photoFile);

            //show photo onscreen
            IRandomAccessStream photoStream = await photoFile.OpenReadAsync();
            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(photoStream);
            CaptureImage.Source = bitmap;

            //AppStatus.Text = "Took Photo: " + photoFile.Name;

            return photoFile;

        }

    }
}
