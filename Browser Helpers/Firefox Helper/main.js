// This is an active module of the Media Curator Helper Add-on

// Import the APIs we need.
////var data = require("self").data;
var pageMod = require("page-mod");
var request = require("request");

require("widget").Widget({
    id: "widgetID1",
    label: "My Mozilla Widget",
    contentURL: "http://www.mozilla.org/favicon.ico"
});

// Filter out iframes. https://bugzilla.mozilla.org/show_bug.cgi?id=684047
//if (unsafeWindow.self == unsafeWindow.top) {
var contentScript = 
"if (window.frameElement === null) { self.postMessage('{\"messageType\":\"{DA9138A9-3C9D-407E-978D-CD03BF0062EA}\"}'); }"+
"self.on(\"message\", function (message) { self.postMessage( eval(message)() ); });";

function generateGuid() {
    var S4 = function() {
        return (((1+Math.random())*0x10000)|0).toString(16).substring(1);
    };
    return (S4()+S4()+"-"+S4()+"-"+S4()+"-"+S4()+"-"+S4()+S4()+S4());
}

function exchangeMessages(documentId, requestText, callback) {
    var client = request.Request({
        url: "http://miner/mediacurator/helper/",
        contentType: "application/json",
        headers: {"X-MediaCurator-DocumentId": documentId},
        content: requestText,
        onComplete: function(response) {
            var responseText = response.text;
            if (response.status == 200 && responseText) {
                callback(responseText);
            }
        }
    });
    
    client.post();
};

pageMod.PageMod({
    include: "*",
    contentScript: contentScript,
    /* contentScriptFile: data.url("contentscript.js"), */
    // Load content scripts once all the content (DOM, JS, CSS, images) for the page has been loaded, at the time the window.onload event fires.
    contentScriptWhen: "end",
    onAttach: function(worker) {
        worker.documentId = generateGuid();        
        var callback = function(responseText) {
            worker.postMessage(responseText);    
        }        
        worker.on("message", function(requestText) {
            exchangeMessages(worker.documentId, requestText, callback);
        });
    }
});
