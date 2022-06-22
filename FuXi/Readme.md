#伏羲 (FuXi)
版本：1.0.0

说明：AssetBundle 资源管理 和 加载 插件

##功能
1.操作简单易上手, 单配置文件, 功能丰富

2.AssetBundle打包, 自动分析, 零冗余

3.支持分包, 配置方便, 分包下载方便

4.提供同步异步加载接口, 支持 Task 异步 和协程异步

5.内置 加密, 可选 Offset 偏移加密, XOR 全字节 异或加密(查看注意事项), 另提供 加密拓展接口

6.支持 资源 引用 动态分析(未完成)

##简单使用

````
FxManager 全局管理器

//启动 FuXi, 当前初始化方法 必须调用 **
await FxManager.FxLauncherAsync(RuntimeMode);

//检查 版本更新
await FxManager.FxCheckUpdate()
//获取更新大小
var download = await FxManager.FxCheckDownloadSize(true);
//下载资源
await FxManager.FxCheckDownload(download, a =>
{
    form.UpdateHandle(a,$"正在下载: {a}");
});

````
详细可查看 FxManager

````

FxAsset 加载 资源
FxScene 加载 场景
FxRawAsset 加载原生文件

````

##注意事项

1.加密方式为 XOR 时,不支持内置Bundle文件到安装包内, 主要是XOR加密方式需以文件流形式读取解密,
StreamingAssets文件夹 不支持相关操作! 如需 XOR 加密, Bundle 文件需先下载 后使用.




    
    