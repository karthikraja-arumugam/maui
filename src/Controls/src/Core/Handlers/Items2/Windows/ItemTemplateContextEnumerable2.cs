﻿#nullable disable
using System.Collections;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal class ItemTemplateContextEnumerable2 : IEnumerable
	{
		readonly IEnumerable _itemsSource;
		readonly DataTemplate _formsDataTemplate;
		readonly BindableObject _container;
		readonly IMauiContext _mauiContext;
		readonly double _itemHeight;
		readonly double _itemWidth;
		readonly Thickness _itemSpacing;

		public ItemTemplateContextEnumerable2(IEnumerable itemsSource, DataTemplate formsDataTemplate, BindableObject container,
			double? itemHeight = null, double? itemWidth = null, Thickness? itemSpacing = null, IMauiContext mauiContext = null)
		{
			_itemsSource = itemsSource;
			_formsDataTemplate = formsDataTemplate;
			_container = container;
			_mauiContext = mauiContext;
			if (itemHeight.HasValue)
				_itemHeight = itemHeight.Value;

			if (itemWidth.HasValue)
				_itemWidth = itemWidth.Value;

			if (itemSpacing.HasValue)
				_itemSpacing = itemSpacing.Value;
		}

		public IEnumerator GetEnumerator()
		{
			foreach (var item in _itemsSource)
			{
				yield return new ItemTemplateContext2(_formsDataTemplate, item, _container, _itemHeight, _itemWidth, _itemSpacing,
					false, false, _mauiContext);
			}
		}
	}
}