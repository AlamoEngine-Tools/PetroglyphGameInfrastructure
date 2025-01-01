using System;
using System.Collections.Generic;
using System.Linq;

namespace AET.SteamAbstraction.Vdf.Linq;

// Taken from https://github.com/shravan2x/Gameloop.Vdf

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
        Constant = 0,
        Not = 1,
        Or = 2,
        And = 3
    }
}