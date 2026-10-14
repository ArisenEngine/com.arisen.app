using ArisenKernel.Diagnostics;
using ArisenKernel.Packages;
using ArisenKernel.Services;

namespace ArisenEngine.App;

/// <summary>
/// The application shell every product depends on. It owns what a product must not repeat: booting
/// the process's startup target and the interactive viewer that makes the target visible. A product
/// adds what only it knows - gameplay, content selection, rendering choices.
/// </summary>
public sealed class ApplicationShellPackage : IPackageEntry
{
    public void OnLoad(IServiceRegistry services)
    {
        KernelLog.Info("[ApplicationShell] Package loaded.");
    }

    public void OnUnload(IServiceRegistry services)
    {
        KernelLog.Info("[ApplicationShell] Package unloaded.");
    }
}
