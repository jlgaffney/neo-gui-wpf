﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Neo.UI.Base.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class EnumStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal EnumStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Neo.UI.Base.Resources.EnumStrings", typeof(EnumStrings).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dark.
        /// </summary>
        public static string Dark {
            get {
                return ResourceManager.GetString("Dark", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Light.
        /// </summary>
        public static string Light {
            get {
                return ResourceManager.GetString("Light", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Non-Standard.
        /// </summary>
        public static string NonStandard {
            get {
                return ResourceManager.GetString("NonStandard", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Standard.
        /// </summary>
        public static string Standard {
            get {
                return ResourceManager.GetString("Standard", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Watch Only.
        /// </summary>
        public static string WatchOnly {
            get {
                return ResourceManager.GetString("WatchOnly", resourceCulture);
            }
        }
    }
}
