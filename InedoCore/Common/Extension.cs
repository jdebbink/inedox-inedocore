﻿namespace Inedo.Extensions
{
    internal static class Extension
    {
#if Otter
        public const string Product = "Otter";
        public static string ProductVersion => typeof(Otter.IOtterContext).Assembly.GetName().Version.ToString();
#elif BuildMaster
        public const string Product = "BuildMaster";
        public static string ProductVersion => typeof(BuildMaster.IBuildMasterContext).Assembly.GetName().Version.ToString();
#endif

        public static string Version => typeof(Extension).Assembly.GetName().Version.ToString();
    }
}