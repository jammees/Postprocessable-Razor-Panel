using PostprocessPanel.Utility;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using static System.Net.WebRequestMethods;

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
	/// rendered to the <see cref="Texture"/>.
	/// </summary>
	[Parameter]
	public RenderFragment Body { get; set; }

	/// <summary>
	/// Display fragment. This panel will be used to display
	/// the final <see cref="Texture"/>.
	/// </summary>
	[Parameter]
	public RenderFragment Display { get; set; }

	/// <summary>
	/// Add extra pixels to texture in case
	/// the effect requires so. Does not go below 0
	/// on either of the axis.
	/// </summary>
	[Parameter]
	public Vector2Int TexturePadding
	{
		get => RootSettings.TexturePadding;
		set => RootSettings.TexturePadding = value;
	}

	/// <summary>
	/// Called once the body is rendered onto the texture
	/// and saved it into the <see cref="RenderAttributes"/>. This is the time
	/// to dispatch the <see cref="ComputeShader"/> to modify the texture.
	/// </summary>
	[Parameter]
	public Action OnRendering
	{
		get => RootSettings.OnRendering;
		set => RootSettings.OnRendering = value;
	}

	/// <summary>
	/// Name to look for in the <see cref="RenderAttributes"/> to
	/// display it on the <see cref="Display"/> panel.
	/// 
	/// By default, this is <c>ProcessedTexture</c> stored in
	/// <see cref="RenderingRootSettings.DEFAULT_PROCESSED_NAME"/>
	/// </summary>
	[Parameter]
	public string ProcessedName
	{
		get => RootSettings.ProcessedName;
		set
		{
			if ( string.IsNullOrWhiteSpace( value ) )
			{
				Log.Error( $"Invalid processed texture name! Defaulting to \"{RenderingRootSettings.DEFAULT_PROCESSED_NAME}\"" );
				value = RenderingRootSettings.DEFAULT_PROCESSED_NAME;
			}

			RootSettings.ProcessedName = value;
		}
	}

	/// <summary>
	/// Name to look for in the <see cref="RenderAttributes"/> to
	/// save the rendered <see cref="Texture"/> of
	/// the <see cref="Body"/> panel.
	/// 
	/// By default, this is <c>RawTexture</c> stored in
	/// <see cref="RenderingRootSettings.DEFAULT_RAW_NAME"/>
	/// </summary>
	[Parameter]
	public string RawName
	{
		get => RootSettings.RawName;
		set
		{
			if ( string.IsNullOrWhiteSpace( value ) )
			{
				Log.Error( $"Invalid processed texture name! Defaulting to \"{RenderingRootSettings.DEFAULT_RAW_NAME}\"" );
				value = RenderingRootSettings.DEFAULT_RAW_NAME;
			}

			RootSettings.RawName = value;
		}
	}

	/// <summary>
	/// Stores everything needed for the <see cref="ComputeShader"/>s
	/// and for the library.
	/// </summary>
	public RenderAttributes Attributes => Root.Attributes;

	/// <summary>
	/// Think of this as the IsValid field. Tells if the panel
	/// has everything it needs to do its job.
	/// </summary>
	public bool IsReady => this.IsValid() && DisplayPanel.IsValid() && (Root?.IsReady ?? false);

	/// <summary>
	/// How big is the texture that we use to render
	/// the <see cref="Body"/> panel.
	/// </summary>
	public Vector2Int TextureSize => Root.TextureSize;

	internal Panel DisplayPanel => GetChild( 0 );

	internal RenderingRootSettings RootSettings = new();

	internal RenderingRoot Root;

	protected override void OnAfterTreeRender( bool firstTime )
	{
		if ( firstTime is false )
			return;

		Root = new( Body, Scene, ref RootSettings );
	}

	public override void Tick()
	{
		if ( IsReady is false )
			return;

		Root.CopyPseudoClasses( this.PseudoClass );

		Texture finalTexture = Attributes.GetTexture( ProcessedName );

		DisplayPanel.Style.SetBackgroundImage( finalTexture );
	}

	public override void OnDeleted()
	{
		Root?.Delete( immediate: true );
	}

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
		RootSettings.AutoScreenScale = autoScale;
		RootSettings.ManualScale = manualScale;
		RootSettings.ScaleStrategy = scaleStrategy;
		RootSettings.ManualOpacity = manualOpacity;
	}

	/// <summary>
	/// Dispatches the compute shader with the attributes
	/// and with the correct amount of threads.
	/// </summary>
	/// <param name="compute"></param>
	public void DispatchCompute( ComputeShader compute )
	{
		compute.DispatchWithAttributes( Attributes, Root.TextureSize.x, Root.TextureSize.y, 1 );
	}

	/// <summary>
	/// Sets the name of the processed texture what will be
	/// used to look it up inside of the render attributes.
	/// 
	/// By default this is called ProcessedTexture. This
	/// method exists in case multiple passes are required.
	/// </summary>
	/// <param name="name"></param>
	[Obsolete( "Use the ProcessedName parameter instead" )]
	public void SetProcessedTextureName( string name = "ProcessedTexture" )
	{
		ProcessedName = name;
	}
}
