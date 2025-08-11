using System;
using System.Collections.Generic;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 7, "OnAppearing of Page called again, although this page was on already replaced NavigationStack")]
	public class IssueOnAppearingNavigationStack : TestContentPage
	{
		public static List<string> AppearingEvents = new List<string>();
		
		protected override void Init()
		{
			AppearingEvents.Clear();
			
			Title = "MainPage";
			
			var label = new Label
			{
				Text = "MainPage - Click button to start navigation test",
				AutomationId = "MainPageLabel"
			};
			
			var button = new Button
			{
				Text = "Navigate to Device Selection Page",
				AutomationId = "NavigateToDeviceSelection"
			};
			
			button.Clicked += async (sender, e) =>
			{
				await Navigation.PushAsync(new DeviceSelectionPage());
			};
			
			Content = new StackLayout
			{
				Children = { label, button }
			};
		}
		
		protected override void OnAppearing()
		{
			base.OnAppearing();
			AppearingEvents.Add("MainPage-OnAppearing");
		}
	}
	
	public class DeviceSelectionPage : ContentPage
	{
		public DeviceSelectionPage()
		{
			Title = "Select Device Page";
			
			var label = new Label
			{
				Text = "Device Selection Page - This should show alert once",
				AutomationId = "DeviceSelectionLabel"
			};
			
			var button = new Button
			{
				Text = "Replace Navigation Stack with Settings",
				AutomationId = "ReplaceWithSettings"
			};
			
			button.Clicked += async (sender, e) =>
			{
				// Replace the navigation stack - this is the key operation that causes the issue
				var settingsPage = new SettingsPage();
				Navigation.InsertPageBefore(settingsPage, this);
				await Navigation.PopAsync(false); // Remove this page
			};
			
			Content = new StackLayout
			{
				Children = { label, button }
			};
		}
		
		protected override void OnAppearing()
		{
			base.OnAppearing();
			IssueOnAppearingNavigationStack.AppearingEvents.Add("DeviceSelectionPage-OnAppearing");
			
			// Simulate the alert that should only show once
			DisplayAlert("Test", "DeviceSelectionPage OnAppearing called", "OK");
		}
		
		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			IssueOnAppearingNavigationStack.AppearingEvents.Add("DeviceSelectionPage-OnDisappearing");
		}
	}
	
	public class SettingsPage : ContentPage
	{
		public SettingsPage()
		{
			Title = "Settings Page";
			
			var label = new Label
			{
				Text = "Settings Page - Click to go back to MainPage",
				AutomationId = "SettingsLabel"
			};
			
			var button = new Button
			{
				Text = "Navigate back to MainPage",
				AutomationId = "NavigateToMainPage"
			};
			
			button.Clicked += async (sender, e) =>
			{
				await Navigation.PopToRootAsync();
			};
			
			Content = new StackLayout
			{
				Children = { label, button }
			};
		}
		
		protected override void OnAppearing()
		{
			base.OnAppearing();
			IssueOnAppearingNavigationStack.AppearingEvents.Add("SettingsPage-OnAppearing");
		}
		
		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			IssueOnAppearingNavigationStack.AppearingEvents.Add("SettingsPage-OnDisappearing");
		}
	}
}