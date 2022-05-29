//using Maui.DogClassifier.Core;

using Maui.DogClassifier.Core;

namespace Maui.DogClassifier;

public partial class MainPage : ContentPage
{
	int count = 0;
	Classifier classifier;

	public MainPage()
	{
		InitializeComponent();
		classifier = new Classifier();

    }

    private async void BtnCapture_Clicked(object sender, EventArgs e)
    {
        var photo = await MediaPicker.CapturePhotoAsync();

        await LoadPhotoAsync(photo);

        byte[] image = File.ReadAllBytes(photo.FullPath);

        var result = classifier.ClassifyImage(image);

        lblResult.IsVisible = true;

        if (result != null)
        {
            if (result.Percentage > 0.1)
            {                
                lblResult.Text = $"It's a {result.Name}!";
                return;
            }
        }

        lblResult.Text = "Dog not recognized, please try again";
    }

    async Task LoadPhotoAsync(FileResult photo)
    {
        // canceled
        //if (photo == null)
        //{
        //    imgCapture.IsVisible = false;
        //    return;
        //}

        // save the file into local storage
        var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
        using (var stream = await photo.OpenReadAsync())
        using (var newStream = File.OpenWrite(newFile))
        {
            await stream.CopyToAsync(newStream);
        }

        imgCapture.Source = newFile;
        imgCapture.Aspect = Aspect.Center;
    }
}

