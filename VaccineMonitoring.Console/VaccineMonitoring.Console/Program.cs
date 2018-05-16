namespace VaccineMonitoring.Console
{
    public static class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            new SyncManager().Start();
            System.Console.ReadLine();
        }
    }
}