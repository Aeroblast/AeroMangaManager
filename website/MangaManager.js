//返回对象数组，对象包含id和title
function GetList()
{
    xmlhttp=new XMLHttpRequest();
    xmlhttp.open("GET","api/GetList",false);
    xmlhttp.send();
    var txt=xmlhttp.responseText;
    var lst=txt.split("\n");
    var objList=new Array();
    lst.forEach(element => {
        var t=element.split(':');
        if(t.length==2)
        {
            var obj=new Object();
            obj.id=t[0];
            obj.title=t[1];
            objList.push(obj);
        }

    });
    return objList;
}
//关闭系统
function ShutDown()
{
    xmlhttp=new XMLHttpRequest();
    xmlhttp.open("GET","api/ShutDown",false);
    xmlhttp.send();
    var txt=xmlhttp.responseText;
    alert(txt);
    window.close();
}

//返回对象，包含result(Success或Fail或Busy)和message(漫画标题或错误信息)。
function StartLoadManga(id)
{
    xmlhttp=new XMLHttpRequest();
    xmlhttp.open("GET","api/LoadManga_"+id,false);
    xmlhttp.send();
    var result=JSON.parse(xmlhttp.responseText);
    return result;
}

//返回对象，包含result(Success或Fail或Loading)和message(已经解压的页数)。
function CheckManga()
{
    xmlhttp=new XMLHttpRequest();
    xmlhttp.open("GET","api/CheckManga",false);
    xmlhttp.send();
    var result=JSON.parse(xmlhttp.responseText);
    return result;
}