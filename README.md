# _Postprocessable Razor Panel_

A panel that allows to be modified with compute shaders to create effects that wouldn't be possible or be a hassle to set up.

__NOTE__: The library has not been tested with a world panel! This was written with a screen panel in mind. Howeve, in theory
they should work just fine... (maybe)

<br>

## Installation

The library can be found in the s&box library manager! It is recommended to install the library from there!

<br>

## Demo scene

The library contains a demo scene that showcases how you could use the library to achieve the desired effects.
It contains 4 compute shaders as of now with different functionality:

| Name:                    | Description:                                            | License & Source:                                                                                                      |
|--------------------------|---------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------|
| wavy                     | Makes the panel wave in the vertical axis               | [CC0](https://creativecommons.org/publicdomain/zero/1.0/) - jammees                                                    |
| dissolve                 | Dissolves the panel using a noise texture               | [CC0](https://creativecommons.org/publicdomain/zero/1.0/) - [Simple 2D dissolve](https://godotshaders.com/shader/simple-2d-dissolve/) by [godotshaders](https://godotshaders.com/)                                               |
| crt wrap                 | Warps the panel to mimic a crt screen                   | [MIT](https://github.com/DevPoodle/yt-example-projects/blob/main/LICENSE) - [crt_screen_effect](https://github.com/DevPoodle/yt-example-projects/tree/main/crt_screen_effect) by [DevPoodle](https://github.com/DevPoodle)  |
| only green red channel   | Outputs only the red and green channels of the texture  | [CC0](https://creativecommons.org/publicdomain/zero/1.0/) - jammees                                                    |

The demo scene can be found under the assets folder, while the shaders under example shaders!

If not needed anymore, all of the files under the assets folder and the _Examples_ folder
found within the _Code_ folder of the library can be deleted safely.

<br>

## Usage

The custom panel can be used in a _.razor_ file with the tag: __\<postprocessablepanel\>__. A reference
to this panel is needed!

To add the panel that needs to be rendered to a texture (referenced as a _body panel_), needs to be added inside
of the __body render fragment__. However, this is not enough as the processed texture needs
to be displayed on a different panel.

That panel from here on now is referenced as a _display panel_, that needs to be added inside of the
_display render fragment_. The sole purpose of this
is to mimic the shape of the _body panel_. Other than that, anything can be done to this.

<br>

## Rendering

After all the panels were created, somewhere along the lines we need to subscribe to the
`OnRendering` action. This is used to dispatch all of the compute shaders, update attributes and
the panel itself. It is recommended to have `PostprocessPanel.IsReady` at the top. This signals if
the processable panel itself is valid and the root panel that is used to render. Think of it as the
`IsValid` field.

One thing that might've noticed is the panel is invisible now. The reason is, after subscribing to
the `OnRendering` action, the processable panel will no longer update the processed texture to
be the raw one. To change what name to look up for inside of the `PostprocessPanel.Attributes` you can
use the `SetProcessedTextureName` method. By default, this is set to _"ProcessedTexture"_.

The raw texture's name is `RawTexture`.

## Example Setup

```razor
<root>
  <PostprocessablePanel @ref="PostprocessPanel">
    <Body>
      The panel that needs to be rendered to the texture
    </Body>
    <Display>
      This panel will be used to display the processed texture
    </Display>
  </PostprocessablePanel>
</root>

@code
{
  private PostprocessablePanel PostprocessPanel {get; set;}

  protected override void OnTreeFirstBuilt()
  {
    PostprocessPanel.OnRendering += OnRendering;
  }

  private void OnRendering()
  {
  	if (PostprocessPanel.IsReady is false)
  		return;

    PostprocessPanel.Attributes.Set("Opacity", 0.5f);

    Log.Info( "Rendering now! Even if you can't see me anymore..." );
  }
}
```

<br>

## API

### PostprocessablePanel

#### __Body__ `RenderFragment`

Used to get reference the _body panel_, which will
be rendered in a separate root.

#### __Display__ `RenderFragment`

Used to get reference the _display panel_, which will
display the processed texture.

Please see the [Some caviats](#some-caviats) section

#### __Attributes__ `RenderAttributes`

Reference to the rendering root's render attributes. This is used
to store the references to the _raw_, the _processed_ textures and
other values that the compute shaders need access to.

This is cleared between every frame!

#### __HasDisplayPanel__ `bool`

Do we have our display panel?

#### __HasBodyPanel__ `bool`

Do we have our body panel, that is rendered to
a texture?

#### __IsReady__ `bool`

Think of it as the `IsValid` field. Tells if the processable
panel and the rendering root is valid.

To be in parity with `IsValid` an `IsReady` extension method
exists, that checks first if the panel is null and then the
`IsReady`.

#### __TextureSize__ `Vector2Int`

How big is the raw texture, which includes padding. This is
determined by how big the _body panel_ is.

#### __TexturePadding__ `Vector2Int`

Add extra pixels for the raw texture. The _body panel_ is then
shifted to be in the center always.

#### __OnRendering__ `Action`

Dispatched after we're done rendering the body panel
and saved it into the attributes. This is the time
to dispatch the compute shaders to modify the texture.

#### __UpdateRootSettingsFrom__ `method`

Update the scale and opacity from a screen panel.

#### __UpdateRootSettings__ `method`

Update the scale, opacity, scaling strategy and manual scale manually.

#### __DispatchCompute__ `method`

Dispatch the provided compute shader with the `Attributes` and with `TextureSize`
amount of threads. This is more or less a quality-of-life method.

#### __SetProcessedTextureName__ `method`

Change the name of the processed texture. By default this is _"ProcessedTexture"_.
This is useful if multiple passes of compute shaders are used.

<br>

## Some caviats

### Hands off approach to styling:

The library does not modify styles! This means for example, that the body panel cannot have a
scaling transformation applied to it as it would be cut off or be anywhere else than the top left
corner of the screen.

### Display panel styling

To properly show the processed texture, the display panel MUST have a
variation of these styles:

```scss
.display {
  background-size: contain;
  background-repeat: no-repeat;
}
```

These are a must to make sure the texture if it has a padding applied to it will not repeat,
breaking the effect in the process.

### Backdrop filters and filters

With this approach unfortunately, backdrop filters and filters do not work quite well. Especially the
ones that need something to be in the background to be applied correctly. The reason is that the body panel
is rendered to a separate texture, which does not contain anything at all.

Applying the filters to the display panel will work somewhat. As long as there is no padding, 
