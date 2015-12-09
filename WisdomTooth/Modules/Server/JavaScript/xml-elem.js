function xElem(e, v, dic) {
  var aa = '';
  if (dic) { for (var a in dic) { aa += ' ' + a + '="' + dic[a].split('"').join('&quot;') + '"' } }
  return v ? '<' + e + aa + '>' + v + '</' + e + '>' : '<' + e + aa + '/>'
}

function xEsc(s) {
  return s ? s.split('&').join('&amp;').split('<').join('&lt;').split('>').join('&gt;') : s;
}
