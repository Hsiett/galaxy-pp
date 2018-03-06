# galaxy-pp

An active fork from Beier's original **galaxy++** back in 2012. I use it to publish my game project in Starcraft 2 Arcade.

## New features

1, Add support to extract functions to `precompiled.libraryData`, and access the texture from the new Casc file system using CascLib (https://github.com/ladislav-zezula/CascLib).
2, Can extract all libraries from user created MODs. They should reside in the default MOD folder as usual. Currently, it only searches for user mods in the sub-folders of MOD.
3, Upgrade to .Net 4.0 Framework with xna 4.0. The project can be compiled in win10 with VS 2015 community version.
