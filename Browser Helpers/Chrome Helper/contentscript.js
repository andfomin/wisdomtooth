function exchangeMessages(documentId, data, callback) {
    var client = new XMLHttpRequest();
    // The URL fetched here must be matched by a permission in the manifest.json file.
    client.open("POST", "http://miner/mediacurator/helper/");
    client.setRequestHeader("Content-Type", "application/json; charset=UTF-8");
    client.setRequestHeader("X-MediaCurator-DocumentId", documentId);
    client.send(data);
    client.onreadystatechange = function() {
        if (client.readyState == 4 && client.status == 200) {
            var responseText = client.responseText;
            if (responseText) {
                callback(responseText);
            }
        }
    }
};

function ececuteScript(script) {
    var result = eval(script)();
    ExchangeMessages(documentId, result, EcecuteScript);
}

function generateGuid() {
    var S4 = function () {
        return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
    };
    return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
};
var documentId = generateGuid();

exchangeMessages(documentId, '{"messageType":"{DA9138A9-3C9D-407E-978D-CD03BF0062EA}"}', ececuteScript);

