# UniFTP

基于C#的简易FTP库，支持FTPS、IPv6等大部分FTP协议扩展功能，可通过mono跨平台。

## 组件

* UniFTP.Server 基于SharpServer的FTP Server库
* UniFTP#Server 基于UniFTP.Server的FTP服务器软件
* UniFTP.Client FTP Client库（Planning）

### 注意

本项目与[uniFTP](http://linux.softpedia.com/get/Internet/FTP/uniFTP-28869.shtml)（当前已经更名[CompleteFTP](http://enterprisedt.com/products/completeftp/)）无任何关联。本项目并不使用edtFTP组件。

by Ulysses

![CC 4.0 - BY](https://licensebuttons.net/l/by/4.0/88x31.png)

---
UniFTP is a FTP server lib & software written in C#. It's based on Sharp FTP Server, but have been rewritted and enhanced.

Many bugs in SharpServer have been fixed, such as encoding, path parsing, performance counters, FTPS and so on.

Many new functions are added, such as cross-platform(Mono) support, FTPS support, RESTart support, new usergroup, log and virtual file system(I spend most time on them). 

LICENSE: [CC-4.0-BY](http://creativecommons.org/licenses/by/4.0/)

