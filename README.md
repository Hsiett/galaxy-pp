# galaxy-pp #

An fork from Beier's original **galaxy++** back in 2012. I use it to publish my game project in Starcraft 2 Arcade. This project includes:
	
* A compiler to compile galaxy++ to galaxy code.
* An IDE to edit, compile and generate Arcade game map.
* An GUI to customize the user interface in Game.

Galaxy++ is an programming language that extends `Galaxy` scripts used by Starcraft 2. It applies object-oriented programming concept to allow map makers to code and manage the script more efficiently. To learn more about Galaxy++,
you can refer to [Galaxy++ editor by Beier](https://www.sc2mapster.com/projects/galaxy-editor-beier).


## New features ##


1. Add support to extract functions to `precompiled.libraryData`, and access the texture from the new Casc file system using CascLib (https://github.com/ladislav-zezula/CascLib).
2. Can extract all libraries from user created MODs. They should reside in the default MOD folder as usual. Currently, it only searches for user mods in the sub-folders of MOD.
3. Dialog Designer: Add a button to delete a dialog and its children.
4. Upgrade to .Net 4.0 Framework with xna 4.0. The project can be compiled in win10 with VS 2015 community version.
