# 伏羲 (FuXi) 

[![License](https://img.shields.io/github/license/mistletoeKANO/fuxi)]([https://github.com/tuyoogame/YooAsset/blob/master/LICENSE](https://github.com/mistletoeKANO/fuxi-example/blob/main/LICENSE))[![openupm](https://img.shields.io/npm/v/com.mistletoeKANO.fuxi?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.cn/packages/com.mistletoeKANO.fuxi/)

版本：1.0.0

说明：AssetBundle 资源管理 和 加载 插件

## 功能 
1.操作简单易上手, 单配置文件, 方便管理

2.AssetBundle打包, 自动分析, 零冗余

3.支持分包, 配置方便, 分包下载方便

4.提供同步异步加载接口, 支持 Task 异步 和协程异步

5.内置 加密, 可选 Offset 偏移加密, XOR 全字节 异或加密(查看注意事项), 另提供 加密拓展接口

6.支持 资源 引用 动态分析(未完成)

## 注意事项

1.加密方式为 XOR 时,不支持内置Bundle文件到安装包内, 主要是XOR加密方式需以文件流形式读取解密, StreamingAssets文件夹 不支持相关操作! 如需 XOR 加密, Bundle 文件需先下载 后使用.
