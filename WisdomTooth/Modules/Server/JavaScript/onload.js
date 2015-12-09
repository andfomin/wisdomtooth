
function findHead() {
  var heads = document.getElementsByTagName('head');
  return heads.length > 0 ? heads[0] : null;
}

function findFeeds(head) {
  var feeds = [];
  var links = head.getElementsByTagName('link');
  for (var i = 0; i < links.length; i++) {
    var l = links[i];
    if ((l.getAttribute('rel') == 'alternate') && l.getAttribute('type') && l.getAttribute('href')) {
      var t = l.getAttribute('type').toLowerCase();
      if ((t.indexOf('rss') >= 0) || (t.indexOf('atom') >= 0) || (t.indexOf('rdf') >= 0) || (t.indexOf('text/xml') >= 0)) {
        feeds.push(l.getAttribute('href'));
      }
    }
  }
  return feeds;
}

function findBase(head) {
  var bases = head.getElementsByTagName('base');
  return bases.length > 0 ? bases[0].getAttribute('href') : null;
}

function main() {
  var xe = [];
  xe.push(xElem('title', xEsc(document.title)));
  xe.push(xElem('url', xEsc(document.URL)));
  if (document.URL != 'about:blank') xe.push(xElem('domain', xEsc(document['domain'])));

  var head = findHead();
  if (head) {
    xe.push(xElem('base', xEsc(findBase(head))));

    var xf = [];
    var feeds = findFeeds(head);
    for (var i in feeds) {
      xf.push(xElem('feed', xEsc(feeds[i])));
    }
    var a = {src:'head'}
    xe.push(xElem('feeds', xf.join(''), a));
  }

  var a = {type:'8789DA10-7140-4707-A56A-8D5EB7896D59'}
  return xElem('message', xe.join(''), a);
}
