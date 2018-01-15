function loadList()
{
var lst=GetList();
var c=document.getElementById("list");
lst.forEach(e => {
    var l=document.createElement("div");
    l.className="MangaTitle";
    l.onclick=function(){loadManga(e.id)};
    l.innerHTML=e.title;
    c.appendChild(l);
});
}
var handle;
function loadManga(id)
{
    var obj=StartLoadManga(id);
    console.log(obj.message);
    clearInterval(handle);
    handle=setInterval(checkState,500);
}
function checkState()
{
    var obj=CheckManga();
    if(obj.result=="Success")
    {
        
        clearInterval(handle);
        var body=document.getElementsByTagName("body")[0];
        var imgs=document.getElementsByTagName("img");
        for(var i=imgs.length-1;i>=0;i--)
        {
            body.removeChild(imgs[i]);
        }
        for(var i=0;i<parseInt(obj.message);i++)
        {
            var c=document.createElement("img");
            c.src="/api/GetPage_"+i+"_"+obj.message;
            body.appendChild(c);
        }
    }
}