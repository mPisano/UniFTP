#UniFTP

基于C#的简易FTP库

##组件

* UniFTP.Server 基于SharpServer的FTP Server库
* UniFTP.Client FTP Client库

更多内容正在开发中...

###注意

本项目与[uniFTP](http://linux.softpedia.com/get/Internet/FTP/uniFTP-28869.shtml)（当前已经更名[CompleteFTP](http://enterprisedt.com/products/completeftp/)）无任何关联。本项目并不使用edtFTP组件。

by Ulysses

![署名-非商业性使用-相同方式共享 3.0 中国大陆](http://i.creativecommons.org/l/by-nc-sa/3.0/88x31.png)

---
UniFTP is a FTP server lib written in C#. It's based on Sharp FTP Server, but have been rewritted and enhanced.
Many bugs in SharpServer have been fixed, such as encoding, path parsing, performance counters, FTPS and so on.
Many new functions are added, such as cross-platform(Mono) support, FTPS support, RESTart support, new usergroup, log and virtual file system(I spend most time on them). 
I hope this banch would be more stable, safe and powerful.(though still throw an exception sometimes)
Notice that summarys are written in Chinese, but I think it's quite easy to figure out meaning by method names.

LICENSE: CC-3.0-BY-NC-SA
