using System;
using System.IO;
using System.Runtime.CompilerServices;
using ColossalFramework.UI;

/// <summary>
/// Updated from SkylinesBulldoze added global preferences
/// added Additional selections
/// </summary>
namespace AnotherBulldoze
{
    public class Log
    {
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void debug(string message)
        {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, message);
        }

        
    }

    /// <summary>
    /// Global options
    /// </summary>
    public static class G
    {
        public static bool _Roads { get; internal set; }
        public static bool _Railroads { get; internal set; }
        public static bool _Highways { get; internal set; }
        public static bool _Buildings { get; internal set; }
        public static bool _Trees { get; internal set; }
        public static bool _PowerLines { get; internal set; }
        public static bool _Pipes { get; internal set; }
        public static bool _Props { get; internal set; }
    }
}
