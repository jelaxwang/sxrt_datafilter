
String.prototype.isIn = function (stringArray) {
    return stringArray.indexOf(this) > -1;
}

String.prototype.isNotIn = function (stringArray) {
    return stringArray.indexOf(this) <= -1;
}

String.prototype.isInteger = function () {
    if (!this) return false;
    var strP = /^-?\d+$/;
    if (!strP.test(this)) return false;
    try {
        if (window.parseInt(this) != this)
            return false;
    }
    catch (ex) {
        return false;
    }
    return true;
}

String.prototype.isFloat = function () {
    if (!this) return false;
    var strP = /^-?\d+(\.\d+)?$/;
    if (!strP.test(this)) return false;
    try {
        if (window.parseFloat(this) != this)
            return false;
    }
    catch (ex) {
        return false;
    }
    return true;
}

Array.prototype.indexOf = function (substr, start) {
    var ta, rt, d = '\0';
    if (start != null) { ta = this.slice(start); rt = start; } else { ta = this; rt = 0; }
    var str = d + ta.join(d) + d, t = str.indexOf(d + substr + d);
    if (t == -1) return -1; rt += str.slice(0, t).replace(/[^\0]/g, '').length;
    return rt;
}

Array.prototype.lastIndexOf = function (substr, start) {
    var ta, rt, d = '\0';
    if (start != null) { ta = this.slice(start); rt = start; } else { ta = this; rt = 0; }
    ta = ta.reverse(); var str = d + ta.join(d) + d, t = str.indexOf(d + substr + d);
    if (t == -1) return -1; rt += str.slice(t).replace(/[^\0]/g, '').length - 2;
    return rt;
}

Array.prototype.replace = function (reg, rpby) {
    var ta = this.slice(0), d = '\0';
    var str = ta.join(d); str = str.replace(reg, rpby);
    return str.split(d);
}
