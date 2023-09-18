namespace iSun.Workshop.Persistence.Configuration
{
    public class StorageConfiguration
    {
        public StorageConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }
    }
}
