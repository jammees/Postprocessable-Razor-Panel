# _Postprocessable Razor Panel_

A panel that allows to be modified with compute shaders to create effects that wouldn't be possible or be a hassle to set up.

__NOTE__: The library has not been tested with a world panel! This was written with a screen panel in mind.

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

<br>

## Usage

The custom panel can be used in a _.razor_ file with the tag: <postprocessablepanel>. A reference
to this panel is needed!

To add the panel that needs to be rendered to a texture (referenced as a "body" panel), needs to be added inside
of the body render fragment. However, this is not enough as the processed texture needs
to be displayed on a different panel.

This panel from here on now is referenced as a "display" panel. The sole purpose of this
is to mimic the size and the position of the panel as it was the one that is being rendered to
a texture. The diplay panel can be created inside of the display render fragment.

### Example

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
}
```

## Some caviats

### Texture cut off:

The library does not modify styles! This means the body panel for example cannot have a
scaling transformation applied to it as it would be cut off!

### Display panel styling

To properly show the processed texture, the display panel MUST have a
variation of these styles:

```scss
.display {
  background-size: contain;
  background-repeat: no-repeat;
}
```

### Backdrop filters and filters

The ones that require the scene to be in the background do not work! As the body panel is rendered to a separate texture,
it is impossible to blur the scene behind the panel. Unless, it is applied to the display panel but that comes with its own negatives.
