using PostprocessPanel.Utility;
using Sandbox;
using Sandbox.UI;

namespace PostprocessPanel;

/// <summary>
/// Handles the rendering of the main panel and storing the result
/// in a texture.
/// This should not be used, but if you know what you're doing
/// feel free!
/// </summary>
public sealed partial class RenderingRoot : RootPanel
{
	public RenderAttributes Attributes { get; private set; }

	public Vector2Int TextureSize { get; private set; }

	public bool IsReady => BodyPanel.IsValid() && Renderer.IsValid();

	internal Panel BodyPanel => GetChild( 0 );

	internal Vector2 ScaledTexturePadding => (Vector2)GetPadding() / PanelUtils.GetScale( this, RendererScene.Camera.ScreenRect );

	internal RenderingRootSettings RootSettings => ProcessablePanel?.RootSettings;

	internal RenderFragment BodyFragment => ProcessablePanel?.Body;

	internal Scene RendererScene => ProcessablePanel?.Scene;

	internal SceneCustomObject Renderer;

	internal PostprocessablePanel ProcessablePanel;

	private bool _appliedStylesheets;

	public RenderingRoot( PostprocessablePanel panel )
	{
		ProcessablePanel = panel;

		SetupForRendering();
	}

	// from GameRootPanel
	protected override void UpdateScale( Rect screenSize )
	{
		if ( RootSettings is null )
			return;

		base.Scale = PanelUtils.GetScale( this, screenSize );
	}

	public override void OnDeleted()
	{
		Renderer?.Delete();
		Attributes?.Clear();
	}

	internal void TryApplyStylesheet()
	{
		if ( _appliedStylesheets is true )
			return;

		if ( ProcessablePanel?.IsReady is false )
			return;

		_appliedStylesheets = true;

		foreach ( var item in ProcessablePanel.DisplayPanel.AllStyleSheets )
		{
			BodyPanel.StyleSheet.Add( item );
		}
	}

	public void CopyPseudoClasses( PseudoClass classes )
	{
		if ( BodyPanel.IsValid() is false )
		{
			Log.Error( "No body panel as child!" );
			return;
		}

		BodyPanel.PseudoClass = classes;
	}

	private void SetupForRendering()
	{
		Attributes = new();

		RenderedManually = true;

		if ( RendererScene.IsValid() is false )
		{
			Log.Error( "RenderingRoot attached to invalid scene!" );
			Delete( true );
			return;
		}

		Renderer = new( RendererScene.SceneWorld );
		Renderer.Batchable = false;
		Renderer.RenderLayer = SceneRenderLayer.OverlayWithoutDepth;
		Renderer.RenderOverride = OnRender;
	}

	private void UpdatePadding()
	{
		Style.PaddingLeft = ScaledTexturePadding.x * 0.5f;
		Style.PaddingTop = ScaledTexturePadding.y * 0.5f;
	}

	private void OnRender( SceneObject obj )
	{
		if ( BodyPanel.IsValid() is false )
			return;

		UpdatePadding();

		Attributes.Clear();

		Vector2 panelSize = BodyPanel.Box.Rect.Size;

		TextureSize = new( panelSize.x.FloorToInt(), panelSize.y.FloorToInt() );
		TextureSize += GetPadding();

		if ( TextureSize.x < 1f || TextureSize.y < 1f )
			return;

		RenderTarget target = RenderTarget.GetTemporary( TextureSize.x, TextureSize.y, ImageFormat.RGBA8888, ImageFormat.None );

		Graphics.RenderTarget = target;
		Graphics.Clear( Color.Transparent );

		RenderManual( opacity: 1f );

		Texture processedTexture = Texture.Create( TextureSize.x, TextureSize.y, ImageFormat.RGBA8888 )
			.WithUAVBinding()
			.Finish();

		Attributes.Set( RootSettings.RawName, target.ColorTarget );
		Attributes.Set( RootSettings.ProcessedName, processedTexture );

		if ( RootSettings.HasRenderingCallback is false )
			Attributes.Set( RootSettings.ProcessedName, target.ColorTarget );

		Graphics.RenderTarget = null;
		target.Dispose();

		RootSettings.OnRendering?.Invoke( ProcessablePanel );
	}

	private Vector2Int GetPadding()
	{
		switch ( RootSettings.PaddingStrategy )
		{
			case PaddingScale.UseRatio:
				{
					Rect bodyRect = BodyPanel.Box.Rect;
					float bodyRatio = bodyRect.Width / bodyRect.Height;
					int paddingSize = RootSettings.TexturePadding.x;

					Vector2Int padding;
					if ( bodyRatio < 1 )
						padding = new( (paddingSize * bodyRatio).CeilToInt(), paddingSize );
					else
						padding = new( paddingSize, (paddingSize / bodyRatio).CeilToInt() );

					return EnsureVectorIsEven( padding );
				}
			default:
				return RootSettings.TexturePadding;
		}
	}

	private Vector2Int EnsureVectorIsEven( Vector2Int vector )
	{
		return new Vector2Int(
			vector.x.UnsignedMod( 2 ) == 0 ? vector.x : vector.x + 1,
			vector.y.UnsignedMod( 2 ) == 0 ? vector.y : vector.y + 1 );
	}
}
