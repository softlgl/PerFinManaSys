//添加cookie
//name=cookie名称
//value=cookie值
//expiresHours=过期时间
//document.cookie = "name=value;domain=.google.com";
//这样，所有google.com下的主机都可以访问该cookie。
//例如：www.google.com和gmail.google.com就是两个不同的主机名。
function addCookie(name, value, expiresHours,path) {
    var cookieString = name + "=" + escape(value);
    //判断是否设置过期时间 
    if (expiresHours > 0) {
        var date = new Date();
        date.setTime(date.getTime + expiresHours * 3600 * 1000);
        cookieString = cookieString + "; expires=" + date.toGMTString();
    }
    if (path != null) {
        cookieString += ";path=/"+path;
    }
    document.cookie = cookieString;
}
//获取cookie
//name=cookie名称
function getCookie(name) {
    var strCookie = document.cookie;
    var arrCookie = strCookie.split("; ");
    for (var i = 0; i < arrCookie.length; i++) {
        var arr = arrCookie[i].split("=");
        if (arr[0] == name) return arr[1];
    }
    return "";
}
//删除cookie
//name=cookie名称
function deleteCookie(name) {
    var date = new Date();
    date.setTime(date.getTime() - 10000);
    document.cookie = name + "=v; expires=" + date.toGMTString();
}