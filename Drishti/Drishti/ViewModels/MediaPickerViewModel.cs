using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Drishti.ViewModels
{
    public class MediaPickerViewModel : BaseViewModel
    {
        private string photoPath;
        private bool showPhoto;

        public string PhotoPath
        {
            get { return photoPath; }
            set { SetPropertyValue(ref photoPath, value); }
        }

        public bool ShowPhoto
        {
            get { return showPhoto; }
            set { SetPropertyValue(ref showPhoto, value); }
        }

        public ICommand PickPhotoCommand { get; }
        public ICommand CapturePhotoCommand { get; }


        public MediaPickerViewModel()
        {
            PickPhotoCommand = new Command(DoPickPhoto);
            CapturePhotoCommand = new Command(DoCapturePhoto, () => MediaPicker.IsCaptureSupported);
        }

        async void DoPickPhoto()
        {
            try
            {
                var photo = await MediaPicker.PickPhotoAsync();

                await LoadPhoto(photo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PickPhotoAsync THREW: {ex.Message}");
            }
        }

        async void DoCapturePhoto()
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();

                await LoadPhoto(photo);
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {fnsEx.Message}");
            }
            catch (PermissionException pEx)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {pEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {ex.Message}");
            }
        }

        async Task LoadPhoto(FileResult photo)
        {
            // cancelled
            if (photo == null)
            {
                PhotoPath = null;
                return;
            }

            // save the file into local storage
            var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
            {
                await stream.CopyToAsync(newStream);
            }

            PhotoPath = newFile;
            ShowPhoto = true;
        }
    }
}
