using Sandbox;
using System;
using System.Text.Json.Serialization;
using static Sandbox.ScreenPanel;

namespace PostprocessPanel.Utility;

public class RenderingRootSettings()
{
	public const string DEFAULT_PROCESSED_NAME = "ProcessedTexture";

	public const string DEFAULT_RAW_NAME = "RawTexture";

	[JsonIgnore, ReadOnly]
	public bool HasRenderingCallback => OnRendering?.GetInvocationList().Length > 0;

	public Action<PostprocessablePanel> OnRendering { get; set; }

	public string ProcessedName { get; set; } = DEFAULT_PROCESSED_NAME;

	public string RawName { get; set; } = DEFAULT_RAW_NAME;

	public float ManualScale { get; set; } = 1f;

	public bool AutoScreenScale { get; set; } = true;

	public AutoScale ScaleStrategy { get; set; }

	[field: Hide]
	public Vector2Int TexturePadding
	{
		get;
		set
		{
			if ( TexturePadding == value )
				return;

			field = value;

			if ( value.x < 0 )
			{
				Log.Warning( $"Texture padding ({value}) x size is below 0! Setting it to 0." );
				field.x = 0;
			}

			if ( value.y < 0 )
			{
				Log.Warning( $"Texture padding ({value}) y size is below 0! Setting it to 0." );
				field.y = 0;
			}
		}
	}
}
