using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Drishti.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private ImageSource _image;
        public ImageSource Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        public ICommand CapturePhotoCommand { get; }
        public ICommand PickPhotoCommand { get; }

        public MainViewModel()
        {
            PickPhotoCommand = new Command(PickPhoto, () => CrossMedia.Current.IsPickPhotoSupported);
            CapturePhotoCommand = new Command(CapturePhoto, () => CrossMedia.Current.IsTakePhotoSupported);
        }

        private async void CapturePhoto()
        {
            try
            {
                var photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    DefaultCamera = CameraDevice.Front,
                    PhotoSize = PhotoSize.Medium,
                    SaveToAlbum = false
                });

                await LoadPhoto(photo);
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Console.WriteLine($"TakePhotoAsync THREW: {fnsEx.Message}");
            }
            catch (PermissionException pEx)
            {
                Console.WriteLine($"TakePhotoAsync THREW: {pEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TakePhotoAsync THREW: {ex.Message}");
            }
        }

        private async void PickPhoto()
        {
            try
            {
                MediaFile photo = await CrossMedia.Current.PickPhotoAsync();
                await LoadPhoto(photo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PickPhotoAsync THREW: {ex.Message}");
            }
        }

        private async Task LoadPhoto(MediaFile photo)
        {
            // cancelled
            if (photo == null)
            {
                return;
            }

            Image = ImageSource.FromStream(() => photo.GetStream());
        }
    }
}
