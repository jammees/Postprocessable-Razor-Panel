namespace PostprocessPanel.Examples;

public static class Constants
{
	public static string ShadersPath => "example shaders";

	public enum ComputeTypes
	{
		/// <summary>
		/// Nothing interesting happens.
		/// </summary>
		None,

		/// <summary>
		/// Dissolve the texture using the slider.
		/// </summary>
		Dissolve,

		/// <summary>
		/// Static, shows basic texture
		/// modification.
		/// </summary>
		OnlyRedGreen,
		
		/// <summary>
		/// Applies a crt distortion, strength
		/// can be configured with the slider.
		/// </summary>
		CrtDistortion
	}
}
