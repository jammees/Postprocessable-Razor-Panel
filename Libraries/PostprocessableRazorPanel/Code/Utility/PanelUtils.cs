using Sandbox;
using static Sandbox.ScreenPanel;

namespace PostprocessPanel;

internal static class PanelUtils
{
	// from GameRootPanel
	public static float GetScale( RenderingRoot root, Rect screenSize )
	{
		if ( root.AutoScreenScale )
		{
			float scale = 1f;

			if ( root.ScaleStrategy == AutoScale.ConsistentHeight )
			{
				scale = screenSize.Height / 1080f;
			}
			else if ( root.ScaleStrategy == AutoScale.FollowDesktopScaling )
			{
				scale = Screen.DesktopScale;
				float num = 1080f * Screen.DesktopScale;
				if ( screenSize.Height < num )
				{
					scale *= screenSize.Height / num;
				}
			}

			if ( Game.IsRunningOnHandheld )
			{
				scale *= 1.333f;
			}

			if ( root.IsVR && root.IsHighQualityVR )
			{
				scale = 2.33f;
			}

			return SanitizeScale( scale );
		}

		return SanitizeScale( root.ManualScale );
	}

	private static float SanitizeScale( float scale )
	{
		return scale.Clamp( 0.001f, 5f );
	}
}
