# CustomMapUtility

Use by dropping the cs files in Assembly into your own project and add using CustomMapUtility to your namespace references.

Just credit me in the description or leave a link to the tool if you use CustomMapUtility

Requires NAudio 1.85 OR needs the NOMP3 compilation symbol defined.

Documentation is in-progress, basic usage is detailed in Template.cs
You can find info over at https://github.com/uGuardian/LoR_CustomMapUtility/wiki/Adding-invitation-maps

The Resources folder is included in your own mod, rename Template to your map name and include whichever image files you need. Only Background and Floor are required.

Can be used as a psuedo sound file loader, proper as one coming soon.

FAQ:
"No overload for method "Remove" takes 2 arguments"
Reference mscorlib.dll from the LibraryOfRuina_Data\Managed folder, you might have to add it to your project manually.
