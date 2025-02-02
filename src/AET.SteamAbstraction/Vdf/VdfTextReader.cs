﻿using System;
using System.IO;

namespace AET.SteamAbstraction.Vdf;

// Taken from https://github.com/shravan2x/Gameloop.Vdf

internal class VdfTextReader : VdfReader
{
    private const int DefaultBufferSize = 1024;

    private readonly TextReader _reader;
    private readonly char[] _charBuffer, _tokenBuffer;
    private int _charPos, _charsLen, _tokenSize;
    private bool _isQuoted, _isComment, _isConditional;

    public VdfTextReader(TextReader reader)
        : this(reader, VdfSerializerSettings.Default) { }

    public VdfTextReader(TextReader reader, VdfSerializerSettings settings)
        : base(settings)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _charBuffer = new char[DefaultBufferSize];
        _tokenBuffer = new char[settings.MaximumTokenSize];
        _charPos = _charsLen = 0;
        _tokenSize = 0;
        _isQuoted = false;
        _isComment = false;
        _isConditional = false;
    }

    /// <summary>
    /// Reads a single token. The value is stored in the 'Value' property.
    /// </summary>
    /// <returns>True if a token was read, false otherwise.</returns>
    public override bool ReadToken()
    {
        if (!SeekToken())
            return false;

        _tokenSize = 0;

        while (EnsureBuffer())
        {
            var curChar = _charBuffer[_charPos];

            #region Comment

            if (_isComment)
            {
                if (curChar == VdfStructure.CarriageReturn || curChar == VdfStructure.NewLine)
                {
                    _isComment = false;
                    Value = new string(_tokenBuffer, 0, _tokenSize);
                    CurrentState = State.Comment;
                    return true;
                }

                _tokenBuffer[_tokenSize++] = curChar;
                _charPos++;
                continue;
            }

            if (!_isQuoted && _tokenSize == 0 && curChar == VdfStructure.Comment && _charBuffer[_charPos + 1] == VdfStructure.Comment)
            {
                _isComment = true;
                _charPos += 2;
                continue;
            }

            #endregion

            #region Escape

            if (curChar == VdfStructure.Escape)
            {
                _tokenBuffer[_tokenSize++] = !Settings.UsesEscapeSequences ? curChar : VdfStructure.GetUnescape(_charBuffer[++_charPos]);
                _charPos++;
                continue;
            }

            #endregion

            #region Quote

            if (curChar == VdfStructure.Quote || !_isQuoted && char.IsWhiteSpace(curChar))
            {
                Value = new string(_tokenBuffer, 0, _tokenSize);
                CurrentState = State.Property;
                _charPos++;
                return true;
            }

            #endregion

            #region Object start/end

            if (curChar == VdfStructure.ObjectStart || curChar == VdfStructure.ObjectEnd)
            {
                if (_isQuoted)
                {
                    _tokenBuffer[_tokenSize++] = curChar;
                    _charPos++;
                    continue;
                }

                if (_tokenSize != 0)
                {
                    Value = new string(_tokenBuffer, 0, _tokenSize);
                    CurrentState = State.Property;
                    return true;
                }

                Value = curChar.ToString();
                CurrentState = State.Object;
                _charPos++;
                return true;
            }

            #endregion

            #region Conditional start/end

            if (_isConditional || !_isQuoted && curChar == VdfStructure.ConditionalStart)
            {
                if (_tokenSize > 0 && (curChar == VdfStructure.ConditionalOr || curChar == VdfStructure.ConditionalAnd || curChar == VdfStructure.ConditionalEnd))
                {
                    Value = new string(_tokenBuffer, 0, _tokenSize);
                    CurrentState = State.Conditional;
                    return true;
                }

                if (curChar == VdfStructure.ConditionalOr || curChar == VdfStructure.ConditionalAnd)
                {
                    Value = new string(_charBuffer, _charPos, 2);
                    CurrentState = State.Conditional;
                    _charPos += 2;
                    return true;
                }

                if (curChar == VdfStructure.ConditionalStart || curChar == VdfStructure.ConditionalEnd || curChar == VdfStructure.ConditionalNot)
                {
                    Value = curChar.ToString();
                    CurrentState = State.Conditional;
                    _isConditional = curChar != VdfStructure.ConditionalEnd;
                    _charPos++;
                    return true;
                }
            }

            #endregion

            #region Long token

            _tokenBuffer[_tokenSize++] = curChar;
            _charPos++;

            #endregion
        }

        return false;
    }

    /// <summary>
    /// Moves the pointer to the location of the first token character.
    /// </summary>
    /// <returns>True if a token is found, false otherwise.</returns>
    private bool SeekToken()
    {
        while (EnsureBuffer())
        {
            // Whitespace
            if (char.IsWhiteSpace(_charBuffer[_charPos]))
            {
                _charPos++;
                continue;
            }

            // Token
            if (_charBuffer[_charPos] == VdfStructure.Quote)
            {
                _isQuoted = true;
                _charPos++;
                return true;
            }

            _isQuoted = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Refills the buffer if we're at the end.
    /// </summary>
    /// <returns>False if the stream is empty, true otherwise.</returns>
    private bool EnsureBuffer()
    {
        if (_charPos < _charsLen - 1)
            return true;

        var remainingChars = _charsLen - _charPos;
        _charBuffer[0] = _charBuffer[(_charsLen - 1) * remainingChars]; // A bit of mathgic to improve performance by avoiding a conditional.
        _charsLen = _reader.Read(_charBuffer, remainingChars, DefaultBufferSize - remainingChars) + remainingChars;
        _charPos = 0;

        return _charsLen != 0;
    }

    public override void Close()
    {
        base.Close();
        if (CloseInput)
            _reader.Dispose();
    }
}