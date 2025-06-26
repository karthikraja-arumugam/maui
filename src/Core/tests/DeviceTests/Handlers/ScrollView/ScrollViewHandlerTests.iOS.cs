using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ScrollViewHandlerTests : CoreHandlerTestBase<ScrollViewHandler, ScrollViewStub>
	{
		[Fact]
		public async Task ContentInitializesCorrectly()
		{
			EnsureHandlerCreated(builder => { builder.ConfigureMauiHandlers(handlers => { handlers.AddHandler<EntryStub, EntryHandler>(); }); });

			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var entry = new EntryStub() { Text = "In a ScrollView" };

				var scrollView = new ScrollViewStub()
				{
					Content = entry
				};

				var scrollViewHandler = CreateHandler(scrollView);
				return scrollViewHandler.PlatformView.FindDescendantView<MauiTextField>() is not null;
			});

			Assert.True(result, $"Expected (but did not find) a {nameof(MauiTextField)} in the Subviews array");
		}

		[Fact]
		public async Task ScrollViewContentSizeSet()
		{
			EnsureHandlerCreated(builder => { builder.ConfigureMauiHandlers(handlers => { handlers.AddHandler<EntryStub, EntryHandler>(); }); });

			var scrollView = new ScrollViewStub();
			var entry = new EntryStub() { Text = "In a ScrollView" };
			scrollView.Content = entry;

			var scrollViewHandler = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(scrollView);

				// Setting an arbitrary value so we can verify that the handler is setting
				// the UIScrollView's ContentSize property during AttachAndRun
				handler.PlatformView.ContentSize = new CoreGraphics.CGSize(100, 100);
				return handler;
			});

			await InvokeOnMainThreadAsync(async () =>
			{
				await scrollViewHandler.PlatformView.AttachAndRun(() =>
				{
					// Verify that the ContentSize values have been modified
					Assert.NotEqual(100, scrollViewHandler.PlatformView.ContentSize.Height);
					Assert.NotEqual(100, scrollViewHandler.PlatformView.ContentSize.Width);
				});
			});
		}

		[Theory]
		[InlineData(ScrollBarVisibility.Always, true)]
		[InlineData(ScrollBarVisibility.Default, true)]
		[InlineData(ScrollBarVisibility.Never, false)]
		public async Task VerticalScrollBarVisibilityInitializesCorrectly(ScrollBarVisibility visibility, bool expected)
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub()
				{
					Orientation = ScrollOrientation.Vertical,
					VerticalScrollBarVisibility = visibility
				};

				var scrollViewHandler = CreateHandler(scrollView);
				return scrollViewHandler.PlatformView.ShowsVerticalScrollIndicator;
			});

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(ScrollBarVisibility.Always, true)]
		[InlineData(ScrollBarVisibility.Default, true)]
		[InlineData(ScrollBarVisibility.Never, false)]
		public async Task HorizontalScrollBarVisibilityInitializesCorrectly(ScrollBarVisibility visibility, bool expected)
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub()
				{
					Orientation = ScrollOrientation.Horizontal,
					HorizontalScrollBarVisibility = visibility
				};

				var scrollViewHandler = CreateHandler(scrollView);
				return scrollViewHandler.PlatformView.ShowsHorizontalScrollIndicator;
			});

			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task AlwaysVisibleVerticalScrollBarHasTimer()
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub()
				{
					Orientation = ScrollOrientation.Vertical,
					VerticalScrollBarVisibility = ScrollBarVisibility.Always
				};

				var scrollViewHandler = CreateHandler(scrollView);
				var mauiScrollView = scrollViewHandler.PlatformView as MauiScrollView;
				
				// Access the private field using reflection for testing
				var timerField = typeof(MauiScrollView).GetField("_scrollbarVisibilityTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var timer = timerField?.GetValue(mauiScrollView);
				
				return timer != null;
			});

			Assert.True(result);
		}

		[Fact]
		public async Task AlwaysVisibleHorizontalScrollBarHasTimer()
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub()
				{
					Orientation = ScrollOrientation.Horizontal,
					HorizontalScrollBarVisibility = ScrollBarVisibility.Always
				};

				var scrollViewHandler = CreateHandler(scrollView);
				var mauiScrollView = scrollViewHandler.PlatformView as MauiScrollView;
				
				// Access the private field using reflection for testing
				var timerField = typeof(MauiScrollView).GetField("_scrollbarVisibilityTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var timer = timerField?.GetValue(mauiScrollView);
				
				return timer != null;
			});

			Assert.True(result);
		}
	}
}
