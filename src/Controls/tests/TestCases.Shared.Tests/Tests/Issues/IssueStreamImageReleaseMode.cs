using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class IssueStreamImageReleaseMode : _IssuesUITest
{
	public override string Issue => "Android: Updating Image.Source with ImageSource.FromStream() is broken in Release builds";

	public IssueStreamImageReleaseMode(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Image)]
	public void ImageSourceFromStreamShouldUpdateInReleaseBuild()
	{
		App.WaitForElement("TestInstructions");
		
		// Verify initial state
		App.WaitForElement("TestImage");
		App.WaitForElement("StatusLabel");
		
		// Initial status should show initial image loaded
		var initialStatus = App.FindElement("StatusLabel").GetText();
		Assert.That(initialStatus, Does.Contain("Initial image loaded"));
		
		// Click the update button multiple times to test image updating
		for (int i = 1; i <= 3; i++)
		{
			App.Click("UpdateImageButton");
			
			// Wait for status to update
			Thread.Sleep(500);
			
			// Verify the status shows the image was updated
			var updatedStatus = App.FindElement("StatusLabel").GetText();
			Assert.That(updatedStatus, Does.Contain($"(#{i})"), 
				$"Image update #{i} should be reflected in status");
			
			// Verify the status doesn't show an error
			Assert.That(updatedStatus, Does.Not.Contain("Error"), 
				$"No error should occur during image update #{i}");
		}
	}
}