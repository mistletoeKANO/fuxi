## 安装 伏羲(FuXi)

1. Open Edit/Project Settings/Package Manager
2. Add a new Scoped Registry (or edit the existing OpenUPM entry)
3.  Name: FuXi

    URL:  https://package.openupm.cn
    
    Scope(s): com.tendo.fuxi
4. Click Save (or Apply)
5. Open Window/Package Manager
6. Change Packages to My Registries
7. Install FuXi

### 或者 拷贝以下内容 到 Packages/manifest.json

````
{
    "scopedRegistries": [
        {
            "name": "package.openupm.cn",
            "url": "https://package.openupm.cn",
            "scopes": [
                "com.tendo.fuxi"
            ]
        }
    ],
    "dependencies": {
        "com.tendo.fuxi": "1.1.2"
    }
}
````

# 从 0 开始使用

### 新增配置, 配置参数说明

1. Project 视图 下 Create/FuXi Asset/FuXi Asset

![Snipaste_2022-06-22_21-48-23](https://user-images.githubusercontent.com/33541704/175045268-e6c5381b-d3bf-43ee-839d-7602c5f3f755.png)

3. FuXiAsset 为 主配置, 包含版本文件列表, 包含 需要 动态加载的 所有 需要热更新的 资源. 被依赖资源 可选择添加, 未添加资源 会被自动打包. 
4. Builtin 为分包配置文件, 可 新增多个, 并 按照分包 接口 单独下载; 分包资产 包含的是 分包 文件 或者文件夹. 
5. Settings 文件 是 相关设置, [资源根路径: 热更文件夹根路径, 打包时会被 自动剔除出 Bundle 包名, 减少包名长度], [配置所属平台: 当前配置文件 所属的 平台, 游戏运行时会根据当前设置选取对应平台配置 初始化 可加载文件列表], [加密类型: 设置加密文件类型, 默认 不加密, 可选 字节偏移加密 或者 全字节异或加密, 或者自定义加密类型]; 勾选 拷贝 全部 Bundle 到安装包 后 打包时 会自动 拷贝所有Bundle 到StreamingAssets 文件夹下; 忽略文件列表 包含 不打包的文件名后缀; [首包包含分包: 可添加 分包配置, 构建安装包时, 会拷贝 已添加分包文件 关联 Bundle 包到安装包内].

### 配置截图说明

FuXiAsset 主配置

![Snipaste_2022-06-23_14-10-22](https://user-images.githubusercontent.com/33541704/175227726-0dbb19ba-1740-45c4-bf1b-dadc990dd107.png)

### 代码启动流程

1. (**必须**) 游戏入口处 调用 启动资源管理器接口, 包含三个参数, 1: 版本管理配置文件名称(上一步新增配置文件名称: 如: FuXiAsset); 2: 资源服务器下载地址; 3: 游戏运行模式, 当前共 三种模式, 分别为 Editor 编辑器下、Offline 离线模式、RunTime 热更新 下载模式
````
await FxManager.FxLauncherAsync("FuXiAssetWindow", "http://192.168.1.2/Windows/", RuntimeMode);
````
2. 检查 版本更新 
````
await FxManager.FxCheckUpdate(f =>
{
    form.UpdateHandle(0, $"检查更新:{f}");
});
````
3. 获取更新 列表, 返回 DownloadInfo 包含 文件下载大小 和 文件下载列表
````
var download = await FxManager.FxCheckDownloadSize(true);
````
4. 下载 资源
````
if (download.DownloadSize > 0)
{
    GameDebugger.Log($"检测到版本变更, 大小:{download.FormatSize}");
    await FxManager.FxCheckDownload(download, a =>
    {
        form.UpdateHandle(a,$"正在下载: {a}");
    });
    GameDebugger.Log("下载完成!");
}
````

## 加载资源 

1. FxAsset 加载 资源, 相关接口 自行查看
``
FxAsset fxAsset = await FxAsset.LoadAsync<GameObject>(path);
``
3. FxScene 加载 场景
``
await FxScene.LoadSceneAsync(scenePath, additive);
``
5. FxRawAsset 加载 原生文件



