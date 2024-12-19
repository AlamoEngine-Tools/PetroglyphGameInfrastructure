using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace AET.SteamAbstraction.Vdf.Linq;

internal interface IVEnumerable<out T> : IEnumerable<T> where T : VToken
{
    IVEnumerable<VToken> this[object key] { get; }
}

internal enum VTokenType
{
    None,
    Property,
    Object,
    Value,
    Comment,
    Conditional
}

internal static class Extensions
{
    public static T? Value<T>(this IEnumerable<VToken> value)
    {
        return value.Value<VToken, T>();
    }

    public static TValue? Value<T, TValue>(this IEnumerable<T> value) where T : VToken
    {
        if (value == null) 
            throw new ArgumentNullException(nameof(value));

        if (value is not VToken token)
            throw new ArgumentException("Source value must be a JToken.");

        return token.Convert<VToken, TValue>();
    }

    internal static TValue? Convert<T, TValue>(this T? token) where T : VToken
    {
        if (token == null)
            return default;

        // don't want to cast JValue to its interfaces, want to get the internal value
        if (token is TValue && typeof(TValue) != typeof(IComparable) && typeof(TValue) != typeof(IFormattable))
            return (TValue)(object)token;

        if (token is not VValue value)
            throw new InvalidCastException($"Cannot cast {token.GetType()} to {typeof(T)}.");

        if (value.Value is TValue u)
            return u;

        var targetType = typeof(TValue);

        if (ReflectionUtils.IsNullableType(targetType))
        {
            if (value.Value == null)
                return default;

            targetType = Nullable.GetUnderlyingType(targetType);
        }

        if (TryConvertVdf<TValue>(value.Value!, out var resultObj))
            return resultObj;

        return (TValue)System.Convert.ChangeType(value.Value, targetType!, CultureInfo.InvariantCulture);
    }

    private static bool TryConvertVdf<T>(object value, out T? result)
    {
        result = default;

        // It won't be null at this point, so just handle the nullable type.
        if ((typeof(T) == typeof(bool) || Nullable.GetUnderlyingType(typeof(T)) == typeof(bool)) && value is string valueString)
        {
            switch (valueString)
            {
                case "1":
                    result = (T)(object)true;
                    return true;

                case "0":
                    result = (T)(object)false;
                    return true;
            }
        }

        return false;
    }
}

internal abstract class VToken : IVEnumerable<VToken>
{ 
    public abstract void WriteTo(VdfWriter writer);

    public abstract VTokenType Type { get; }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<VToken>)this).GetEnumerator();
    }

    IEnumerator<VToken> IEnumerable<VToken>.GetEnumerator()
    {
        return Children().GetEnumerator();
    }

    IVEnumerable<VToken> IVEnumerable<VToken>.this[object key] => this[key]!;

    public static bool DeepEquals(VToken? t1, VToken? t2)
    {
        return t1 == t2 || (t1 != null && t2 != null && t1.DeepEquals(t2));
    }

    public abstract VToken DeepClone();

    public virtual VToken? this[object key]
    {
        get => throw new InvalidOperationException($"Cannot access child value on {GetType()}.");
        set => throw new InvalidOperationException($"Cannot set child value on {GetType()}.");
    }

    public virtual T? Value<T>(object key)
    {
        var token = this[key];
        return token == null ? default : token.Convert<VToken, T>();
    }

    public virtual IEnumerable<VToken> Children()
    {
        return [];
    }

    public IEnumerable<T> Children<T>() where T : VToken
    {
        return Children().OfType<T>();
    }

    protected abstract bool DeepEquals(VToken node);

    public override string ToString()
    {
        using var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
        var vdfTextWriter = new VdfTextWriter(stringWriter);
        WriteTo(vdfTextWriter);
        return stringWriter.ToString();
    }
}

internal class VValue : VToken
{
    private readonly VTokenType _tokenType;

    public object? Value { get; set; }

    private VValue(object? value, VTokenType type)
    {
        Value = value;
        _tokenType = type;
    }

    public VValue(object? value) : this(value, VTokenType.Value)
    {
    }

    public VValue(VValue other) : this(other.Value, other.Type)
    {
    }

    public override VTokenType Type => _tokenType;

    public override VToken DeepClone()
    {
        return new VValue(this);
    }

    public override void WriteTo(VdfWriter writer)
    {
        if (_tokenType == VTokenType.Comment)
            writer.WriteComment(ToString());
        else
            writer.WriteValue(this);
    }

    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }

    public static VValue CreateComment(string value)
    {
        return new VValue(value, VTokenType.Comment);
    }

    public static VValue CreateEmpty()
    {
        return new VValue(string.Empty);
    }

    protected override bool DeepEquals(VToken token)
    {
        if (token is not VValue otherVal)
            return false;

        return this == otherVal || (Type == otherVal.Type && Value != null && Value.Equals(otherVal.Value));
    }
}

internal class VProperty(string key, VToken value, VConditional? conditional = null) : VToken
{
    // Json.NET calls this 'Name', but since VDF is technically KeyValues we call it a 'Key'.
    public string Key { get; set; } = key ?? throw new ArgumentNullException(nameof(key));
    public VToken Value { get; set; } = value;
    public VConditional? Conditional { get; set; } = conditional;

    public VProperty(VProperty other) : this(other.Key, other.Value.DeepClone(), (VConditional?)other.Conditional?.DeepClone())
    {

    }

    public override VTokenType Type => VTokenType.Property;

    public override VToken DeepClone()
    {
        return new VProperty(this);
    }

    public override void WriteTo(VdfWriter writer)
    {
        writer.WriteKey(Key);
        Value.WriteTo(writer);

        if (Value is VValue && Conditional != null)
            Conditional.WriteTo(writer);
    }

    protected override bool DeepEquals(VToken node)
    {
        return node is VProperty otherProp && Key == otherProp.Key && VToken.DeepEquals(Value, otherProp.Value) && VConditional.DeepEquals(Conditional, otherProp.Conditional);
    }
}

internal class VConditional : VToken
{
    private readonly List<Token> _tokens = new();

    public override VTokenType Type => VTokenType.Conditional;

    public override VToken DeepClone()
    {
        var newCond = new VConditional();
        foreach (var token in _tokens)
            newCond.Add(token.DeepClone());

        return newCond;
    }

    public override void WriteTo(VdfWriter writer)
    {
        writer.WriteConditional(_tokens);
    }

    protected override bool DeepEquals(VToken token)
    {
        if (token is not VConditional otherCond)
            return false;

        return _tokens.Count == otherCond._tokens.Count && Enumerable.Range(0, _tokens.Count).All(x => Token.DeepEquals(_tokens[x], otherCond._tokens[x]));
    }

    public void Add(Token token)
    {
        _tokens.Add(token);
    }

    public bool Evaluate(IReadOnlyList<string> definedConditionals)
    {
        var index = 0;

        bool EvaluateToken()
        {
            if (_tokens[index].TokenType != TokenType.Not && _tokens[index].TokenType != TokenType.Constant)
                throw new Exception($"Unexpected conditional token type ({_tokens[index].TokenType}).");

            var isNot = false;
            if (_tokens[index].TokenType == TokenType.Not)
            {
                isNot = true;
                index++;
            }

            if (_tokens[index].TokenType != TokenType.Constant)
                throw new Exception($"Unexpected conditional token type ({_tokens[index].TokenType}).");

            return isNot ^ definedConditionals.Contains(_tokens[index++].Name!);
        }

        var runningResult = EvaluateToken();
        while (index < _tokens.Count)
        {
            var tokenType = _tokens[index++].TokenType;

            if (tokenType == TokenType.Or)
                runningResult |= EvaluateToken();
            else if (tokenType == TokenType.And)
                runningResult &= EvaluateToken();
            else
                throw new Exception($"Unexpected conditional token type ({tokenType}).");
        }

        return runningResult;
    }

    public readonly struct Token(TokenType tokenType, string? name = null)
    {
        public TokenType TokenType { get; } = tokenType;
        public string? Name { get; } = name;

        public Token DeepClone()
        {
            return new Token(TokenType, Name);
        }

        public static bool DeepEquals(Token t1, Token t2)
        {
            return t1.TokenType == t2.TokenType && t1.Name == t2.Name;
        }
    }

    public enum TokenType
    {
        Constant,
        Not,
        Or,
        And
    }
}

internal class VObject : VToken, IList<VToken>, IDictionary<string, VToken>
{
    private readonly List<VToken> _children;

    public VObject()
    {
        _children = new List<VToken>();
    }

    public VObject(VObject other)
    {
        _children = other._children.Select(x => x.DeepClone()).ToList();
    }

    public override VTokenType Type => VTokenType.Object;

    public int Count => _children.Count;

    public override VToken? this[object key]
    {
        get
        {
            if (key == null) 
                throw new ArgumentNullException(nameof(key));

            if (key is not string propertyName) 
                throw new ArgumentException($"Accessed JObject values with invalid key value: {MiscellaneousUtils.ToString(key)}. Object property name expected.");

            return this[propertyName];
        }

        set
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (key is not string propertyName) 
                throw new ArgumentException($"Set JObject values with invalid key value: {MiscellaneousUtils.ToString(key)}. Object property name expected.");

            this[propertyName] = value;
        }
    }

    public VToken this[int index]
    {
        get => _children[index];
        set => _children[index] = value;
    }

    public VToken? this[string key]
    {
        get
        {
            if (!TryGetValue(key, out var result))
                return null;

            return result;
        }

        set
        {
            var prop = Properties().FirstOrDefault(x => x.Key == key);
            if (prop != null)
                prop.Value = value ?? VValue.CreateEmpty();
            else
                Add(key, value ?? VValue.CreateEmpty());
        }
    }

    public bool IsReadOnly => false;

    ICollection<string> IDictionary<string, VToken>.Keys => Properties().Select(x => x.Key).ToList();

    ICollection<VToken> IDictionary<string, VToken>.Values => throw new NotImplementedException();

    public override IEnumerable<VToken> Children()
    {
        return _children;
    }

    public IEnumerable<VProperty> Properties()
    {
        return _children.Where(x => x is VProperty).OfType<VProperty>();
    }

    public void Add(string key, VToken value)
    {
        Add(new VProperty(key, value));
    }

    public void Add(VProperty property)
    {
        if (property == null)
            throw new ArgumentNullException(nameof(property));
        if (property.Value == null)
            throw new ArgumentNullException(nameof(property.Value));

        _children.Add(property);
    }

    public void Add(VToken token)
    {
        if (token == null)
            throw new ArgumentNullException(nameof(token));

        _children.Add(token);
    }

    public void Clear()
    {
        _children.Clear();
    }

    public bool Contains(VToken item)
    {
        return _children.Contains(item);
    }

    public bool ContainsKey(string key)
    {
        return Properties().Any(x => x.Key == key);
    }

    public void CopyTo(VToken[] array, int arrayIndex)
    {
        _children.CopyTo(array, arrayIndex);
    }

    public override VToken DeepClone()
    {
        return new VObject(this);
    }

    public int IndexOf(VToken item)
    {
        return _children.IndexOf(item);
    }

    public void Insert(int index, VToken item)
    {
        _children.Insert(index, item);
    }

    public bool Remove(string key)
    {
        return _children.RemoveAll(x => x is VProperty p && p.Key == key) != 0;
    }

    public bool Remove(VToken item)
    {
        return _children.Remove(item);
    }

    public void RemoveAt(int index)
    {
        _children.RemoveAt(index);
    }

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out VToken value)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    {
        //if (value == null) 
        //    throw new ArgumentNullException(nameof(value));
        value = Properties().FirstOrDefault(x => x.Key == key)?.Value;
        return value != null;
    }

    public override void WriteTo(VdfWriter writer)
    {
        writer.WriteObjectStart();

        foreach (var child in _children)
            child.WriteTo(writer);

        writer.WriteObjectEnd();
    }

    #region ICollection<KeyValuePair<string,JToken>> Members

    public IEnumerator<KeyValuePair<string, VToken>> GetEnumerator()
    {
        foreach (var property in Properties())
            yield return new KeyValuePair<string, VToken>(property.Key, property.Value);
    }

    VToken IDictionary<string, VToken>.this[string key]
    {
        get => this[key] ?? throw new KeyNotFoundException();
        set => this[key] = value ?? throw new ArgumentNullException(nameof(value));
    }

    void ICollection<KeyValuePair<string, VToken>>.Add(KeyValuePair<string, VToken> item)
    {
        Add(new VProperty(item.Key, item.Value));
    }

    void ICollection<KeyValuePair<string, VToken>>.Clear()
    {
        _children.Clear();
    }

    bool ICollection<KeyValuePair<string, VToken>>.Contains(KeyValuePair<string, VToken> item)
    {
        var property = Properties().FirstOrDefault(x => x.Key == item.Key);
        if (property == null)
            return false;

        return property.Value == item.Value;
    }

    void ICollection<KeyValuePair<string, VToken>>.CopyTo(KeyValuePair<string, VToken>[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "arrayIndex is less than 0.");
        if (arrayIndex >= array.Length && arrayIndex != 0)
            throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
        if (Count > array.Length - arrayIndex)
            throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");

        var index = 0;
        foreach (var property in Properties())
            array[arrayIndex + index++] = new KeyValuePair<string, VToken>(property.Key, property.Value);
    }

    bool ICollection<KeyValuePair<string, VToken>>.IsReadOnly => false;

    bool ICollection<KeyValuePair<string, VToken>>.Remove(KeyValuePair<string, VToken> item)
    {
        if (!((ICollection<KeyValuePair<string, VToken>>)this).Contains(item))
            return false;

        ((IDictionary<string, VToken>)this).Remove(item.Key);
        return true;
    }

    #endregion

    protected override bool DeepEquals(VToken token)
    {
        if (token is not VObject otherObj)
            return false;

        return _children.Count == otherObj._children.Count && Enumerable.Range(0, _children.Count).All(x => DeepEquals(_children[x], otherObj._children[x]));
    }
}

internal static class VdfConvert
{
    public static VProperty Deserialize(TextReader reader)
    {
        return Deserialize(reader, VdfSerializerSettings.Common);
    }

    public static VProperty Deserialize(TextReader reader, VdfSerializerSettings settings)
    {
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));
        return new VdfSerializer(settings).Deserialize(reader);
    }
}

internal class VdfSerializerSettings
{
    public static VdfSerializerSettings Default => new();
    public static VdfSerializerSettings Common => new()
    {
        UsesEscapeSequences = true,
        UsesConditionals = false
    };

    public bool UsesEscapeSequences;

    public bool UsesConditionals = true;

    public IReadOnlyList<string>? DefinedConditionals { get; set; }

    public int MaximumTokenSize = 4096;
}

internal class VdfSerializer
{
    private readonly VdfSerializerSettings _settings;

    public VdfSerializer() : this(VdfSerializerSettings.Default) { }

    public VdfSerializer(VdfSerializerSettings settings)
    {
        _settings = settings;

        if (_settings.UsesConditionals && _settings.DefinedConditionals == null)
            throw new Exception("DefinedConditionals must be set when UsesConditionals=true.");
    }

    public void Serialize(TextWriter textWriter, VToken value)
    {
        using VdfWriter vdfWriter = new VdfTextWriter(textWriter, _settings);
        value.WriteTo(vdfWriter);
    }

    public VProperty Deserialize(TextReader textReader)
    {
        using VdfReader vdfReader = new VdfTextReader(textReader, _settings);

        if (!vdfReader.ReadToken())
            throw new VdfException("Incomplete VDF data at beginning of file.");

        // For now, we discard these comments.
        while (vdfReader.CurrentState == VdfReader.State.Comment)
            if (!vdfReader.ReadToken())
                throw new VdfException("Incomplete VDF data after root comment.");

        return ReadProperty(vdfReader);
    }

    private VProperty ReadProperty(VdfReader reader)
    {
        // Setting it to null is temporary, we'll set Value in just a second.
        var result = new VProperty(reader.Value, null!);

        if (!reader.ReadToken())
            throw new VdfException("Incomplete VDF data after property key.");

        // For now, we discard these comments.
        while (reader.CurrentState == VdfReader.State.Comment)
            if (!reader.ReadToken())
                throw new VdfException("Incomplete VDF data after property comment.");

        if (reader.CurrentState == VdfReader.State.Property)
        {
            result.Value = new VValue(reader.Value);

            if (!reader.ReadToken())
                throw new VdfException("Incomplete VDF data after property value.");

            if (reader.CurrentState == VdfReader.State.Conditional)
                result.Conditional = ReadConditional(reader);
        }
        else if (reader.CurrentState == VdfReader.State.Object)
            result.Value = ReadObject(reader);
        else
            throw new VdfException($"Unexpected state when deserializing property (key: {result.Key}, state: {reader.CurrentState}).");

        return result;
    }

    private VObject ReadObject(VdfReader reader)
    {
        var result = new VObject();

        if (!reader.ReadToken())
            throw new VdfException("Incomplete VDF data after object start.");

        while (!(reader.CurrentState == VdfReader.State.Object && reader.Value == VdfStructure.ObjectEnd.ToString()))
        {
            if (reader.CurrentState == VdfReader.State.Comment)
            {
                result.Add(VValue.CreateComment(reader.Value));

                if (!reader.ReadToken())
                    throw new VdfException("Incomplete VDF data after object comment.");
            }
            else if (reader.CurrentState == VdfReader.State.Property)
            {
                var prop = ReadProperty(reader);

                if (!_settings.UsesConditionals || prop.Conditional == null || prop.Conditional.Evaluate(_settings.DefinedConditionals!))
                    result.Add(prop);
            }
            else
                throw new VdfException($"Unexpected state when deserializing (state: {reader.CurrentState}, value: {reader.Value}).");
        }

        reader.ReadToken();

        return result;
    }

    private VConditional ReadConditional(VdfReader reader)
    {
        var result = new VConditional();

        if (!reader.ReadToken())
            throw new VdfException("Incomplete VDF data after conditional start.");

        while (reader.CurrentState == VdfReader.State.Conditional && reader.Value != VdfStructure.ConditionalEnd.ToString())
        {
            if (reader.Value == "!")
                result.Add(new VConditional.Token(VConditional.TokenType.Not, null));
            else if (reader.Value == "&&")
                result.Add(new VConditional.Token(VConditional.TokenType.And, null));
            else if (reader.Value == "||")
                result.Add(new VConditional.Token(VConditional.TokenType.Or, null));
            else
                result.Add(new VConditional.Token(VConditional.TokenType.Constant, reader.Value.Substring(1)));

            if (!reader.ReadToken())
                throw new VdfException("Incomplete VDF data after conditional expression.");
        }

        if (!reader.ReadToken())
            throw new VdfException("Incomplete VDF data after conditional end.");

        return result;
    }
}

internal abstract class VdfReader(VdfSerializerSettings settings) : IDisposable
{
    public VdfSerializerSettings Settings { get; } = settings;
    public bool CloseInput { get; set; } = true;
    public string Value { get; set; } = null!;

    protected internal State CurrentState { get; protected set; } = State.Start;

    protected VdfReader()
        : this(VdfSerializerSettings.Default) { }

    public abstract bool ReadToken();

    void IDisposable.Dispose()
    {
        if (CurrentState == State.Closed)
            return;

        Close();
    }

    public virtual void Close()
    {
        CurrentState = State.Closed;
        Value = null!;
    }

    protected internal enum State
    {
        Start,
        Property,
        Object,
        Comment,
        Conditional,
        Finished,
        Closed
    }
}

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

            if (curChar == VdfStructure.Quote || (!_isQuoted && char.IsWhiteSpace(curChar)))
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

            if (_isConditional || (!_isQuoted && curChar == VdfStructure.ConditionalStart))
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

internal static class VdfStructure
{
    // Format
    public const char CarriageReturn = '\r', NewLine = '\n';
    public const char Quote = '"', Escape = '\\', Comment = '/', Assign = ' ', Indent = '\t';
    public const char ConditionalStart = '[', ConditionalEnd = ']', ConditionalConstant = '$', ConditionalNot = '!', ConditionalAnd = '&', ConditionalOr = '|';
    public const char ObjectStart = '{', ObjectEnd = '}';

    // Escapes
    private const uint EscapeMapLength = 128;
    private static readonly bool[] EscapeExistsMap;
    private static readonly char[] EscapeMap, UnescapeMap;
    private static readonly char[,] EscapeConversions =
    {
            { '\n', 'n'  },
            { '\t', 't'  },
            { '\v', 'v'  },
            { '\b', 'b'  },
            { '\r', 'r'  },
            { '\f', 'f'  },
            { '\a', 'a'  },
            { '\\', '\\' },
            { '?' , '?'  },
            { '\'', '\'' },
            { '\"', '\"' }
        };

    static VdfStructure()
    {
        EscapeExistsMap = new bool[EscapeMapLength];
        EscapeMap = new char[EscapeMapLength];
        UnescapeMap = new char[EscapeMapLength];

        for (var index = 0; index < EscapeMapLength; index++)
            EscapeMap[index] = UnescapeMap[index] = (char)index;

        for (var index = 0; index < EscapeConversions.GetLength(0); index++)
        {
            char unescaped = EscapeConversions[index, 0], escaped = EscapeConversions[index, 1];

            EscapeExistsMap[unescaped] = true;
            EscapeMap[unescaped] = escaped;
            UnescapeMap[escaped] = unescaped;
        }
    }

    public static bool IsEscapable(char ch) => ch < EscapeMapLength && EscapeExistsMap[ch];
    public static char GetEscape(char ch) => ch < EscapeMapLength ? EscapeMap[ch] : ch;
    public static char GetUnescape(char ch) => ch < EscapeMapLength ? UnescapeMap[ch] : ch;
}

internal abstract class VdfWriter(VdfSerializerSettings settings) : IDisposable
{
    public VdfSerializerSettings Settings { get; } = settings;
    public bool CloseOutput { get; set; } = true;
    protected internal State CurrentState { get; protected set; } = State.Start;

    protected VdfWriter() : this(VdfSerializerSettings.Default) { }

    public abstract void WriteObjectStart();

    public abstract void WriteObjectEnd();

    public abstract void WriteKey(string key);

    public abstract void WriteValue(VValue value);

    public abstract void WriteComment(string text);

    public abstract void WriteConditional(IReadOnlyList<VConditional.Token> tokens);

    void IDisposable.Dispose()
    {
        if (CurrentState == State.Closed)
            return;

        Close();
    }

    public virtual void Close()
    {
        CurrentState = State.Closed;
    }

    protected internal enum State
    {
        Start,
        Key,
        Value,
        ObjectStart,
        ObjectEnd,
        Comment,
        Conditional,
        Finished,
        Closed
    }
}

internal class VdfTextWriter(TextWriter writer, VdfSerializerSettings settings) : VdfWriter(settings)
{
    private readonly TextWriter _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    private int _indentationLevel = 0;

    public VdfTextWriter(TextWriter writer) : this(writer, VdfSerializerSettings.Default)
    {
    }

    public override void WriteKey(string key)
    {
        AutoComplete(State.Key);
        _writer.Write(VdfStructure.Quote);
        WriteEscapedString(key);
        _writer.Write(VdfStructure.Quote);
    }

    public override void WriteValue(VValue value)
    {
        AutoComplete(State.Value);
        _writer.Write(VdfStructure.Quote);
        WriteEscapedString(value.ToString());
        _writer.Write(VdfStructure.Quote);
    }

    public override void WriteObjectStart()
    {
        AutoComplete(State.ObjectStart);
        _writer.Write(VdfStructure.ObjectStart);

        _indentationLevel++;
    }

    public override void WriteObjectEnd()
    {
        _indentationLevel--;

        AutoComplete(State.ObjectEnd);
        _writer.Write(VdfStructure.ObjectEnd);

        if (_indentationLevel == 0)
            AutoComplete(State.Finished);
    }

    public override void WriteComment(string text)
    {
        AutoComplete(State.Comment);
        _writer.Write(VdfStructure.Comment);
        _writer.Write(VdfStructure.Comment);
        _writer.Write(text);
    }

    public override void WriteConditional(IReadOnlyList<VConditional.Token> tokens)
    {
        AutoComplete(State.Conditional);
        _writer.Write(VdfStructure.ConditionalStart);

        foreach (var token in tokens)
        {
            switch (token.TokenType)
            {
                case VConditional.TokenType.Constant:
                    _writer.Write(VdfStructure.ConditionalConstant);
                    _writer.Write(token.Name);
                    break;

                case VConditional.TokenType.Not:
                    _writer.Write(VdfStructure.ConditionalNot);
                    break;

                case VConditional.TokenType.Or:
                    _writer.Write(VdfStructure.ConditionalOr);
                    _writer.Write(VdfStructure.ConditionalOr);
                    break;

                case VConditional.TokenType.And:
                    _writer.Write(VdfStructure.ConditionalAnd);
                    _writer.Write(VdfStructure.ConditionalAnd);
                    break;
            }
        }

        _writer.Write(VdfStructure.ConditionalEnd);
    }

    private void AutoComplete(State next)
    {
        if (CurrentState == State.Start)
        {
            CurrentState = next;
            return;
        }

        switch (next)
        {
            case State.Value:
            case State.Conditional:
                _writer.Write(VdfStructure.Assign);
                break;

            case State.Key:
            case State.ObjectStart:
            case State.ObjectEnd:
            case State.Comment:
                _writer.WriteLine();
                _writer.Write(new string(VdfStructure.Indent, _indentationLevel));
                break;

            case State.Finished:
                _writer.WriteLine();
                break;
        }

        CurrentState = next;
    }

    private void WriteEscapedString(string str)
    {
        if (!Settings.UsesEscapeSequences)
        {
            _writer.Write(str);
            return;
        }

        foreach (var ch in str)
        {
            if (!VdfStructure.IsEscapable(ch))
                _writer.Write(ch);
            else
            {
                _writer.Write(VdfStructure.Escape);
                _writer.Write(VdfStructure.GetEscape(ch));
            }
        }
    }

    public override void Close()
    {
        base.Close();
        if (CloseOutput)
            _writer.Dispose();
    }
}

internal static class MiscellaneousUtils
{
    public static string ToString(object value)
    {
        if (value == null)
        {
            return "{null}";
        }

        return value is string ? @"""" + value.ToString() + @"""" : value.ToString();
    }
}

internal static class ReflectionUtils
{
    public static bool IsNullable(Type t)
    {
        if (t == null) 
            throw new ArgumentNullException(nameof(t));


        if (t.IsValueType)
            return IsNullableType(t);

        return true;
    }

    public static bool IsNullableType(Type t)
    {
        if (t == null) 
            throw new ArgumentNullException(nameof(t));
        return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}