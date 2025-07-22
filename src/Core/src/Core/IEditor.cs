namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View used to accept multi-line input.
	/// </summary>
	public interface IEditor : IView, ITextInput, ITextStyle, ITextAlignment
	{
		/// <summary>
		/// Occurs when the user finalizes the text in an editor with the return key.
		/// </summary>
		void Completed();

#pragma warning disable RS0016 // Add public types and members to the declared API
		bool IsAutoSize();
#pragma warning restore RS0016 // Add public types and members to the declared API
	}
}
