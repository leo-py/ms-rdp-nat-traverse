# 微软远程控制 内网穿透助手
-------------
## 说明
* 一般来说，同在一个局域网下的两个Windows计算机可以正常使用微软的远程桌面实现远程登录。但是，当两台计算机网络环境相对复杂，尤其是不在同一局域网下，就需要使用别的手段来实现访问了。
* 本方案就是使用SSH连接到远程代理服务器，建立SSH反向隧道的方式实现无论在何种网络环境下，只要代理服务器工作正常，并且双方均可以与服务器正常交互，便可借助本客户端实现轻松实现远程控制。
* 应用开发分为客户端软件开发和服务器软件开发。

## 简要工作原理
* 在客户端中选中要进行远程控制的目标主机，在连接后，目标主机会收到远程控制请求；目标主机会自动建立反向隧道，将远程桌面端口(默认`3389`)映射到远程服务器的随机端口中。
* 由于使用了随机分配的隧道端口，并且，端口号不是常用的，相对比较安全（寡人是这么认为的）；况且本地在完成控制后，目标主机也会自动断开隧道。

## PC客户端软件
* 客户端软件使用Visual C#开发，运行在.Net 4.5及以上平台的系统上，理论上支持Windows 7及以上系统，但是要确保.Net版本符合要求，否则无法运行本客户端。
* 客户端实现中，借助于`plink.exe`（即`putty.exe`）的命令行版本实现免密码输入，建立反向SSH隧道的功能。
* 客户端提供的功能说明：
	* __注册__：客户端在启动后，点击"启动"按钮，客户端会自动尝试与代理服务器联系，并获取ID，同时在代理服务器上存储自身的信息；完成“注册”过程；
	* __远程列表__: 在“远程”页面中，点击“更新”按钮，可以拿取目前在线注册的计算机列表，将提供计算机ID和名称的显示；
	* __连接/断开__: 本机可以选择远程主机，点击“连接”按钮后申请远程控制；当完成控制后，可以点击“断开”按钮，要求远程主机关闭隧道。
* 主要使用到的第三方库：[Newtonsoft.json](https://github.com/JamesNK/Newtonsoft.Json)，有关该库的`license`请参见`Licese`目录

## Mac客户端软件
* 客户端基于`PyQt-5.5.1`使用`Python 3.5`开发，经过测试，基本功能正常；
* Mac客户端仅支持远程请求连接开放连接权限的主机；
* Mac客户端不支持Windows下的被远程控制的功能，仅仅是为了配合Mac下的`Microsoft Remote Desktop`软件实现内网穿透访问其他远程计算机。

## 服务器软件
* 服务器端采用了`Python 3`开发；
* 新版本使用了[Twisted 16](https://twistedmatrix.com/trac/)框架重新编写部分代码，增强了服务器端的稳定性；
* 服务器端程序会在启动后自动在指定端口（默认为9000）进行监听，并及时处理来自客户端的请求；
* 服务器端在运行前，可以在配置文件中更改日志文件名称，以及输出日志的等级；默认日志等级为`DEBUG`。日志等级可以选择的项目包括：
	* `DEBUG`
	* `INFO`
	* `ERROR`
	* `WARN`

## 如何运行
### 服务器端程序
1. 确保在服务器系统上（我使用的是`Cent OS 7`)已经成功安装了`Python 3`尽可能新的版本
2. 确保在服务器系统上为`Python 3`安装了`Twisted 16.0`相关模块，服务器程序依赖此库
3. 将`ProxyServer`的代码全部放置在某个目录下，如`ProxyServer`目录，使用`python3 server.py`即可运行
4. 可以在config文件中修改相关设置，根据需要修改

### PC客户端软件
1. 请在[这儿](https://github.com/ChrisLeeGit/MSRDPNatTraverse/releases)下载最新发布的`Realse`版本，将`RAR`格式的压缩包下载到电脑上，并解压；
2. 确保在Windows系统上安装了`.Net 4.5`及以上版本，双击即可运行软件；
3. 在运行后，需要根据需要配置代理服务器以及本机的设置，然后才可以点击启动按钮启动相关服务。其他功能比较简单，很容易使用；
4. 客户端运行截图：
	* ![截图0](https://raw.githubusercontent.com/ChrisLeeGit/MSRDPNatTraverse/master/ScreenShots/win7_1.png)
	* ![截图1](https://raw.githubusercontent.com/ChrisLeeGit/MSRDPNatTraverse/master/ScreenShots/win10_1.png)
	* ![截图2](https://raw.githubusercontent.com/ChrisLeeGit/MSRDPNatTraverse/master/ScreenShots/win10_2.png)
	* ![截图3](https://raw.githubusercontent.com/ChrisLeeGit/MSRDPNatTraverse/master/ScreenShots/win10_3.png)


### Mac客户端软件
1. 确保`PyQt-5`已经在Mac上安装成功；
2. 确保`Python 3`在Mac上安装成功；
3. 下载Mac版客户端代码，解压后在终端中进入解压后的目录，使用命令：`python3 App.py`即可运行；
4. 请确保下载了微软远程桌面软件(`Microsoft Remote Desktop`)，然后才能借助于客户端穿透内网访问其他远程计算机；
5. 客户端运行截图：
	* ![截图1](https://raw.githubusercontent.com/ChrisLeeGit/MSRDPNatTraverse/master/ScreenShots/mac_1.png)
	* ![截图2](https://raw.githubusercontent.com/ChrisLeeGit/MSRDPNatTraverse/master/ScreenShots/mac_2.png)

## 许可
* 本项目为个人业余项目，算不上专业，代码组织的也不是特别好，还在进一步学习改进中。所有本人编写的源码采用`MIT LICENSE`。
* Enjoy~
