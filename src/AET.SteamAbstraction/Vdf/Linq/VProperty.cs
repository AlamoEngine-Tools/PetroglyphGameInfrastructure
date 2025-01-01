using System;

namespace AET.SteamAbstraction.Vdf.Linq;

// Taken from https://github.com/shravan2x/Gameloop.Vdf

internal class VProperty(string key, VToken value, VConditional? conditional = null) : VToken
{
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