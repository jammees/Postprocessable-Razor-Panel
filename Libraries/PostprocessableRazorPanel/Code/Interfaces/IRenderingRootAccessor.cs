using Sandbox;
using System;

namespace PostprocessPanel;

public interface IRenderingRootAccessor
{
	public RenderAttributes Attributes => null;

	public Vector2Int TextureSize => Vector2Int.Zero;

	public Vector2Int TexturePadding { get; set; }

	public Action OnRendering { get; set; }
}
