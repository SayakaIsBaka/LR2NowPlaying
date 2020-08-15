# LR2NowPlaying

A tool that extracts the chart you're currently playing on LR2(HD).

## Requirements

- .NET Framework 4.6.1

## Setup

Grab the latest release from the [Releases tab](https://github.com/SayakaIsBaka/LR2NowPlaying/releases) and run `LR2NowPlaying.exe` **while LR2 is running**. The data will be saved in the executable's folder as `nowplaying.txt`.
If you wish to edit the way the data is displayed in the file, edit the `template.txt` file. Currently, the following tokens are available:
- `{artist}`: artist of the song
- `{genre}`: genre of the song
- `{title}`: title of the song
- `{tags}`: tags on the chart on LR2IR

## Building

You will need at least Visual Studio 2017 and the Http Agility Pack package to build this project.
**The DLL's source code (LR2mind.dll) has been voluntarily omitted from the repository to avoid enabling cheating, as the DLL is being injected into LR2's process.**

## TODO

- Implement a fallback if the BMS cannot be found on LR2IR / the connection to LR2IR fails
- Exit cleanly (probably not going to happen)
- Making LR2mind not broadcast the message on UDP but instead only send it on localhost (low priority)
- Make the program work if the DLL's path contains non-ASCII characters

## Special thanks

Nothilvien for writing LR2mind's code and the original injector's code (in C++) and basically giving me the motivation to actually make this
