using System;
using System.Reflection;
using System.Security.Permissions;
using CustomMapUtility;

[assembly: System.Reflection.AssemblyCompanyAttribute("uGuardian")]
#if DEBUG
#warning DEBUG
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
#else
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
#endif
[assembly: System.Reflection.AssemblyFileVersionAttribute(CustomMapHandler.ModResources.CacheInit.version)]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute(CustomMapHandler.ModResources.CacheInit.version)]
[assembly: System.Reflection.AssemblyProductAttribute("CustomMapUtility")]
[assembly: System.Reflection.AssemblyTitleAttribute("CustomMapUtility")]
[assembly: System.Reflection.AssemblyVersionAttribute(CustomMapHandler.ModResources.CacheInit.version)]