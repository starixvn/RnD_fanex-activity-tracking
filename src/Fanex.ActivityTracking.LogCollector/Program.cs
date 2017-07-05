namespace Fanex.ActivityTracking.LogCollector
{
    using System;
    using System.Reflection;
    using System.ServiceProcess;

    public class Program
    {
        private static void Main(string[] args)
        {
            ServiceBase[] servicesToRun = new ServiceBase[]
            {
                new TrackingActivityService()
            };

            if (Environment.UserInteractive)
            {
                Type type = typeof(ServiceBase);
                BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                MethodInfo method = type.GetMethod("OnStart", flags);
                foreach (ServiceBase service in servicesToRun)
                {
                    method.Invoke(service, new object[] { args });
                }

                Console.WriteLine("Activity Tracking Service has been started.");
                Console.WriteLine("Press any key to exit!");
                Console.ReadLine();

                foreach (ServiceBase service in servicesToRun)
                {
                    service.Stop();
                }
            }
            else
            {
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}