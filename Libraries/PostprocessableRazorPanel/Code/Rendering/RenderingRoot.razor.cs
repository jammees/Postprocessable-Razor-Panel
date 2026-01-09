using PostprocessPanel.Utility;
using Sandbox;
using Sandbox.UI;
using System;
using static Sandbox.ScreenPanel;

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

	public bool HasBodyPanel => BodyPanel is not null;

	internal Panel BodyPanel => GetChild( 0 );

	internal Vector2 ScaledTexturePadding => (Vector2)RootSettings.TexturePadding / PanelUtils.GetScale( this, RendererScene.Camera.ScreenRect );
	internal Scene RendererScene;

	internal SceneCustomObject Renderer;

	internal RenderingRootSettings RootSettings;

	private RenderFragment _body;

	public RenderingRoot(
		RenderFragment body,
		Scene scene,
		ref RenderingRootSettings settings )
	{
		_body = body;
		RootSettings = settings;
		RendererScene = scene;

		SetupForRendering();
	}

	public RenderingRoot( Scene scene, ref RenderingRootSettings settings )
	{
		_body = ChildContent;
		RootSettings = settings;
		RendererScene = scene;

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

	// for some reason, the panel does not get deleted if this is present
	//public override int GetHashCode() => HashCode.Combine( TextureSize, HasRenderingCallback );

	/// <summary>
	/// Applies the pseudo classes to the body panel
	/// </summary>
	/// <param name="classes"></param>
	public void CopyPseudoClasses( PseudoClass classes )
	{
		if ( HasBodyPanel is false )
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
		if ( HasBodyPanel is false )
			return;

		UpdatePadding();

		Attributes.Clear();

		Vector2 panelSize = BodyPanel.Box.Rect.Size;

		TextureSize = new( panelSize.x.FloorToInt(), panelSize.y.FloorToInt() );
		TextureSize += RootSettings.TexturePadding;

		RenderTarget target = RenderTarget.GetTemporary( TextureSize.x, TextureSize.y, ImageFormat.RGBA8888, ImageFormat.None );

		Graphics.RenderTarget = target;
		Graphics.Clear( Color.Transparent );

		RenderManual( RootSettings.ManualOpacity );

		Texture processedTexture = Texture.Create( TextureSize.x, TextureSize.y, ImageFormat.RGBA8888 )
			.WithUAVBinding()
			.Finish();

		Attributes.Set( RootSettings.RawName, target.ColorTarget );
		Attributes.Set( RootSettings.ProcessedName, processedTexture );

		if ( RootSettings.HasRenderingCallback is false )
			Attributes.Set( RootSettings.ProcessedName, target.ColorTarget );

		Graphics.RenderTarget = null;
		target.Dispose();

		RootSettings.OnRendering?.InvokeWithWarning();
	}
}
