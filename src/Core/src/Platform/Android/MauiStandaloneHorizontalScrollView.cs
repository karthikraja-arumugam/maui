using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// A standalone horizontal scroll view that properly handles touch events
	/// to prevent unintended focus behavior of child elements during scroll gestures.
	/// </summary>
	public class MauiStandaloneHorizontalScrollView : HorizontalScrollView
	{
		float _downX;
		float _downY;
		readonly int _touchSlop;

		public MauiStandaloneHorizontalScrollView(Context context) : base(context)
		{
			_touchSlop = ViewConfiguration.Get(context)?.ScaledTouchSlop ?? 20;
		}

		public MauiStandaloneHorizontalScrollView(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			_touchSlop = ViewConfiguration.Get(context)?.ScaledTouchSlop ?? 20;
		}

		public MauiStandaloneHorizontalScrollView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			_touchSlop = ViewConfiguration.Get(context)?.ScaledTouchSlop ?? 20;
		}

		protected MauiStandaloneHorizontalScrollView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			_touchSlop = 20; // fallback value
		}

		public override bool OnInterceptTouchEvent(MotionEvent? ev)
		{
			if (ev == null)
				return false;

			switch (ev.Action)
			{
				case MotionEventActions.Down:
					_downX = ev.GetX();
					_downY = ev.GetY();
					break;

				case MotionEventActions.Move:
					float deltaX = Math.Abs(ev.GetX() - _downX);
					float deltaY = Math.Abs(ev.GetY() - _downY);

					// If horizontal movement exceeds vertical movement and touch slop threshold,
					// intercept the touch to handle scrolling
					if (deltaX > _touchSlop && deltaX > deltaY)
					{
						return true;
					}
					break;

				case MotionEventActions.Up:
				case MotionEventActions.Cancel:
					break;
			}

			return base.OnInterceptTouchEvent(ev);
		}
	}
}