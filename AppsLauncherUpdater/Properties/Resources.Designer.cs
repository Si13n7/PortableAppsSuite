﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Updater.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Updater.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @echo off
        ///title &quot;{0}&quot;
        ///
        ///cd /d &quot;%~dp0&quot;
        ///7zG.exe x Update.7z -o&quot;{1}&quot; -y
        ///ping -n 5 localhost &gt;nul
        ///del /f /s /q Update.7z
        ///del /f /s /q 7z.dll
        ///del /f /s /q 7zG.exe
        ///
        ///set path=%WinDir%\Microsoft.NET\Framework\v4.0.30319\ngen.exe
        ///if exist %path% call %path% executeQueuedItems
        ///set path=%WinDir%\Microsoft.NET\Framework64\v4.0.30319\ngen.exe
        ///if exist %path% call %path% executeQueuedItems
        ///
        ///set path=%WinDir%\System32\cmd.exe
        ///if exist %0 start &quot;{2}&quot; %path% /c del /f /q %0 &amp;&amp; taskkill /fi &quot;{0}&quot; /im cmd.exe / [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string BatchDummy {
            get {
                return ResourceManager.GetString("BatchDummy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap diagonal_pattern {
            get {
                object obj = ResourceManager.GetObject("diagonal_pattern", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [default6]
        ///domain=www6.si13n7.com
        ///addr=
        ///ipv6=2a03:4000:0006:80e2:0000:0000:0000:0000
        ///ssl=False.
        /// </summary>
        internal static string IPv6DNS {
            get {
                return ResourceManager.GetString("IPv6DNS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Icon similar to (Icon).
        /// </summary>
        internal static System.Drawing.Icon PortableApps_green_64 {
            get {
                object obj = ResourceManager.GetObject("PortableApps_green_64", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap PortableAppsUpdater {
            get {
                object obj = ResourceManager.GetObject("PortableAppsUpdater", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}
