using Nexus1.BuildingBlocks.Application;

namespace Nexus1.CorePlatform.Application;

/// <summary>The atlas's own defining behavior for AppSetting (C.1.4.2): "Runtime values that can be changed without redeploying the application."</summary>
public sealed record UpdateAppSettingValueCommand(string Key, string NewValue) : ICommand;
