﻿using System;
using System.Collections.Generic;

using IHI.Server.Database;
using IHI.Server.Database.Actions;
using IHI.Server.Events;
using IHI.Server.Rooms;
using IHI.Server.Rooms.Figure;
using IHI.Server.Useful;
using IHI.Server.Habbos.Messenger;
using IHI.Server.Network;
using IHI.Server.Permissions;
using RoomUnitMoveEventArgs = IHI.Server.Events.RoomUnitMoveEventArgs;

namespace IHI.Server.Habbos
{
    public class Habbo : IPlayerHuman, IMessageable, IInstanceStorage, IPersistableStorage, IRoomOwner
    {
        #region Properties

        #region Property: Id

        private RoomUnitFigure _genericFigure;

        /// <summary>
        /// The ID of this habbo.
        /// This value is read only.
        /// </summary>
        public int Id { get; private set; }

        #endregion

        #region Property: LoginId

        private ResettableLazyDirty<int> _loginId;

        /// <summary>
        /// The login this Habbo is assigned to.
        /// </summary>
        /// <remarks>Uses lazy loading.</remarks>
        public int LoginId
        {
            get
            {
                return _loginId.Value;
            }
            set
            {
                _loginId.Value = value;
            }
        }

        #endregion

        #region Property: Username

        private ResettableLazyDirty<string> _username;

        /// <summary>
        /// The username of this Habbo.
        /// This value is read only.
        /// </summary>
        /// <remarks>Uses lazy loading.</remarks>
        public string Username
        {
            get
            {
                return _username.Value;
            }
        }

        #endregion

        #region Property: DisplayName

        private string _displayName;

        /// <summary>
        /// The display name of this Habbo.
        /// If the true value is null then this will return the username instead.
        /// This property is not saved in the database and is not maintained between Habbo instances.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return _displayName ?? Username;
            }
            set
            {
                _displayName = value;
            }
        }

        #endregion

        #region Property: DateCreated

        private ResettableLazyDirty<DateTime> _dateCreated;

        /// <summary>
        /// The time and date this Habbo was created.
        /// This value is read only.
        /// </summary>
        /// <remarks>Uses lazy loading.</remarks>
        public DateTime DateCreated
        {
            get
            {
                return _dateCreated.Value;
            }
        }

        #endregion

        #region Property: LastAccess

        /// <summary>
        /// The time and date this Habbo last connected.
        /// </summary>
        /// <remarks>Uses lazy loading.</remarks>
        private ResettableLazyDirty<DateTime> _lastAccess;

        public DateTime LastAccess
        {
            get
            {
                return _lastAccess.Value;
            }
            set
            {
                _lastAccess.Value = value;
            }
        }

        #endregion

        #region Property: SSOTicket

        /// <summary>
        /// The SSO ticket of this Habbo.
        /// This property is not cached. It is always loaded/saved from/to the database.
        /// </summary>
        internal string SSOTicket
        {
            get
            {
                return HabboActions.GetSSOTicketFromHabboId(Id);
            }
            set
            {
                HabboActions.SetSSOTicketFromHabboId(Id, value);
            }
        }

        #endregion

        #region Property: LoggedIn
        /// <summary>
        ///   Is the Habbo logged in.
        /// </summary>
        public bool LoggedIn
        {
            get;
            set;
        }
        #endregion

        #region Property: Position
        private RoomPosition _position;
        public RoomPosition Position
        {
            get
            {
                return _position;
            }
            set
            {
                RoomUnitMoveEventArgs eventArgs = new RoomUnitMoveEventArgs(this, _position, value);
                CoreManager.ServerCore.OfficalEventFirer.Fire("roomunit_move:before", eventArgs);
                if (!eventArgs.IsCancelled)
                {
                    if (_position.Room != value.Room) // Is the room unit changing rooms?
                    {
                        _position.Room.RemoveRoomUnit(this);
                        _position = value;
                        _position.Room.AddRoomUnit(this);
                    }
                    else
                    {
                        _position = value;
                    }
                    CoreManager.ServerCore.OfficalEventFirer.Fire("roomunit_move:before", eventArgs);
                }
            }
        }
        #endregion

        #region Property: RoomUnitId
        public int RoomUnitId
        {
            get;
            set;
        }
        #endregion

        #region Property: Figure
        private ResettableLazyDirty<HabboFigure> _figure;
        /// <summary>
        /// TODO: Document
        /// </summary>
        /// <remarks>Uses lazy loading.</remarks>
        public HabboFigure Figure
        {
            get
            {
                return _figure.Value;
            }
            set
            {
                _figure.Value = value;
            }
        }
        public RoomUnitFigure GenericFigure
        {
            get
            {
                return Figure;
            }
        }
        #endregion

        #region Property: Motto

        private ResettableLazyDirty<string> _motto;

        /// <summary>
        /// The motto of this Habbo.
        /// </summary>
        /// <remarks>Uses lazy loading.</remarks>
        public string Motto
        {
            get
            {
                return _motto.Value;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", "Setting the motto of a Habbo to null is not allowed!");
                _motto.Value = value;
            }
        }

        #endregion

        #region Property: Credits

        private ResettableLazyDirty<int> _credits;

        /// <summary>
        /// The amount of credits this Habbo has.
        /// </summary>
        /// <remarks>Uses lazy loading.</remarks>
        public int Credits
        {
            get
            {
                return _credits.Value;
            }
            set
            {
                _credits.Value = value;
            }
        }

        #endregion

        #region Property: MessengerCategories

        public HashSet<MessengerCategory> MessengerCategories { get; private set; }

        #endregion

        #region IBefriendable Properties

        #region Property: Stalkable

        /// <summary>
        /// 
        /// </summary>
        public bool Stalkable { get; set; }

        #endregion

        #region Property: Requestable

        /// <summary>
        /// 
        /// </summary>
        public bool Requestable { get; set; }

        #endregion

        #region Property: Inviteable

        /// <summary>
        /// 
        /// </summary>
        public bool Inviteable { get; set; }

        #endregion

        #endregion

        #region Property: Socket

        /// <summary>
        /// The current socket connection of this Habbo.
        /// </summary>
        public GameSocket Socket
        {
            get;
            set;
        }

        #endregion

        #region Property: InstanceStorage
        private InstanceStorage _instanceStorage;
        public InstanceStorage InstanceStorage
        {
            get
            {
                return _instanceStorage;
            }
        }
        #endregion

        #region Property: PersistentStorage
        private PersistentStorage _persistentStorage;
        public PersistentStorage PersistentStorage
        {
            get
            {
                return _persistentStorage;
            }
        }
        #endregion

        #region Property: PersistInstanceId

        public long PersistableInstanceId
        {
            get
            {
                return Id;
            }
        }

        #endregion

        #region Property: Permissions
        private ResettableLazyDirty<IDictionary<string, PermissionState>> _permissions;
        /// <summary>
        /// TODO: Document
        /// </summary>
        /// <remarks>Uses lazy loading.</remarks>
        public IDictionary<string, PermissionState> Permissions
        {
            get
            {
                return _permissions.Value;
            }
        }
        #endregion

        #endregion

        #region Methods
        #region Method: Habbo (Constructor)
        internal Habbo(int id)
        {
            if (!HabboActions.DoesHabboIdExist(id))
                throw new Exception(CoreManager.ServerCore.StringLocale.GetString("CORE:EXCEPTION_HABBO_CONSTRUCT_BAD_ID"));
            Id = id;
            Init();
        }
        internal Habbo(string username)
        {
            // The username really should be set here but it is not currently possible with the lazy loading.
            Id = HabboActions.GetHabboIdFromHabboUsername(username);
            Init();
        }
        internal Habbo(GameSocket socket)
        {
            Socket = socket;
        }
        #endregion

        #region Method: Init
        private void Init()
        {
            _loginId = new ResettableLazyDirty<int>(() => HabboActions.GetLoginIdFromHabboId(Id));
            _username = new ResettableLazyDirty<string>(() => HabboActions.GetHabboUsernameFromHabboId(Id));
            _dateCreated = new ResettableLazyDirty<DateTime>(() => HabboActions.GetCreationDateFromHabboId(Id));
            _lastAccess = new ResettableLazyDirty<DateTime>(() => HabboActions.GetLastAccessDateFromHabboId(Id));
            _credits = new ResettableLazyDirty<int>(() => HabboActions.GetCreditsFromHabboId(Id));

            _instanceStorage = new InstanceStorage();
            _persistentStorage = new PersistentStorage(this);

            _permissions = new ResettableLazyDirty<IDictionary<string, PermissionState>>(() => CoreManager.ServerCore.PermissionDistributor.GetHabboPermissions(this));

            _figure = new ResettableLazyDirty<HabboFigure>(() => LoadFigure());
            _motto = new ResettableLazyDirty<string>(() => HabboActions.GetMottoFromHabboId(Id));

            MessengerCategories = new HashSet<MessengerCategory>();
        }
        #endregion
        
        #region Method: SendMessage
        public IMessageable SendMessage(IInternalOutgoingMessage message)
        {
            Socket.SendMessage(message);
            return this;
        }
        #endregion

        #region Method: HasPermission
        public bool HasPermission(string permission)
        {
            return CoreManager.ServerCore.PermissionDistributor.HasPermission(_permissions.Value, permission);
        }
        #endregion  
        
        #region Method: LoadFigure
        private void LoadFigure()
        {
            string figureString;
            bool gender;
            HabboActions.GetFigureFromHabboId(Id, out figureString, out gender);
            _figure.Value = CoreManager.ServerCore.HabboFigureFactory.Parse(figureString, gender);
        }
        #endregion

        #endregion
    }
}