﻿using System;
using System.Collections.Generic;

using IHI.Server.Database;
using IHI.Server.Database.Actions;
using IHI.Server.Useful;
using IHI.Server.Rooms.Figure;
using IHI.Server.Habbos.Messenger;
using IHI.Server.Network;
using IHI.Server.Permissions;

namespace IHI.Server.Rooms
{
    public interface IHuman : IRoomUnit
    {
        #region Property: Motto
        /// <summary>
        /// The motto of this IHuman.
        /// </summary>
        string Motto
        {
            get;
            set;
        }

        #endregion
    }
}