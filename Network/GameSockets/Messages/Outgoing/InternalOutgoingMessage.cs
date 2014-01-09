﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace IHI.Server.Network
{
    internal class InternalOutgoingMessage : IInternalOutgoingMessage
    {
        #region Fields

        /// <summary>
        ///   The content of this message as a System.Collections.Generic.List(byte).
        /// </summary>
        private List<byte> _content;

        /// <summary>
        ///   The content of this message as a byte array.
        /// </summary>
        private byte[] _compiledContent;


        #endregion

        #region Properties

        public bool IsCompiled
        {
            get { return _compiledContent != null; }
        }

        private uint? _id;

        /// <summary>
        ///   Gets the ID of this message as an unsigned 32 bit integer.
        /// </summary>
        public uint Id
        {
            get
            {
                return _id.Value;
            }
        }

        /// <summary>
        ///   Gets the header of this message, by Base64 encoding the message ID to a 2 byte string.
        /// </summary>
        public string Header
        {
            get { return Encoding.UTF8.GetString(Base64Encoding.EncodeuUInt32(Id)); }
        }

        /// <summary>
        ///   Gets the length of the content in this message.
        /// </summary>
        public int ContentLength
        {
            get { return _content.Count; }
        }

        /// <summary>
        ///   Returns the total content of this message as a string.
        /// </summary>
        /// <returns>String</returns>
        public string ContentString
        {
            get { return _content.ToArray().ToUtf8String(); }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///   Constructs a weak ServerMessage, which is not usable until Initialize() is called.
        /// </summary>
        internal InternalOutgoingMessage()
        {
            // Requires a call to Initialize before usage
        }

        /// <summary>
        ///   Constructs a ServerMessage object with a given ID and no content.
        /// </summary>
        /// <param name = "id">The ID of this message as an unsigned 32 bit integer.</param>
        internal InternalOutgoingMessage(uint id)
        {
            Initialize(id);
        }

        #endregion

        #region Methods

        private void CompileCheck()
        {
            if (IsCompiled)
                throw new ReadOnlyException("Cannot modify a InternalOutgoingMessage once it has been compiled.");
        }
        private void InitCheck()
        {
            if (!_id.HasValue)
                throw new InvalidOperationException("Cannot modify a InternalOutgoingMessage before it has been initialized.");
        }

        /// <summary>
        ///   Clears all the content in the message and sets the message ID.
        /// </summary>
        /// <param name = "id">The ID of this message as an unsigned 32 bit integer.</param>
        public IInternalOutgoingMessage Initialize(uint id)
        {
            CompileCheck();

            if (_id.HasValue)
                throw new InvalidOperationException("Cannot reinitialize a InternalOutgoingMessage after it has already been initialized.");

            _id = id;
            _content = new List<byte>();

            return this;
        }

        /// <summary>
        ///   Clears the message content.
        /// </summary>
        public IInternalOutgoingMessage Clear()
        {
            InitCheck();
            CompileCheck();

            _content.Clear();

            return this;
        }

        /// <summary>
        ///   Appends a single byte to the message content.
        /// </summary>
        /// <param name = "b">The byte to append.</param>
        public IInternalOutgoingMessage Append(byte b)
        {
            InitCheck();
            CompileCheck();

            _content.Add(b);

            return this;
        }

        /// <summary>
        ///   Appends a byte array to the message content.
        /// </summary>
        /// <param name = "bzData">The byte array to append.</param>
        public IInternalOutgoingMessage Append(byte[] bzData)
        {
            InitCheck();
            CompileCheck();

            if (bzData != null && bzData.Length > 0)
                _content.AddRange(bzData);

            return this;
        }

        /// <summary>
        ///   Encodes a string with the environment's default text encoding and appends it to the message content.
        /// </summary>
        /// <param name = "s">The string to append.</param>
        public IInternalOutgoingMessage Append(string s)
        {
            InitCheck();
            CompileCheck();

            Append(s, null);

            return this;
        }

        /// <summary>
        ///   Encodes a string with a given text encoding and appends it to the message content.
        /// </summary>
        /// <param name = "s">The string to append.</param>
        /// <param name = "encoding">A System.Text.Encoding to use for encoding the string.</param>
        public IInternalOutgoingMessage Append(string s, Encoding encoding)
        {
            InitCheck();
            CompileCheck();

            if (!string.IsNullOrEmpty(s))
            {
                Append(Encoding.UTF8.GetBytes(s));
            }

            return this;
        }

        /// <summary>
        ///   Appends a 32 bit integer in it's string representation to the message content.
        /// </summary>
        /// <param name = "i">The 32 bit integer to append.</param>
        public IInternalOutgoingMessage Append(Int32 i)
        {
            InitCheck();
            CompileCheck();

            Append(i.ToString(CultureInfo.InvariantCulture), Encoding.UTF8);

            return this;
        }

        /// <summary>
        ///   Appends a 32 bit unsigned integer in it's string representation to the message content.
        /// </summary>
        /// <param name = "i">The 32 bit unsigned integer to append.</param>
        public IInternalOutgoingMessage Append(uint i)
        {
            InitCheck();
            CompileCheck();

            Append((Int32)i);

            return this;
        }

        /// <summary>
        ///   Appends a wire encoded boolean to the message content.
        /// </summary>
        /// <param name = "b">The boolean to encode and append.</param>
        public IInternalOutgoingMessage AppendBoolean(bool b)
        {
            InitCheck();
            CompileCheck();

            _content.Add(b ? WireEncoding.Positive : WireEncoding.Negative);

            return this;
        }

        /// <summary>
        ///   Appends a wire encoded 32 bit integer to the message content.
        /// </summary>
        /// <param name = "i">The 32 bit integer to encode and append.</param>
        public IInternalOutgoingMessage AppendInt32(Int32 i)
        {
            InitCheck();
            CompileCheck();

            Append(WireEncoding.EncodeInt32(i));

            return this;
        }

        /// <summary>
        ///   Appends a wire encoded 32 bit unsigned integer to the message content.
        /// </summary>
        /// <param name = "i">The 32 bit unsigned integer to encode and append.</param>
        /// <seealso>AppendInt32</seealso>
        public IInternalOutgoingMessage AppendUInt32(uint i)
        {
            InitCheck();
            CompileCheck();

            AppendInt32((Int32)i);

            return this;
        }

        /// <summary>
        ///   Appends a string with the default string breaker byte to the message content.
        /// </summary>
        /// <param name = "s">The string to append.</param>
        public IInternalOutgoingMessage AppendString(string s)
        {
            InitCheck();
            CompileCheck();

            AppendString(s, 2);

            return this;
        }

        /// <summary>
        ///   Appends a string with a given string breaker byte to the message content.
        /// </summary>
        /// <param name = "s">The string to append.</param>
        /// <param name = "breaker">The byte used to mark the end of the string.</param>
        public IInternalOutgoingMessage AppendString(string s, byte breaker)
        {
            InitCheck();
            CompileCheck();

            Append(s); // Append string with default encoding
            Append(breaker); // Append breaker

            return this;
        }

        public IInternalOutgoingMessage Compile()
        {
            InitCheck();

            _compiledContent = new byte[ContentLength + 2 + 1];

            byte[] headerBytes = Base64Encoding.EncodeuUInt32(Id);
            _compiledContent[0] = headerBytes[0];
            _compiledContent[1] = headerBytes[1];

            for (int i = 0; i < ContentLength; i++)
            {
                _compiledContent[i + 2] = _content[i];
            }

            _compiledContent[_compiledContent.Length - 1] = 1;

            return this;
        }

        public byte[] GetBytes()
        {
            return _compiledContent;
        }

        #endregion
    }
}