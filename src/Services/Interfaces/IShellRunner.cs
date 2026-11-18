using System.Diagnostics;

namespace XperienceCommunity.Manager.Services.Interfaces
{
    /// <summary>
    /// Contains methods for executing shell scripts.
    /// </summary>
    public interface IShellRunner : IService
    {
        /// <summary>
        /// Executes a script in a hidden shell with redirected streams.
        /// </summary>
        /// <returns>The script process.</returns>
        public Process Execute(ShellOptions options);
    }
}
