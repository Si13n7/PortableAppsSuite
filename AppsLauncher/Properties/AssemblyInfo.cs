using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if x86
[assembly: AssemblyTitle("Portable Apps Launcher")]
[assembly: AssemblyProduct("AppsLauncher")]
#else
[assembly: AssemblyTitle("Portable Apps Launcher (64-bit)")]
[assembly: AssemblyProduct("AppsLauncher64")]
#endif
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Si13n7 Dev. ®")]
[assembly: AssemblyCopyright("Copyright © Si13n7 Dev. ® 2016")]
[assembly: AssemblyTrademark("Si13n7 Dev. ®")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: Guid("ee806ac7-2a8b-4d79-8634-adbdbba0aebf")]

[assembly: AssemblyVersion("16.5.20.*")]
