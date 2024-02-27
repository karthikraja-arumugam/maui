﻿using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue2617 : IssuesUITest
	{
		const string Success = "Success";

		public Issue2617(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Error on binding ListView with duplicated items";

		[Test]
		public async Task BindingToValuesTypesAndScrollingNoCrash()
		{
			await Task.Delay(4000);
			App.WaitForNoElement(Success);
		}
	}
}