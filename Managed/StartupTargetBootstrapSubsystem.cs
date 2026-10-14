using ArisenEngine.Core.Assets;
using ArisenEngine.Core.ECS;
using ArisenEngine.ECS.Lifecycle;
using ArisenEngine.Resources.Serialization;
using ArisenKernel.Diagnostics;
using ArisenKernel.Lifecycle;
using ArisenKernel.Packages;

namespace ArisenEngine.App;

/// <summary>
/// Activates the process's startup target before the first frame: the command-line selection when
/// the process names one, otherwise the workspace manifest's. A world wins over a scene, matching
/// the manifest's world-first bootstrap; a process that names no target at all is a defect and
/// fails the boot loudly.
/// </summary>
public sealed class StartupTargetBootstrapSubsystem : IEngineSubsystem
{
    public int Priority => 50;
    public EnginePhase InitPhase => EnginePhase.PostInit;

    public void Initialize()
    {
        EngineKernel kernel = EngineKernel.Instance;
        var sceneSubsystem = kernel.GetSubsystem<SceneSubsystem>()
            ?? throw new InvalidOperationException("Application shell startup requires SceneSubsystem.");
        var project = kernel.Services.GetService<ProjectSubsystem>().ActiveProject
            ?? throw new InvalidOperationException(
                "Application shell startup requires a loaded workspace manifest.");

        StartupTargetOptions selection = kernel.Config?.StartupTarget ?? StartupTargetOptions.None;
        ResolvedStartupTarget target = selection.Resolve(project);
        if (!target.HasTarget)
        {
            throw new InvalidOperationException(
                "The workspace declares no startup target. Declare StartupScene or StartupWorld in " +
                "the manifest, or pass --startup-scene/--startup-world.");
        }

        sceneSubsystem.RegisterSystem(new MeshSystem());

        if (target.World is { } startupWorld)
        {
            var worldService = kernel.Services.GetService<IRuntimeWorldStreamingService>();
            RuntimeWorldLoadResult worldResult = worldService.LoadWorld(
                new AssetRef<WorldSourceAsset>(
                    startupWorld.Guid,
                    "World",
                    startupWorld.PackageId));
            if (!worldResult.Success)
            {
                throw new InvalidOperationException(worldResult.Diagnostic);
            }

            KernelLog.InfoFormat(
                worldResult.Deferred
                    ? "[ApplicationShell] Deferred startup world '{0}' until persistent residency is ready ({1} streamable cells)."
                    : "[ApplicationShell] Activated startup world '{0}' with {1} streamable cells.",
                worldResult.WorldGuid,
                worldResult.CellCount);
            return;
        }

        ProjectAssetReference startupScene = target.Scene!;
        var sceneService = kernel.Services.GetService<IRuntimeSceneService>();
        var sceneRef = new AssetRef<SceneSourceAsset>(
            startupScene.Guid,
            "Scene",
            startupScene.PackageId);
        SceneLoadResult result = sceneService.LoadScene(sceneRef);
        if (!result.Success)
        {
            throw new InvalidOperationException(result.Diagnostic);
        }

        KernelLog.InfoFormat(
            "[ApplicationShell] Activated startup scene '{0}' ({1}) with {2} entities.",
            result.SceneName,
            startupScene.Guid,
            result.EntityCount);
    }

    public void Shutdown()
    {
    }

    public void Dispose()
    {
    }
}
