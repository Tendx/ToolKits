# Klick
将右Alt键改成鼠标左键，方便笔记本电脑操作。  
使用方式：  
+ 开始菜单运行shell:startup，将编译好的文件放到目录里，就能开机启动了  

# RDBan
限制用户远程桌面登录（比如之允许RemoteApp连接）。程序在登陆的时候执行，如果是远程登陆，执行登出动作。  
使用方式：
1. 新建用户组，比如 *Remote Desktop Ban* ，将需要限制的用户放到该组
2. 编写脚本，将组名传参给程序，比如 *RDBan.bat* ，内容为：  
`start /w C:\RDBan.exe "Remote Desktop Ban"`
3. *gpedit.msc* 打开组策略，定位到：用户配置->Windows 设置->脚本(登陆/注销)，【登陆】里面添加该脚本