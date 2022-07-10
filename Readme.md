# Wrath Of The Righteous Resource Unification

##What is this?

It's a tool to merge different (game engine) instances of the same (tabletop rules) pool from different classes and make them behave correctly.

## Changelog
1.2.0
Fix for breaking change somewhere in Owlcat land.


1.1.1
Fix for not properly handing bad input GUIDs.

1.1: 
Improved internal logic and autoparsing.
Improved config.


##Default Configuration:
Monk Ki: Combines normal Monk, Scaled Fist, Sacred Fist and Hellcat from Homebrew Archetypes, and Ninja from the Ninja mod.
Magus Arcane Pool: Combines normal Magus and Eldritch Scion. 


Alchemist Bombs: Combines normal Alchmist and Rappa from the Ninja mod.

Arcanist: Imposes mod resource scaling which should fix an issue with Arcanist/Arcane Enforce multiclasses.

##Known issues:
The UMM menu status report will tell you that TTT Bladebound is not having its arcane pool progression reduced. Ignore this, TTT's method of handling that isn't detected but isn't blocked either.
Archetypes that replace


##Customization
You can make the mod handle more resources by adding the guids to the ResourceDefines.json files in UMM mod root directories.

ResourceAdderFeatureGuid contains the Guid of the feature granting the resource via AddAbilityResource. It needs to be in the class/archetype progression to work alone.
If there is a feature that grants the actual resource-granting feature as with Alchemist Bombs, put the guid of that intermediate feature into WrapperGuid

It doesn't currently matter if you use ClassResourceFeatureGuids or NonClassResourceFeatureGuids.
It will eventually - put class feature from leveling in ClassResourceFeatureGuids and anything else that grants a resource like the Rogue Ki Pool Talent into NonClassResourceFeatureGuids

Features that increase existing resources or alter costs should be handled automagically by my code, you don't need to do anything with them.

##Uninstalling:
Just remove it.

It doesn't add any blueprints or make any state changes, just redirects things.
