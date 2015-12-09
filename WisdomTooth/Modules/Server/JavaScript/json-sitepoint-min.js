//Custom JSON.stringify begin
var JSON;
JSON || (JSON = {});
JSON.stringify = JSON.stringify || function(a) {
  var c = typeof a;
  if("object" != c || null === a) {
    return"string" == c && (a = '"' + a + '"'), "" + a
  }
  var d, b, f = [], e = a && a.constructor == Array;
  for(d in a) {
    b = a[d], c = typeof b, "string" == c ? b = '"' + b + '"' : "object" == c && null !== b && (b = JSON.stringify(b)), f.push((e ? "" : '"' + d + '":') + ("" + b))
  }
  return(e ? "[" : "{") + ("" + f) + (e ? "]" : "}")
};
//Custom JSON.stringify end