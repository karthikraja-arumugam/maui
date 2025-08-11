using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class NavigationStackReplacementTests : BaseTestFixture
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task NavigationStackReplacementShouldNotCallOnAppearingTwice(bool useMaui)
		{
			// Arrange
			var mainPage = new PageLifeCycleTests.LCPage { Title = "MainPage" };
			var deviceSelectionPage = new PageLifeCycleTests.LCPage { Title = "DeviceSelectionPage" };
			var settingsPage = new PageLifeCycleTests.LCPage { Title = "SettingsPage" };
			
			var navigationPage = new TestNavigationPage(useMaui, mainPage);
			_ = new TestWindow(navigationPage);
			
			// Step 1: Navigate from MainPage to DeviceSelectionPage
			await navigationPage.PushAsync(deviceSelectionPage);
			
			Assert.Equal(1, deviceSelectionPage.AppearingCount);
			Assert.Equal(0, deviceSelectionPage.DisappearingCount);
			
			// Step 2: Replace navigation stack by inserting SettingsPage before DeviceSelectionPage and then popping DeviceSelectionPage
			// This simulates "replacing the navigation stack"
			navigationPage.Navigation.InsertPageBefore(settingsPage, deviceSelectionPage);
			await navigationPage.PopAsync(false); // Remove deviceSelectionPage
			
			// Verify that DeviceSelectionPage disappearing was called
			Assert.Equal(1, deviceSelectionPage.DisappearingCount);
			Assert.Equal(1, settingsPage.AppearingCount);
			
			// Clear counters for the problematic step
			var initialDeviceSelectionAppearingCount = deviceSelectionPage.AppearingCount;
			
			// Step 3: Navigate back to MainPage - this should NOT call OnAppearing on DeviceSelectionPage
			await navigationPage.PopToRootAsync();
			
			// Verify that DeviceSelectionPage OnAppearing was NOT called again
			Assert.Equal(initialDeviceSelectionAppearingCount, deviceSelectionPage.AppearingCount);
			Assert.Equal(2, mainPage.AppearingCount); // Should appear again when returning from settings
		}
		
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task RemovingPageFromStackShouldCallDisappearing(bool useMaui)
		{
			// Arrange
			var rootPage = new PageLifeCycleTests.LCPage { Title = "RootPage" };
			var middlePage = new PageLifeCycleTests.LCPage { Title = "MiddlePage" };
			var topPage = new PageLifeCycleTests.LCPage { Title = "TopPage" };
			
			var navigationPage = new TestNavigationPage(useMaui, rootPage);
			_ = new TestWindow(navigationPage);
			
			// Push two pages
			await navigationPage.PushAsync(middlePage);
			await navigationPage.PushAsync(topPage);
			
			Assert.Equal(1, middlePage.AppearingCount);
			Assert.Equal(1, topPage.AppearingCount);
			
			// Remove middle page from stack
			navigationPage.Navigation.RemovePage(middlePage);
			await navigationPage.NavigatingTask;
			
			// Middle page should have disappearing called when removed from stack
			// even though it wasn't the current page
			Assert.Equal(1, middlePage.DisappearingCount);
		}
	}
}