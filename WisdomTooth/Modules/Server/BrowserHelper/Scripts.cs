using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace MediaCurator.Server.BrowserHelper
{
    class Scripts
    {
        // The script have to be an Immediately-Invoked Function Expression (IIFE) which returns a function which in turn returns a string.
        // For example, "(function(){return function(){ return 'abc'; };})();"
        public const string ScriptBegin = @"(function(){ return function(){

";
        public const string ScriptEnd = @"
};})();";

        /*
         * Microsoft chose to create versioned features of the JScript engine for Internet Explorer 8.
         * For IE5 and IE7 document modes, the JScript engine acts as it did in the actual Internet Explorer 7, 
         * complete with all deviations from ECMAScript 3. 
         * The native JSON object is only present when the page is running in IE8 document mode. The JSON object in IE5 or IE7 document mode is undefined.
         * Some pages with unproper DOCTYPE directive fall into the IE7 mode.
         * +http://www.nczonline.net/blog/2010/02/02/how-internet-explorer-8-document-mode-affects-javascript/
         */

        // json-sitepoint-min.js copied from +http://www.sitepoint.com/javascript-json-serialization/ . (C) By Craig Buckler
        // The canonical implementation is json2.js at +https://github.com/douglascrockford/JSON-js/blob/master/json2.js
        // Both scripts are slightly edited and minified by Google Closure Compiler at +http://closure-compiler.appspot.com/home.

        ////public static string JsonSitepointScript
        ////{
        ////    get { return MediaCurator.HttpServer.Properties.Resources.json_sitepoint_min; }
        ////}

        public static string XmlElemScript
        {
            get { return MediaCurator.Server.Properties.Resources.xml_elem; }
        }

        // ("domain" in document) returns "true" in IE8, but an error occurs on attempt to get the value. The protocol is also unaccessable.
        public static string OnLoadScript
        {
            get { return MediaCurator.Server.Properties.Resources.onload; }
        }

        public const string ScriptToExecute = @"
return main();
";
        ////System.Web.HttpUtility.JavaScriptStringEncode

        ////public static string GetJson2Script0()
        ////{
        ////    Assembly assembly = Assembly.GetExecutingAssembly();
        ////    // "namespace.foldername.filename.ext"
        ////    Stream stream = assembly.GetManifestResourceStream(
        ////        "MediaCurator.HttpServer.JavaScript.json2-min.js");
        ////    StreamReader reader = new StreamReader(stream);
        ////    return reader.ReadToEnd();
        ////}

    }
}
