#nullable enable
using System.IO;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 9, "Android: Updating Image.Source with ImageSource.FromStream() is broken in Release builds", PlatformAffected.Android)]
public partial class IssueStreamImageReleaseMode : ContentPage
{
	private int _imageCounter = 0;
	
	// Create test image data - simple colored squares
	private static readonly byte[] RedImageData = Convert.FromBase64String(
		"iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAYAAAAeP4ixAAAABmJLR0QA/wD/AP+gvaeTAAAAA0lEQVRo3mNgGJUMhgwAAEgAAQgKXn1gAAAAAElFTkSuQmCC");
	private static readonly byte[] GreenImageData = Convert.FromBase64String(
		"iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAYAAAAeP4ixAAAABmJLR0QA/wD/AP+gvaeTAAAAA0lEQVRo3mNgGJX8hwwAAEgAAQihz/VwAAAAAElFTkSuQmCC");
	private static readonly byte[] BlueImageData = Convert.FromBase64String(
		"iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAYAAAAeP4ixAAAABmJLR0QA/wD/AP+gvaeTAAAAA0lEQVRo3mNgGJX8hwQAAEgAAQihz/VwAAAAAElFTkSuQmCC");

	public IssueStreamImageReleaseMode()
	{
		InitializeComponent();
		LoadInitialImage();
	}

	private void LoadInitialImage()
	{
		try
		{
			// Load initial red image
			var stream = new MemoryStream(RedImageData);
			testImage.Source = ImageSource.FromStream(() => stream);
			statusLabel.Text = "Initial image loaded (Red)";
		}
		catch (Exception ex)
		{
			statusLabel.Text = $"Error loading initial image: {ex.Message}";
		}
	}

	private void OnUpdateImageClicked(object sender, EventArgs e)
	{
		try
		{
			_imageCounter++;
			var imageData = (_imageCounter % 3) switch
			{
				0 => RedImageData,
				1 => GreenImageData,
				_ => BlueImageData
			};

			var color = (_imageCounter % 3) switch
			{
				0 => "Red",
				1 => "Green", 
				_ => "Blue"
			};

			// Create a new stream each time to simulate real-world usage
			var stream = new MemoryStream(imageData);
			testImage.Source = ImageSource.FromStream(() => stream);
			
			statusLabel.Text = $"Image updated to {color} (#{_imageCounter})";
		}
		catch (Exception ex)
		{
			statusLabel.Text = $"Error updating image: {ex.Message}";
		}
	}
}