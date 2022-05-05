using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using Xamarin.Forms;

namespace Drishti
{
    public partial class MainPage : ContentPage
    {
        public MediaFile image;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void CameraButton_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            image = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                DefaultCamera = CameraDevice.Front,
                PhotoSize = PhotoSize.Full,
                SaveToAlbum = false
            });

            if (image == null)
                return;

            Image.Source = ImageSource.FromStream(() => image.GetStream());
        }

        private async void GalleryButton_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            image = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
            {
                PhotoSize = PhotoSize.Full
            });

            if (image == null)
                return;

            Image.Source = ImageSource.FromStream(() => image.GetStream());
        }
    }
}
