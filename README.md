# SDL_Easyx

#### 介绍

基于 SDL# 实现的 Easyx C# API
依赖于SDL，SDL_Image，SDL_TTF

#### 安装

将Easyx.cs文件放入项目文件夹下即可。

#### 说明

在源代码的基础上修改了部分内容:
- 大写处理
- 将函数前带有 "get" 或 "set" 的函数替换为属性 (例如`getbkcolor`变为`BKColor`)
- 替换部分函数 (例如`GetHWnd`变为`GetWindow`)
- 删除无法实现的函数 (`setcliprgn`, `setorigin`等)
- 封装了`Getch`函数
您可以查阅[官方文档](https://docs.easyx.cn/zh-cn/)获取函数的使用说明

#### 使用

```csharp
using SDL_Easyx;
using SDL2;
class Program
{
    static void Main(string[] args)
    {
        Easyx.InitGraph(700,500);
        Easyx.BKcolor=Easyx.BLUE;
        while(true)
        {
            Easyx.ExMessage message;
            message.Get();
            if(message.message==SDL.SDL_EventType.SDL_QUIT)
            {
                Easyx.CloseGraph();
                break;
            }
            Easyx.Rectangle(100,100,300,300);
            Easyx.FlushBatchDraw();
        }
    }
}
```