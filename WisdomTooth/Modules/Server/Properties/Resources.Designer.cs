﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MediaCurator.Server.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MediaCurator.Server.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to 
        ///function findHead() {
        ///  var heads = document.getElementsByTagName(&apos;head&apos;);
        ///  return heads.length &gt; 0 ? heads[0] : null;
        ///}
        ///
        ///function findFeeds(head) {
        ///  var feeds = [];
        ///  var links = head.getElementsByTagName(&apos;link&apos;);
        ///  for (var i = 0; i &lt; links.length; i++) {
        ///    var l = links[i];
        ///    if ((l.getAttribute(&apos;rel&apos;) == &apos;alternate&apos;) &amp;&amp; l.getAttribute(&apos;type&apos;) &amp;&amp; l.getAttribute(&apos;href&apos;)) {
        ///      var t = l.getAttribute(&apos;type&apos;).toLowerCase();
        ///      if ((t.indexOf(&apos;rss&apos;) &gt;= 0) || (t.indexOf(&apos;atom&apos;) &gt;= 0)  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string onload {
            get {
                return ResourceManager.GetString("onload", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to function xElem(e, v, dic) {
        ///  var aa = &apos;&apos;;
        ///  if (dic) { for (var a in dic) { aa += &apos; &apos; + a + &apos;=&quot;&apos; + dic[a].split(&apos;&quot;&apos;).join(&apos;&amp;quot;&apos;) + &apos;&quot;&apos; } }
        ///  return v ? &apos;&lt;&apos; + e + aa + &apos;&gt;&apos; + v + &apos;&lt;/&apos; + e + &apos;&gt;&apos; : &apos;&lt;&apos; + e + aa + &apos;/&gt;&apos;
        ///}
        ///
        ///function xEsc(s) {
        ///  return s ? s.split(&apos;&amp;&apos;).join(&apos;&amp;amp;&apos;).split(&apos;&lt;&apos;).join(&apos;&amp;lt;&apos;).split(&apos;&gt;&apos;).join(&apos;&amp;gt;&apos;) : s;
        ///}
        ///.
        /// </summary>
        internal static string xml_elem {
            get {
                return ResourceManager.GetString("xml_elem", resourceCulture);
            }
        }
    }
}
