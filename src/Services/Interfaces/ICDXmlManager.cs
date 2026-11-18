using XperienceCommunity.Manager.Options;

namespace XperienceCommunity.Manager.Services.Interfaces
{
    /// <summary>
    /// Contains methods for managing Continuous Deployment XML configuration files.
    /// </summary>
    public interface ICDXmlManager : IService
    {
        /// <summary>
        /// Loads the provided Continuous Deployment XML file.
        /// </summary>
        public Task<RepositoryConfiguration?> GetConfig(string path);


        /// <summary>
        /// Saves the provided configuration to a physical XML file.
        /// </summary>
        public void WriteConfig(RepositoryConfiguration config, string path);
    }
}
