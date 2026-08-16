using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Application;

/// <summary>AppSettingId is the aggregate id, but callers look settings up by the atlas's own natural key (UQ_CorePlatform_AppSetting_Key), not the surrogate id.</summary>
public interface IAppSettingFinder
{
    Task<AppSetting?> FindByKeyAsync(string key, CancellationToken cancellationToken);
}
