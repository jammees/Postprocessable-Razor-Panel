using Sandbox;
using System;
using System.Text.Json.Serialization;
using static Sandbox.ScreenPanel;

namespace PostprocessPanel
{
	public enum PaddingScale
	{
		/// <summary>
		/// Default behaviour, the <see cref="Texture"/>
		/// will be padded by the <see cref="Vector2Int"/>'s
		/// <c>x</c> and <c>y</c> components.
		/// </summary>
		Manual,

		/// <summary>
		/// Uses the aspect ratio of the <see cref="PostprocessablePanel.Body"/>
		/// and the <see cref="Vector2Int.x"/> component to figure out the
		/// padding required.
		/// </summary>
		UseRatio
	}
}

namespace PostprocessPanel.Utility
{
	public class RenderingRootSettings()
	{
		public const string DEFAULT_PROCESSED_NAME = "ProcessedTexture";

		public const string DEFAULT_RAW_NAME = "RawTexture";

		#region Rendering Action
		[JsonIgnore, ReadOnly]
		public bool HasRenderingCallback => OnRendering?.GetInvocationList().Length > 0;

		public Action<PostprocessablePanel> OnRendering { get; set; }
		#endregion

		#region Attribute Names
		public string ProcessedName { get; set; } = DEFAULT_PROCESSED_NAME;

		public string RawName { get; set; } = DEFAULT_RAW_NAME;
		#endregion

		#region Scaling
		public float ManualScale { get; set; } = 1f;

		public bool AutoScreenScale { get; set; } = true;

		public AutoScale ScaleStrategy { get; set; } = AutoScale.ConsistentHeight;

		public PaddingScale PaddingStrategy { get; set; } = PaddingScale.Manual;

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
		#endregion
	}
}
