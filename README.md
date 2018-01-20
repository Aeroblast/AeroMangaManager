# Aero Manga Manager
简述
--
通过浏览器管理压缩包形式的漫画资源，进行临时的解压浏览。

建立简单的HTTP服务器与浏览器通讯

之前的桌面软件[AeroMangaViewer](https://github.com/Aeroblast/AeroMangaViewer)不会再继续了（很早就只用不改了）

当前测试环境：编译.Net Core 2.0.3 。运行系统Windows 10。浏览器测试环境Chrome 63.

感谢原生C#压缩包库[SharpCompress](https://github.com/adamhathcock/sharpcompress)

To-do:
--
找Bug（废话）

前端优化（自动播放，移动端适配）

标题筛选（根据现有标题过滤及排列）

Drag & Drop（扔进本地命令行……？得特地敲回车有点不舒服……）

History:
--
2018/01/15:肝了三四天基本能跑了。还有考试先停一下……已经实现：HTTP服务器;解压（有密码的）;基本查询API;简单网页
2018/01/20:大幅度前端优化,异步浏览。少量细节改进。
