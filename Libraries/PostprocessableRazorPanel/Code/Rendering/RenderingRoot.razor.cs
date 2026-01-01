using Sandbox;
using Sandbox.UI;
using System;
using static Sandbox.ScreenPanel;

namespace PostprocessPanel;

/// <summary>
/// Handles the rendering of the main panel and storing the result
/// in a texture.
/// This should not be used (I wasn't able to make it internal)!
/// </summary>
public sealed partial class RenderingRoot : RootPanel
{
	public float ManualOpacity { get; set; } = 1f;

	public float ManualScale { get; set; } = 1f;

	public bool AutoScreenScale { get; set; } = true;

	public AutoScale ScaleStrategy { get; set; }

	public RenderAttributes Attributes;

	public Action OnRendering { get; set; }

	private Vector2Int _texturePadding;
	public Vector2Int TexturePadding
	{
		get => _texturePadding;
		set
		{
			if ( TexturePadding == value )
				return;

			_texturePadding = value;

			if ( value.x < 0 )
			{
				Log.Warning( $"Texture padding ({value}) x size is below 0! Setting it to 0." );
				_texturePadding.x = 0;
			}

			if ( value.y < 0 )
			{
				Log.Warning( $"Texture padding ({value}) y size is below 0! Setting it to 0." );
				_texturePadding.y = 0;
			}
		}
	}

	public string ProcessedTextureName { get; set; } = "ProcessedTexture";

	public Vector2Int TextureSize { get; private set; }

	public bool HasBodyPanel => BodyPanel is not null;

	internal bool HasRenderingCallback { get; set; } = false;

	internal Panel BodyPanel => GetChild( 0 );

	internal Vector2 ScaledTexturePadding => (Vector2)TexturePadding / PanelUtils.GetScale( this, Scene.Camera.ScreenRect );

	internal SceneCustomObject Renderer;

	private RenderFragment _body;

	public RenderingRoot( RenderFragment body )
	{
		_body = body;

		SetupForRendering();
	}

	public RenderingRoot()
	{
		_body = ChildContent;

		SetupForRendering();
	}

	// from GameRootPanel
	protected override void UpdateScale( Rect screenSize )
	{
		base.Scale = PanelUtils.GetScale( this, screenSize );
	}

	public override void OnDeleted()
	{
		Renderer?.Delete();
		Attributes?.Clear();
		OnRendering = null;
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

		Renderer = new( Scene.SceneWorld );
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
		TextureSize += TexturePadding;

		RenderTarget target = RenderTarget.GetTemporary( TextureSize.x, TextureSize.y, ImageFormat.RGBA8888, ImageFormat.None );

		Graphics.RenderTarget = target;
		Graphics.Clear( Color.Transparent );

		RenderManual( ManualOpacity );

		Texture processedTexture = Texture.Create( TextureSize.x, TextureSize.y, ImageFormat.RGBA8888 )
			.WithUAVBinding()
			.Finish();

		Attributes.Set( "RawTexture", target.ColorTarget );
		Attributes.Set( ProcessedTextureName, processedTexture );

		if ( HasRenderingCallback is false )
			Attributes.Set( ProcessedTextureName, target.ColorTarget );

		Graphics.RenderTarget = null;
		target.Dispose();

		OnRendering?.InvokeWithWarning();
	}
}
