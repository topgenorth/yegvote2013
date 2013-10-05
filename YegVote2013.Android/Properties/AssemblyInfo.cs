using System.Reflection;
using System.Runtime.InteropServices;

using Android;
using Android.App;

[assembly: AssemblyTitle("YegVote2013")]
[assembly: AssemblyCompany("Opgenorth Holdings Ltd.")]
[assembly: AssemblyProduct("YegVote2013")]
[assembly: AssemblyCopyright("Copyright ©  2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]

[assembly: UsesPermission(Manifest.Permission.Internet)]
[assembly: UsesPermission(Manifest.Permission.WriteExternalStorage)]
[assembly: UsesPermission(Manifest.Permission.AccessNetworkState)]
#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
[assembly: AssemblyDescription("DEBUG")]
[assembly: Application(Debuggable = true,
    Icon = "@drawable/ic_launcher",
    Theme = "@style/Theme.YegVote2013",
    Label = "@string/app_name")]

#else
[assembly: AssemblyConfiguration("Release")]
[assembly: Application(Debuggable = false,
    Icon = "@drawable/ic_launcher",
    Theme = "@style/Theme.YegVote2013",
    Label = "@string/app_name")]

#endif
