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
}
