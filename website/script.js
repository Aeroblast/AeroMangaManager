function loadList() {
    clearList();
    GetList(function (lst) {
        var c = document.getElementById("list");
        lst.forEach(e => {
            var l = document.createElement("div");
            l.className = "button mangaTitle";
            l.onclick = function () { loadManga(e.id) };
            l.innerHTML = e.title;
            c.appendChild(l);
        });
        c.style.visibility="visible";
    });

}
function clearList() {
    var c = document.getElementById("list");
    var ls = c.getElementsByTagName("div");
    for (var a = ls.length - 1; a >= 0; a--) {
        c.removeChild(ls[a]);
    }
    c.style.visibility="hidden";
}

function loadManga(id) {
    clearList();
    document.getElementById("list").style.zIndex = -5;
    StartLoadManga(id, function (obj) {
        console.log(obj.message);
        v0_start();
    });
}