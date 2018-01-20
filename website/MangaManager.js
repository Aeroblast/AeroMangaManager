var GetList;//传入方法，执行参数为对象数组，对象包含id和title
var ShutDown;//关闭系统
var StartLoadManga;//传入ID和方法，执行参数为对象，包含result(Success或Fail或Busy)和message(漫画标题或错误信息)。
var CheckManga;//传入方法，执行参数为对象，包含result(Success或Fail或Loading)和message(已经解压的页数)。
GetList = function (func) {
    xmlHttp = new XMLHttpRequest();
    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4) {
            var responseText = xmlHttp.responseText;
            var txt = xmlHttp.responseText;
            var lst = txt.split("\n");
            var objList = new Array();
            lst.forEach(element => {
                var t = element.split(':');
                if (t.length == 2) {
                    var obj = new Object();
                    obj.id = t[0];
                    obj.title = t[1];
                    objList.push(obj);
                }

            });
            func(objList);
        }
    }
    xmlHttp.open("GET", "api/GetList", true);
    xmlHttp.send();
};

ShutDown = function () {
    xmlHttp = new XMLHttpRequest();
    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4) {
            var txt = xmlHttp.responseText;
            alert(txt);
            window.close();
        }
    }
    xmlHttp.open("GET", "api/ShutDown", true);
    xmlHttp.send();
};

StartLoadManga = function (id,func) {
    xmlHttp = new XMLHttpRequest();
    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4) {
            var result = JSON.parse(xmlHttp.responseText);
            func(result);
        }
    }
    xmlHttp.open("GET", "api/LoadManga_" + id, true);
    xmlHttp.send();
};

CheckManga = function (func) {
    xmlHttp = new XMLHttpRequest();
    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4) {
            var result = JSON.parse(xmlHttp.responseText);
            func(result);
        }
    }
    xmlHttp.open("GET", "api/CheckManga", true);
    xmlHttp.send();

};