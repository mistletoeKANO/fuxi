# 伏羲 (FuXi) 

[![License](https://img.shields.io/github/license/mistletoeKANO/fuxi)]([https://github.com/tuyoogame/YooAsset/blob/master/LICENSE](https://github.com/mistletoeKANO/fuxi-example/blob/main/LICENSE))[![openupm](https://img.shields.io/npm/v/com.tendo.fuxi?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.cn/packages/com.tendo.fuxi/)

AssetBundle 资源管理 和 加载 插件

## 功能 
1.操作简单易上手, 单配置文件, 可添加多个配置 管理不同平台, 方便管理

2.一键打包, 自动分析冗余

3.支持分包, 配置方便, 分包下载方便

4.提供同步异步加载接口, 支持 Task 异步 和 Coroutine 异步

5.内置 加密, 可选 Offset 偏移加密, XOR 全字节 异或加密(查看注意事项), 另提供 加密拓展接口

6.支持全量更新 和 分包更新, 多线程下载

7.支持 资源 引用 分析(未完成)

## 配置截图

1.项目资源配置, 热更 动态加载 资源列表

![asset](https://user-images.githubusercontent.com/33541704/173237430-d204dbb2-2ff6-441b-b28b-126b09cf3ce5.png)

2.分包配置, 分包下载 每个 分包 包含的资源列表

![package](https://user-images.githubusercontent.com/33541704/173237445-e6782f72-926e-4f22-b5fc-c6271f25099f.png)

3.设置, 配置 内置 包, 打包 加密 相关配置

![setting](https://user-images.githubusercontent.com/33541704/173237455-789474a5-58a4-40b7-af7e-7df389052b35.png)

## 注意事项

1.加密方式为 XOR 时,不支持内置Bundle文件到安装包内, 主要是XOR加密方式需以文件流形式读取解密, StreamingAssets文件夹 不支持相关操作! 如需 XOR 加密, Bundle 文件需先下载 后使用.

