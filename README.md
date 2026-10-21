# Sector XIV - Prelude

A roleplaying world-simulation fork of Space Station 14.

## Links

<div class="header">

* Website - Under Construction!
* Discord - <https://discord.gg/zVd9F6fjZh>
* Forum - Under Construction!
* Patreon - Under Construction!

</div>

## Contributing

We are happy to accept contributions from anybody. Get in Discord if you want to help.

### AI Policy

AI is part of many developers' workflows; we don't want to wholesale ban a *tool*. That said, we do not accept
low-effort pull requests under *any* context. Trying to flood the repository with 'slop' will result in being barred
from
the project.

- **Art / Sprites**: We do not accept AI-generated **or assisted** visual assets of any kind.
- **Writing / Lore**: We do not accept AI writing in any form. This includes item
  descriptions. You're writing for humans to read, here.
- **Code / Prototypes / Data**: We expect all code to be understood by the person submitting it. If you don't understand
  what the code does - *down to every single line* - you should not be trying to push it.

## Building

1. Clone this repo:

```shell
git clone https://github.com/citadel-station/sector-xiv.git
```

2. Go to the project folder and run `RUN_THIS.py` to initialize the submodules and load the engine:

```shell
cd space-station-14
python RUN_THIS.py
```

3. Compile the solution:

Build the server using `dotnet build`.

[More detailed instructions on building the project.](https://docs.spacestation14.com/en/general-development/setup.html)

## License

Most code for the content repository is licensed
under [MIT](https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT). However, some subdirectories may
contain their own LICENSE file, which **must** be read and heeded as the license for that subdirectory and its children.
`_Citadel` folders are the primary example of this, please read the license carefully before use.

A copy of the MPL (v2.0) is provided in MOZILLA_LICENSE at the root of this repository.

Most assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise.
Assets have their license and copyright specified in the metadata file. For example, see
the [metadata for a crowbar](https://github.com/space-wizards/space-station-14/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

> [!NOTE]
> Some assets are licensed under the
> non-commercial [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) or similar non-commercial
> licenses
> and will need to be removed if you wish to use this project commercially.
