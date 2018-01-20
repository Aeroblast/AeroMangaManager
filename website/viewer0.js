
var v0_maxPage = 0;
var v0_page = 0;
var v0_offset = 0;//in px, of first temp
var v0_beforeSum = 0;//of middle temp
var v0_tempCount = 5;
var v0_ctx;
var v0_handle;
var v0_width = 80;//in percent
var v0_imgs;
var canvas;
function v0_start() {
    canvas = document.getElementById("canvas");
    v0_ctx = canvas.getContext("2d");
    canvas.width = window.innerWidth * 2;
    canvas.height = window.innerHeight * 2;
    canvas.style.width = window.innerWidth + "px";
    canvas.style.height = window.innerHeight + "px";
    v0_ctx.scale(2, 2);
    v0_imgs = new Array(v0_tempCount);
    for (var i = 0; i < v0_tempCount; i++) { v0_imgs[i] = document.createElement("img"); }
    v0_handle = setInterval(v0_onloading, 500);
    window.addEventListener("resize", function () {
        canvas.width = window.innerWidth * 2;
        canvas.height = window.innerHeight * 2;
        canvas.style.width = window.innerWidth + "px";
        canvas.style.height = window.innerHeight + "px";
        v0_ctx.scale(2, 2);
        //to-do：offset影响
        v0_draw();
    });
    document.addEventListener('touchend', v0_touchend);
    document.addEventListener('touchmove', v0_touchmove);
    document.addEventListener('touchstart', v0_touchstart);
    document.addEventListener('mousewheel', function (evt) {
        var d = window.innerHeight / 10;
        if (evt.wheelDelta < 0) d = -d;
        v0_move(d);
    });
    document.addEventListener('keydown', function (evt) {
        var d = window.innerHeight / 10;
        switch (evt.keyCode) {

            case 38://Up
                v0_move(d);
                break;
            case 40:
                v0_move(-d);
                break;
            case 33://Page Up
                v0_move(d * 5);
                break;
            case 34://Page Down
            case 32://space
                v0_move(-d * 5);
                break;
        }
    });
}
var v0_lastY;
var v0_touchD;
var v0_slideHandle;
function v0_touchstart(evt) {
    clearInterval(v0_slideHandle);
    v0_lastY = evt.touches[0].pageY;
}
function v0_touchmove(evt) {
    v0_touchD = evt.touches[0].pageY - v0_lastY;
    v0_move(v0_touchD);
    v0_lastY = evt.touches[0].pageY;
}
function v0_touchend(evt) {
    if (Math.abs(v0_touchD) > 1) {
        v0_speed = v0_touchD;
        v0_slideHandle = setInterval(v0_speedDownSlide, 30);
    }
}
var v0_speed = 0;
function v0_speedDownSlide() {
    if (Math.abs(v0_speed) > 0.1) {
        v0_move(v0_speed);
        v0_speed *= 0.8;
    } else { clearInterval(v0_slideHandle) }
}

function v0_move(dis) {
    v0_offset += dis;
    var w = window.innerWidth * v0_width / 100;
    var y = v0_beforeSum + v0_offset;
    var mid = Math.floor(v0_tempCount / 2);
    var midh = w / v0_imgs[mid].width * v0_imgs[mid].height; assert(!isNaN(midh));
    if (v0_page == 0 && y > 0) {
        assert(v0_beforeSum <= 0.001);
        v0_offset = 0;
    }
    if (v0_page == v0_maxPage - 1 && y + midh < window.height) {
        v0_offset = window.innerHeight - v0_beforeSum - midh;
    }
    var mid1h = w / v0_imgs[mid + 1].width * v0_imgs[mid + 1].height;
    if (v0_page == v0_maxPage - 2 && y + midh + mid1h < window.height) {
        v0_offset = window.innerHeight - v0_beforeSum - midh - mid1h;
    }
    console.log("page " + v0_page + " offset " + y + "=" + v0_beforeSum + "+" + v0_offset);
    v0_draw();
    if (y > window.innerHeight) {
        if (v0_page > 0) {
            v0_page--;
            var t = v0_imgs.pop();
            t.remove();
            v0_imgs.unshift(document.createElement("img"));
            v0_imgs[0].onload = function () {
                var delta = 0;
                var ta = w / v0_imgs[0].width * v0_imgs[0].height;
                if (!isNaN(ta))
                    delta += ta;
                v0_offset -= delta;
                delta -= midh;
                v0_beforeSum += delta;
            };
            var a = v0_page - mid;
            v0_imgs[0].src = "/api/GetPage_" + a + "_" + v0_maxPage;
        }
        else {
            assert(false);
        }

    }
    if (y + midh < 0) {
        if (v0_page < v0_maxPage - 1) {
            v0_page++;
            var t = v0_imgs.shift();
            var l = w / t.width * t.height;
            var delta = 0;
            t.remove();
            if (!isNaN(l))
                delta -= l;
            v0_offset -= delta;
            var ta = w / v0_imgs[mid - 1].width * v0_imgs[mid - 1].height;
            assert(!isNaN(ta));
            delta += ta;
            v0_beforeSum += delta;
            v0_imgs.push(document.createElement("img"));
            var a = v0_page + mid;
            v0_imgs[v0_tempCount - 1].src = "/api/GetPage_" + a + "_" + v0_maxPage;
        }
        else {
            assert(false);
        }
    }
}
function v0_onloading() {
    CheckManga(function (obj) {
        v0_maxPage = parseInt(obj.message);
        if (obj.result == "Success") {
            clearInterval(v0_handle);
        }
        else if (obj.result == "Failed") {
            clearInterval(v0_handle);
            console.log("加载失败。");
        }
        else {
            if (v0_maxPage > v0_page + v0_tempCount / 2) {
                for (var i = 0; i < v0_tempCount; i++) {
                    var a = v0_page + i - Math.floor(v0_tempCount / 2);
                    if (a < 0) continue;
                    v0_imgs[i].onload = v0_draw;
                    v0_imgs[i].src = "/api/GetPage_" + a + "_" + obj.message;
                }
            }
            //to-do:update slider
        }
    });
}
function v0_draw() {
    var offsetSum = 0;
    var x = (100 - v0_width) / 200 * window.innerWidth;
    for (var i = 0; i < v0_tempCount; i++) {
        var y = v0_offset + offsetSum;
        var width = v0_width / 100 * window.innerWidth;
        var height = v0_imgs[i].height / v0_imgs[i].width * width;
        //if (i == Math.floor(v0_tempCount / 2))
        //  v0_beforeSum = offsetSum;
        if (!isNaN(height)) {
            offsetSum = offsetSum + height;
            // if (y + height < 0 || y > window.innerHeight) continue;
            v0_ctx.drawImage(v0_imgs[i], x, y, width, height);
        }

    }

}

function assert(a) {
    if (!a) { throw "err"; }
}