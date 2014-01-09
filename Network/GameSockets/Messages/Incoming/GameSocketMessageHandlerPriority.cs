﻿namespace IHI.Server.Network
{
    /// <summary>
    ///   The priority for packet handlers to execute.
    /// </summary>
    public enum GameSocketMessageHandlerPriority
    {
        /// <summary>
        ///   Executed after High, Low and DefaultAction.
        ///   Only executed if the packet wasn't cancelled.
        /// </summary>
        Watcher = 0,
        /// <summary>
        ///   Executed after High and Low but before Watcher.
        ///   Only executed if High and Low didn't cancel the packet.
        ///   This is the default action for this packet.
        ///   DO NOT USE THIS FOR NON-STANDARD FEATURES!
        /// </summary>
        DefaultAction = 1,
        /// <summary>
        ///   Executed after High but before DefaultAction and Watcher.
        ///   Only executed if High didn't cancel the packet.
        /// </summary>
        LowPriority = 2,
        /// <summary>
        ///   Executed before Low, DefaultAction and Watcher.
        ///   Always Executed.
        /// </summary>
        HighPriority = 3
    }
}