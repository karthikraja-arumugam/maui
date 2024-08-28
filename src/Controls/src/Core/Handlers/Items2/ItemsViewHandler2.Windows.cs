﻿#nullable disable
#pragma warning disable RS0016 // Add public types and members to the declared API

using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using WItemsView = Microsoft.UI.Xaml.Controls.ItemsView;
using WASDKScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WVisibility = Microsoft.UI.Xaml.Visibility;
using WASDKApp = Microsoft.UI.Xaml.Application;
using WASDKDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using WScrollPresenter = Microsoft.UI.Xaml.Controls.Primitives.ScrollPresenter;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Internals;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public abstract class ItemsViewHandler2<TItemsView> : ViewHandler<TItemsView, WItemsView> where TItemsView : ItemsView
	{
		CollectionViewSource _collectionViewSource;
		IList _itemsSource;

		FrameworkElement _emptyView;
		View _mauiEmptyView;
		bool _emptyViewDisplayed;
		double _previousHorizontalOffset;
		double _previousVerticalOffset;

		WASDKScrollBarVisibility? _defaultHorizontalScrollVisibility;
		WASDKScrollBarVisibility? _defaultVerticalScrollVisibility;

		WeakNotifyPropertyChangedProxy _layoutPropertyChangedProxy;
		PropertyChangedEventHandler _layoutPropertyChanged;

		protected TItemsView ItemsView => VirtualView;
		protected TItemsView Element => VirtualView;

		protected WASDKDataTemplate ItemsViewTemplate => (WASDKDataTemplate)WASDKApp.Current.Resources["ItemsViewDefaultTemplate"];

		protected abstract IItemsLayout Layout { get; }

		public ItemsViewHandler2() : base(ItemsViewMapper)
		{

		}

		public ItemsViewHandler2(PropertyMapper mapper = null) : base(mapper ?? ItemsViewMapper)
		{

		}

		public static PropertyMapper<TItemsView, ItemsViewHandler2<TItemsView>> ItemsViewMapper = new(ViewMapper)
		{
			[Controls.ItemsView.ItemsSourceProperty.PropertyName] = MapItemsSource,
			[Controls.ItemsView.HorizontalScrollBarVisibilityProperty.PropertyName] = MapHorizontalScrollBarVisibility,
			[Controls.ItemsView.VerticalScrollBarVisibilityProperty.PropertyName] = MapVerticalScrollBarVisibility,
			[Controls.ItemsView.ItemTemplateProperty.PropertyName] = MapItemTemplate,
			[Controls.ItemsView.EmptyViewProperty.PropertyName] = MapEmptyView,
			[Controls.ItemsView.EmptyViewTemplateProperty.PropertyName] = MapEmptyViewTemplate,
			[Controls.ItemsView.FlowDirectionProperty.PropertyName] = MapFlowDirection,
			[Controls.ItemsView.IsVisibleProperty.PropertyName] = MapIsVisible,
			[Controls.ItemsView.ItemsUpdatingScrollModeProperty.PropertyName] = MapItemsUpdatingScrollMode
		};

		public static void MapItemsSource(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateItemsSource();
		}

		public static void MapHorizontalScrollBarVisibility(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateHorizontalScrollBarVisibility();
		}

		public static void MapVerticalScrollBarVisibility(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateVerticalScrollBarVisibility();
		}

		public static void MapItemTemplate(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateItemTemplate();
		}

		public static void MapEmptyView(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateEmptyView();
		}

		public static void MapEmptyViewTemplate(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateEmptyView();
		}

		public static void MapFlowDirection(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.PlatformView.UpdateFlowDirection(itemsView);
		}

		public static void MapIsVisible(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.PlatformView.UpdateVisibility(itemsView);
		}

		public static void MapItemsUpdatingScrollMode(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
		}

		public static void MapItemsLayout(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateItemsLayout();
		}

		protected override WItemsView CreatePlatformView()
		{
			var itemsView = SelectListViewBase();
			
			return itemsView;
		}

		protected override void ConnectHandler(WItemsView platformView)
		{
			base.ConnectHandler(platformView);

			if (Layout is not null)
			{
				_layoutPropertyChanged ??= LayoutPropertyChanged;
				_layoutPropertyChangedProxy = new WeakNotifyPropertyChangedProxy(Layout, _layoutPropertyChanged);
			}
			else if (_layoutPropertyChangedProxy is not null)
			{
				_layoutPropertyChangedProxy.Unsubscribe();
				_layoutPropertyChangedProxy = null;
			}
		}

		protected override void DisconnectHandler(WItemsView platformView)
		{
			base.DisconnectHandler(platformView);

			if (_layoutPropertyChangedProxy is not null)
			{
				_layoutPropertyChangedProxy.Unsubscribe();
				_layoutPropertyChangedProxy = null;
			}
		}

		CollectionViewSource CreateCollectionViewSource()
		{
			var itemsSource = Element.ItemsSource;
			var itemTemplate = Element.ItemTemplate;

			if (itemTemplate != null)
			{
				if (ItemsView is GroupableItemsView groupableItemsView && groupableItemsView.IsGrouped)
				{
					return new CollectionViewSource
					{
						Source = TemplatedItemSourceFactory2.CreateGrouped(itemsSource, itemTemplate,
							groupableItemsView.GroupHeaderTemplate, groupableItemsView.GroupFooterTemplate,
							Element, mauiContext: MauiContext),
						IsSourceGrouped = true
					};
				}
				else
				{
					return new CollectionViewSource
					{
						Source = TemplatedItemSourceFactory2.Create(itemsSource, itemTemplate, Element, mauiContext: MauiContext),
						IsSourceGrouped = false
					};
				}
			}

			return new CollectionViewSource
			{
				Source = itemsSource,
				IsSourceGrouped = false
			};
		}

		void UpdateItemTemplate()
		{
			if (Element == null || PlatformView == null)
			{
				return;
			}

			PlatformView.ItemTemplate = Element.ItemTemplate == null ? null : ItemsViewTemplate;

			UpdateItemsSource();
		}
		void UpdateItemsSource()
		{
			if (PlatformView is null)
			{
				return;
			}

			CleanUpCollectionViewSource();

			if (Element.ItemsSource is null)
			{
				return;
			}

			_collectionViewSource = CreateCollectionViewSource();
			_itemsSource = _collectionViewSource?.Source as IList;

			if (_collectionViewSource?.Source is INotifyCollectionChanged incc)
			{
				incc.CollectionChanged += ItemsChanged;
			}

			PlatformView.ItemsSource = _collectionViewSource.View;
			PlatformView.ItemTemplate = new ItemFactory(Element);

			UpdateEmptyViewVisibility();
		}

		protected virtual void CleanUpCollectionViewSource()
		{
			if (_collectionViewSource is not null)
			{
				if (_collectionViewSource.Source is ObservableItemTemplateCollection observableItemTemplateCollection)
				{
					observableItemTemplateCollection.CleanUp();
				}

				if (_collectionViewSource.Source is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged -= ItemsChanged;
				}

				_collectionViewSource.Source = null;
				_collectionViewSource = null;
			}

			// Remove all children inside the ItemsSource
			if (VirtualView is not null)
			{
				foreach (var item in PlatformView.GetChildren<ItemContentControl>())
				{
					var element = item.GetVisualElement();
					VirtualView.RemoveLogicalChild(element);
				}
			}

			if (VirtualView?.ItemsSource is null)
			{
				PlatformView.ItemsSource = null;
			}
		}

		void ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateEmptyViewVisibility();

			if (_itemsSource is null)
				return;

			if (VirtualView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepItemsInView)
			{
				// Keeps the first item in the list displayed when new items are added.
				PlatformView.StartBringItemIntoView(0, new BringIntoViewOptions() { AnimationDesired = false });
			}
			else if (VirtualView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepLastItemInView)
			{
				// Adjusts the scroll offset to keep the last item in the list displayed when new items are added.
				PlatformView.StartBringItemIntoView(_itemsSource.Count - 1, new BringIntoViewOptions() { AnimationDesired = false });
			}
		}

		WItemsView SelectListViewBase()
		{
			switch (Layout)
			{
				case GridItemsLayout gridItemsLayout:
					return CreateGridView(gridItemsLayout);
				case LinearItemsLayout listItemsLayout when listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical:
					return new WItemsView()
					{
						Layout = new UI.Xaml.Controls.StackLayout
						{
							Orientation = Orientation.Vertical,
							Spacing = listItemsLayout.ItemSpacing
						}
					};
				case LinearItemsLayout listItemsLayout when listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal:
					var itemsView = new WItemsView()
					{
						Layout = new Microsoft.UI.Xaml.Controls.StackLayout
						{
							Orientation = Orientation.Horizontal,
							Spacing = listItemsLayout.ItemSpacing
						}
					};
					ScrollViewer.SetHorizontalScrollMode(itemsView, UI.Xaml.Controls.ScrollMode.Enabled);
					ScrollViewer.SetHorizontalScrollBarVisibility(itemsView, WASDKScrollBarVisibility.Visible);

					ScrollViewer.SetVerticalScrollMode(itemsView, UI.Xaml.Controls.ScrollMode.Disabled);
					ScrollViewer.SetVerticalScrollBarVisibility(itemsView, WASDKScrollBarVisibility.Disabled);
					return itemsView;
			}
			throw new NotImplementedException("The layout is not implemented");
		}

		static WItemsView CreateGridView(GridItemsLayout gridItemsLayout)
		{
			return new WItemsView()
			{
				Layout = new UniformGridLayout()
				{
					Orientation = gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
						? Orientation.Vertical : Orientation.Horizontal,
					MaximumRowsOrColumns = gridItemsLayout.Span,
					MinColumnSpacing = gridItemsLayout.HorizontalItemSpacing,
					MinRowSpacing = gridItemsLayout.VerticalItemSpacing,
					ItemsStretch = UniformGridLayoutItemsStretch.Fill,
					ItemsJustification = UniformGridLayoutItemsJustification.Start,
				}
			};
		}

		protected virtual void UpdateItemsLayout()
		{
			FindScrollViewer();

			_defaultHorizontalScrollVisibility = null;
			_defaultVerticalScrollVisibility = null;

			UpdateItemTemplate();
			UpdateItemsSource();
			UpdateVerticalScrollBarVisibility();
			UpdateHorizontalScrollBarVisibility();
			UpdateEmptyView();
		}

		void FindScrollViewer()
		{
			if (PlatformView.ScrollView != null)
			{
				OnScrollViewerFound();
				return;
			}

			void ListViewLoaded(object sender, RoutedEventArgs e)
			{
				var lv = (WItemsView)sender;
				lv.Loaded -= ListViewLoaded;
				FindScrollViewer();
			}

			PlatformView.Loaded += ListViewLoaded;
		}

		protected virtual void OnScrollViewerFound()
		{
			if (PlatformView.ScrollView != null)
			{
				PlatformView.ScrollView.ViewChanged -= ScrollViewChanged;
				PlatformView.ScrollView.PointerWheelChanged -= PointerScrollChanged;
			}
			
			PlatformView.ScrollView.ViewChanged += ScrollViewChanged;
			PlatformView.ScrollView.PointerWheelChanged += PointerScrollChanged;
		}

		void LayoutPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == GridItemsLayout.SpanProperty.PropertyName)
				UpdateItemsLayoutSpan();
			else if (e.PropertyName == GridItemsLayout.HorizontalItemSpacingProperty.PropertyName || e.PropertyName == GridItemsLayout.VerticalItemSpacingProperty.PropertyName)
				UpdateItemsLayoutItemSpacing();
			else if (e.PropertyName == LinearItemsLayout.ItemSpacingProperty.PropertyName)
				UpdateItemsLayoutItemSpacing();
		}

		void UpdateItemsLayoutSpan()
		{
			if (PlatformView.Layout is UniformGridLayout listViewLayout &&
				Layout is GridItemsLayout gridItemsLayout)
			{
				listViewLayout.MaximumRowsOrColumns = gridItemsLayout.Span;
			}
		}

		void UpdateItemsLayoutItemSpacing()
		{
			if (PlatformView.Layout is UniformGridLayout listViewLayout &&
				Layout is GridItemsLayout gridItemsLayout)
			{
				listViewLayout.MinColumnSpacing = gridItemsLayout.HorizontalItemSpacing;
				listViewLayout.MinRowSpacing = gridItemsLayout.VerticalItemSpacing;
			}
			else if (PlatformView.Layout is UI.Xaml.Controls.StackLayout stackLayout &&
				Layout is LinearItemsLayout linearItemsLayout)
			{
				stackLayout.Spacing = linearItemsLayout.ItemSpacing;
			}
		}

		protected void UpdateEmptyView()
		{
			if (Element is null || PlatformView is null)
			{
				return;
			}

			var emptyView = Element.EmptyView;

			if (emptyView is null)
			{
				return;
			}

			_emptyView = emptyView switch
			{
				string text => new TextBlock
				{
					HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
					VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
					Text = text
				},
				View view => (FrameworkElement)RealizeEmptyView(view),
				_ => (FrameworkElement)RealizeEmptyViewTemplate(emptyView, Element.EmptyViewTemplate),
			};
			(PlatformView as IEmptyView)?.SetEmptyView(_emptyView, _mauiEmptyView);

			UpdateEmptyViewVisibility();
		}

		FrameworkElement RealizeEmptyViewTemplate(object bindingContext, DataTemplate emptyViewTemplate)
		{
			if (emptyViewTemplate == null)
			{
				return new TextBlock
				{
					HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
					VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
					Text = bindingContext.ToString()
				};
			}

			var template = emptyViewTemplate.SelectDataTemplate(bindingContext, null);
			var view = template.CreateContent() as View;
			view.BindingContext = bindingContext;

			return RealizeEmptyView(view);
		}

		FrameworkElement RealizeEmptyView(View view)
		{
			_mauiEmptyView = view ?? throw new ArgumentNullException(nameof(view));

			var handler = view.ToHandler(MauiContext);
			var platformView = handler.ContainerView ?? handler.PlatformView;

			return platformView;
		}

		protected void UpdateEmptyViewVisibility()
		{
			bool isEmpty = (_collectionViewSource?.View?.Count ?? 0) == 0;

			if (isEmpty)
			{
				if (_mauiEmptyView != null)
				{
					if (_emptyViewDisplayed)
						ItemsView.RemoveLogicalChild(_mauiEmptyView);

					if (ItemsView.EmptyViewTemplate == null)
						ItemsView.AddLogicalChild(_mauiEmptyView);
				}

				if (_emptyView != null && PlatformView is IEmptyView emptyView)
				{
					emptyView.EmptyViewVisibility = WVisibility.Visible;

					if (PlatformView.ActualWidth >= 0 && PlatformView.ActualHeight >= 0)
						_mauiEmptyView?.Layout(new Rect(0, 0, PlatformView.ActualWidth, PlatformView.ActualHeight));
				}

				_emptyViewDisplayed = true;
			}
			else
			{
				if (_emptyViewDisplayed)
				{
					if (_emptyView != null && PlatformView is IEmptyView emptyView)
						emptyView.EmptyViewVisibility = WVisibility.Collapsed;

					ItemsView.RemoveLogicalChild(_mauiEmptyView);
				}

				_emptyViewDisplayed = false;
			}
		}

		void UpdateVerticalScrollBarVisibility()
		{
			if (Element.VerticalScrollBarVisibility != ScrollBarVisibility.Default)
			{
				// If the value is changing to anything other than the default, record the default 
				if (_defaultVerticalScrollVisibility == null)
				{
					_defaultVerticalScrollVisibility = ScrollViewer.GetVerticalScrollBarVisibility(PlatformView);
				}
			}

			if (_defaultVerticalScrollVisibility == null)
			{
				// If the default has never been recorded, then this has never been set to anything but the 
				// default value; there's nothing to do.
				return;
			}

			switch (Element.VerticalScrollBarVisibility)
			{
				case (ScrollBarVisibility.Always):
					ScrollViewer.SetVerticalScrollBarVisibility(PlatformView, WASDKScrollBarVisibility.Visible);
					break;
				case (ScrollBarVisibility.Never):
					ScrollViewer.SetVerticalScrollBarVisibility(PlatformView, WASDKScrollBarVisibility.Hidden);
					break;
				case (ScrollBarVisibility.Default):
					ScrollViewer.SetVerticalScrollBarVisibility(PlatformView, _defaultVerticalScrollVisibility.Value);
					break;
			}
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			if (_defaultHorizontalScrollVisibility == null)
			{
				_defaultHorizontalScrollVisibility = ScrollViewer.GetHorizontalScrollBarVisibility(PlatformView);
			}

			switch (Element.HorizontalScrollBarVisibility)
			{
				case (ScrollBarVisibility.Always):
					ScrollViewer.SetHorizontalScrollBarVisibility(PlatformView, WASDKScrollBarVisibility.Visible);
					break;
				case (ScrollBarVisibility.Never):
					ScrollViewer.SetHorizontalScrollBarVisibility(PlatformView, WASDKScrollBarVisibility.Hidden);
					break;
				case (ScrollBarVisibility.Default):
					ScrollViewer.SetHorizontalScrollBarVisibility(PlatformView, _defaultHorizontalScrollVisibility.Value);
					break;
			}
		}

		void PointerScrollChanged(object sender, PointerRoutedEventArgs e)
		{
			if (PlatformView.ScrollView.ComputedHorizontalScrollMode == ScrollingScrollMode.Enabled)
			{
				PlatformView.ScrollView.AddScrollVelocity(new(e.GetCurrentPoint(PlatformView.ScrollView).Properties.MouseWheelDelta, 0), null);
			}
		}

		void ScrollViewChanged(UI.Xaml.Controls.ScrollView sender, object args)
		{
			HandleScroll(PlatformView.ScrollView.ScrollPresenter);
		}

		internal void HandleScroll(WScrollPresenter scrollViewer)
		{
			var itemsViewScrolledEventArgs = new ItemsViewScrolledEventArgs
			{
				HorizontalOffset = scrollViewer.HorizontalOffset,
				HorizontalDelta = scrollViewer.HorizontalOffset - _previousHorizontalOffset,
				VerticalOffset = scrollViewer.VerticalOffset,
				VerticalDelta = scrollViewer.VerticalOffset - _previousVerticalOffset,
			};

			_previousHorizontalOffset = scrollViewer.HorizontalOffset;
			_previousVerticalOffset = scrollViewer.VerticalOffset;

			bool advancing = true;
			switch (Layout)
			{
				case LinearItemsLayout linearItemsLayout:
					advancing = itemsViewScrolledEventArgs.HorizontalDelta > 0;
					break;
				case GridItemsLayout gridItemsLayout:
					advancing = itemsViewScrolledEventArgs.VerticalDelta > 0;
					break;
				default:
					break;
			}

			itemsViewScrolledEventArgs = ComputeVisibleIndexes(itemsViewScrolledEventArgs, advancing);

			Element.SendScrolled(itemsViewScrolledEventArgs);

			var remainingItemsThreshold = Element.RemainingItemsThreshold;
			if (remainingItemsThreshold > -1 &&
				_collectionViewSource.View.Count - 1 - itemsViewScrolledEventArgs.LastVisibleItemIndex <= remainingItemsThreshold)
			{
				Element.SendRemainingItemsThresholdReached();
			}
		}

		protected virtual ItemsViewScrolledEventArgs ComputeVisibleIndexes(ItemsViewScrolledEventArgs args, bool advancing)
		{
			var (firstVisibleItemIndex, lastVisibleItemIndex, centerItemIndex) = GetVisibleIndexes(advancing);

			args.FirstVisibleItemIndex = firstVisibleItemIndex;
			args.CenterItemIndex = centerItemIndex;
			args.LastVisibleItemIndex = lastVisibleItemIndex;

			return args;
		}

		(int firstVisibleItemIndex, int lastVisibleItemIndex, int centerItemIndex) GetVisibleIndexes(bool advancing)
		{
			int firstVisibleItemIndex = -1;
			int lastVisibleItemIndex = -1;

			PlatformView.TryGetItemIndex(0, 0, out firstVisibleItemIndex);
			PlatformView.TryGetItemIndex(1, 1, out lastVisibleItemIndex);

			double center = (lastVisibleItemIndex + firstVisibleItemIndex) / 2.0;
			int centerItemIndex = advancing ? (int)Math.Ceiling(center) : (int)Math.Floor(center);

			return (firstVisibleItemIndex, lastVisibleItemIndex, centerItemIndex);
		}

		void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			var index = args.Index;
			if (args.Mode == ScrollToMode.Element)
			{
				index = FindItemIndex(args.Item);
			}

			if (index >= 0)
			{
				float offset = 0.0f;
				switch (args.ScrollToPosition)
				{
					case ScrollToPosition.Start:
						offset = 0.0f;
						break;
					case ScrollToPosition.Center:
						offset = 0.5f;
						break;
					case ScrollToPosition.End:
						offset = 1.0f;
						break;
				}

				PlatformView.StartBringItemIntoView(index, new BringIntoViewOptions()
				{
					AnimationDesired = args.IsAnimated,
					VerticalAlignmentRatio = offset,
					HorizontalAlignmentRatio = offset
				});
			}
		}

		internal int FindItemIndex(object item)
		{
			for (int n = 0; n < _collectionViewSource.View.Count; n++)
			{
				if (_collectionViewSource.View[n] is ItemTemplateContext pair)
				{
					if (pair.Item == item)
					{
						return n;
					}
				}
			}

			return -1;
		}
	}
}
#pragma warning restore RS0016 // Add public types and members to the declared API