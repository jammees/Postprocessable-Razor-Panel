using Sandbox;
using Sandbox.UI;
using System;

namespace PostprocessPanel;

/// <summary>
/// A panel that allows to modify the rendered Body with
/// compute shaders and stuffs.
/// 
/// A few quick mentions:
/// 
/// - The body panel should not be transformed if the
///		padding is not big enough. This will cause the
///		panel to be cut off.
///		
///	- When setting the texture padding, make sure to take
///		the aspect ratio of the body panel into account!
///		
/// - The padding is automatically scaled with the screen!
/// 
///	- The body panel does not support backdrop filters
///		as they require the rendered scene to be in
///		the background. However, the texture that this is
///		rendered onto does not contain it!
/// </summary>
public sealed partial class PostprocessablePanel : Panel, IRenderingRootAccessor
{
	/// <summary>
	/// Body fragment. This panel will be the one that gets
	/// rendered in a separate root.
	/// </summary>
	[Parameter]
	public RenderFragment Body { get; set; }

	/// <summary>
	/// Display fragment. This panel will be used to display
	/// the rendered body onto after it was post processed.
	/// </summary>
	[Parameter]
	public RenderFragment Display { get; set; }

	/// <summary>
	/// Reference to the attributes that holds the raw texture
	/// of the body panel as well as other stuffs that might
	/// be necessary for the compute shaders.
	/// 
	/// The key for the raw body texture is "RawTexture" and
	/// the finalised texture is expected to have "ProcessedTexture"
	/// by default. This can be changed with SetProcessedTextureName()!
	/// 
	/// This is cleared between each frame!
	/// </summary>
	public RenderAttributes Attributes => _root.Attributes;

	/// <summary>
	/// Do we have our display panel?
	/// </summary>
	public bool HasDisplayPanel => DisplayPanel is not null;

	/// <summary>
	/// Do we have our body panel, that is rendered to
	/// a texture?
	/// </summary>
	public bool HasBodyPanel => _root.IsValid() && _root.HasBodyPanel;

	/// <summary>
	/// Think of this as the IsValid field. Tells if the panel
	/// has everything it needs to do its job.
	/// </summary>
	public bool IsReady => this.IsValid() && _root.IsValid();

	/// <summary>
	/// How big is our texture that we rendered onto. This
	/// is decided by how big the panel itself is. This includes
	/// the padding!
	/// </summary>
	public Vector2Int TextureSize => _root.TextureSize;

	/// <summary>
	/// Add extra pixels to the render texture in case
	/// the effect requires so. Is clamped if either axis
	/// are below 0!
	/// </summary>
	[Parameter]
	public Vector2Int TexturePadding
	{
		get => _root.TexturePadding;
		set => _root.TexturePadding = value;
	}

	/// <summary>
	/// Dispatched after we're done rendering the body panel
	/// and saved it into the attributes. This is the time
	/// to dispatch the compute shaders to modify the texture.
	/// </summary>
	[Parameter]
	public Action OnRendering
	{
		get => _root.OnRendering;
		set
		{
			_root.HasRenderingCallback = true;
			_root.OnRendering = value;
		}
	}

	private string _processedLookupName;

	internal Panel DisplayPanel => GetChild( 0 );

	private RenderingRoot _root;

	protected override void OnAfterTreeRender( bool firstTime )
	{
		if ( firstTime is false )
			return;

		CreateRoot();
		SetProcessedTextureName();
	}

	public override void Tick()
	{
		if ( _root.IsValid() is false || _root.HasBodyPanel is false || HasDisplayPanel is false )
			return;

		_root.CopyPseudoClasses( this.PseudoClass );

		Texture finalTexture = Attributes.GetTexture( _processedLookupName );

		DisplayPanel.Style.SetBackgroundImage( finalTexture );
	}

	public override void OnDeleted()
	{
		_root?.Delete( immediate: true );
	}

	public override int GetHashCode() => HashCode.Combine( _root.IsValid() );

	/// <summary>
	/// Update the scale and opacity from a screen panel
	/// </summary>
	/// <param name="screenPanel"></param>
	public void UpdateRootSettingsFrom( ScreenPanel screenPanel )
	{
		UpdateRootSettings(
			screenPanel.AutoScreenScale,
			screenPanel.Scale,
			screenPanel.ScaleStrategy,
			screenPanel.Opacity
		);
	}

	/// <summary>
	/// Allows updating the root settings such as the manual scale,
	/// opacity and the scaling strategy. This is useful if the screen panel
	/// or world panel uses different settings
	/// </summary>
	/// <param name="autoScale">Determine scale with the scaling strategy or with the manual scale</param>
	/// <param name="manualScale">If auto scale is off use this value to scale the panel</param>
	/// <param name="scaleStrategy">If auto scale is on, depending on the value set figure out scaling</param>
	/// <param name="manualOpacity">What opacity should the body panel be rendered?</param>
	public void UpdateRootSettings(
		bool autoScale = true,
		float manualScale = 1f,
		ScreenPanel.AutoScale scaleStrategy = ScreenPanel.AutoScale.ConsistentHeight,
		float manualOpacity = 1f
	)
	{
		_root.AutoScreenScale = autoScale;
		_root.ManualScale = manualScale;
		_root.ScaleStrategy = scaleStrategy;
		_root.ManualOpacity = manualOpacity;
	}

	/// <summary>
	/// Dispatches the compute shader with the attributes
	/// and with the correct amount of threads.
	/// </summary>
	/// <param name="compute"></param>
	public void DispatchCompute( ComputeShader compute )
	{
		compute.DispatchWithAttributes( Attributes, _root.TextureSize.x, _root.TextureSize.y, 1 );
	}

	/// <summary>
	/// Sets the name of the processed texture what will be
	/// used to look it up inside of the render attributes.
	/// 
	/// By default this is called ProcessedTexture. This
	/// method exists in case multiple passes are required.
	/// </summary>
	/// <param name="name"></param>
	public void SetProcessedTextureName( string name = "ProcessedTexture" )
	{
		_processedLookupName = name;

		if ( _root.IsValid() )
		{
			_root.ProcessedTextureName = _processedLookupName;
		}
	}

	private void CreateRoot()
	{
		_root = new( Body );
	}
}
