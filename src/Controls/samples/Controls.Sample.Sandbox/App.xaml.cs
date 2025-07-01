namespace Maui.Controls.Sample;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// To test shell scenarios, change this to true
		bool useShell = false;
		Window window;
		if (!useShell)
		{
			window = new Window(new NavigationPage(new MainPage()));
		}
		else
		{
			window = new Window(new SandboxShell());
		}

#if ANDROID
		window.HandlerChanged += OnWindowHandlerChanged;
#endif
		return window;
	}

#if ANDROID
	private void OnWindowHandlerChanged(object? sender, EventArgs e)
	{
		if (sender is Window window && window.Handler?.PlatformView is Android.App.Activity activity)
		{
			var rootView = activity.FindViewById(Android.Resource.Id.Content);
			if (rootView != null)
			{
				AndroidX.Core.View.ViewCompat.SetOnApplyWindowInsetsListener(rootView, new WindowInsetsListener());
			}
	}
#endif
}

#if ANDROID
class WindowInsetsListener : Java.Lang.Object,  AndroidX.Core.View.IOnApplyWindowInsetsListener
{
	public AndroidX.Core.View.WindowInsetsCompat? OnApplyWindowInsets(Android.Views.View? view, AndroidX.Core.View.WindowInsetsCompat? insets)
	{
		if (view == null || insets == null)
		{
			return insets;
		}

		var systemBarInsets = insets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.SystemBars());
		if (view.LayoutParameters is Android.Views.ViewGroup.MarginLayoutParams marginParams && systemBarInsets != null)
		{
			marginParams.LeftMargin = systemBarInsets.Left;
			marginParams.BottomMargin = systemBarInsets.Bottom;
			marginParams.RightMargin = systemBarInsets.Right;
			marginParams.TopMargin = systemBarInsets.Top;
			view.LayoutParameters = marginParams;
		}

		return  AndroidX.Core.View.WindowInsetsCompat.Consumed;
	}
}
#endif
