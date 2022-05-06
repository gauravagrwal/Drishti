using Drishti.Services;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Drishti
{
    public partial class MainPage : ContentPage
    {
        public MediaFile image;
        public ComputerVisionService computervisioncreds;
        public FaceAPIService faceapicreds;

        public MainPage()
        {
            InitializeComponent();

            computervisioncreds = new ComputerVisionService();
            faceapicreds = new FaceAPIService();
        }

        private async void CameraButton_Clicked(object sender, EventArgs e)
        {
            Caption.Text = string.Empty;

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
            ComputerVisionButton.IsVisible = true;
            FaceAPIButton.IsVisible = true;
        }

        private async void GalleryButton_Clicked(object sender, EventArgs e)
        {
            Caption.Text = string.Empty;

            await CrossMedia.Current.Initialize();

            image = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
            {
                PhotoSize = PhotoSize.Full
            });

            if (image == null)
                return;

            Image.Source = ImageSource.FromStream(() => image.GetStream());
            ComputerVisionButton.IsVisible = true;
            FaceAPIButton.IsVisible = true;
        }

        private async void ComputerVisionButton_Clicked(object sender, EventArgs e)
        {
            if (Image.Source == null)
            {
                await DisplayAlert("Error", "Image not found.", "Try Again");
            }
            else
            {
                try
                {
                    var client = new ComputerVisionClient(
                        new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials(computervisioncreds.Key))
                    { Endpoint = computervisioncreds.Endpoint };

                    string cv = await DisplayActionSheet("Choose Option", "Cancel", null, "Try OCR", "Try Image Analysis");

                    switch (cv)
                    {
                        case "Cancel":
                            return;
                        case "Try OCR":
                            await RunOCR(client);
                            break;
                        case "Try Image Analysis":
                            await RunImageAnalysis(client);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Info", ex.Message, "Ok");
                }
            }
        }

        private async Task RunOCR(ComputerVisionClient client)
        {
            var ocrResult = await client.RecognizePrintedTextInStreamAsync(
                false,
                image.GetStream(),
                OcrLanguages.En);

            var extractedText = string.Empty;

            foreach (var region in ocrResult.Regions)
            {
                foreach (var line in region.Lines)
                {
                    foreach (var words in line.Words)
                    {
                        extractedText += words.Text + "\t";
                    }
                }
            }

            await DisplayAlert("Result", extractedText, "Ok");
        }

        private async Task RunImageAnalysis(ComputerVisionClient client)
        {
            var result = await client.DescribeImageInStreamAsync(image.GetStream());
            string caption = result.Captions.FirstOrDefault().Text;

            Caption.Text = caption;
        }

        private async void FaceAPIButton_Clicked(object sender, EventArgs e)
        {
            if (Image.Source == null)
            {
                await DisplayAlert("Info", "Missing Image", "Ok");
            }
            else
            {
                try
                {
                    var client = new FaceClient(
                        new Microsoft.Azure.CognitiveServices.Vision.Face.ApiKeyServiceClientCredentials(faceapicreds.Key))
                    { Endpoint = faceapicreds.Endpoint };

                    var responseList = await client.Face.DetectWithStreamAsync(
                        image.GetStream(),
                        returnFaceAttributes: new List<FaceAttributeType> {
                            FaceAttributeType.Age,
                            FaceAttributeType.Emotion,
                            FaceAttributeType.Gender
                        });

                    var face = responseList.FirstOrDefault();

                    double? age = face.FaceAttributes.Age;
                    string emotion = FindPredominantEmotion(face);
                    Microsoft.Azure.CognitiveServices.Vision.Face.Models.Gender? gender = face.FaceAttributes.Gender;

                    string result = $"- Age: " + age + "\n- Emotion: " + emotion + "\n- Gender: " + gender;

                    await DisplayAlert("Result", result, "Ok");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Info", ex.Message, "Ok");
                }
            }
        }

        public string FindPredominantEmotion(DetectedFace face)
        {
            double max = 0;
            PropertyInfo prop = null;

            var Emotions = typeof(Emotion).GetProperties();
            
            foreach (var emotion in Emotions)
            {
                double value = (double)emotion.GetValue(face.FaceAttributes.Emotion);

                if (value > max)
                {
                    max = value;
                    prop = emotion;
                }
            }

            return prop.Name.ToString();
        }
    }
}
