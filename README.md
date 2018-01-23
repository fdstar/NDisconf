# NDisconf
disconf的NET版本，为与disconf.net区别，命名为NDisconf
（disconf.net也是本人开发，但因为当时是公司安排开发，现个人已从该公司离职，加上当初的disconf.net开发设计上均存在缺陷，所以现在个人重新开发）  
以下是初版开发计划：  
1、zk连接不再阻塞项目的启动（但从服务器数据抓取还是按配置确定是否会阻塞程序）  
2、优化zk重连机制，增加ConnectionLossException时相关操作重试   
3、支持net45+，netstandard2.0+  
4、增加灰度发布概念（方案已确定，还需试验可行性）  
5、增加配置更新版本概念（可能会增加，暂时还没详细方案）  
6、重新开发web部分（该部分在disconf.net中非本人开发）  
7、与disconf兼容，即不需要更新web，可以直接用ndisconf客户端连接disconf的web服务进行更新（但disconf.net的客户端不与ndisconf兼容，需修改连接代码）  
8、client配置本地持久化相关代码重构
