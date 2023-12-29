#if NET6_0
namespace System.Diagnostics.CodeAnalysis;

/// <summary>Fake version of the StringSyntaxAttribute, which was introduced in .NET 7</summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class StringSyntaxAttribute : Attribute
{
/// <summary>
/// Initializes a new instance of the <see cref="StringSyntaxAttribute"/> class.
/// </summary>
/// <param name="syntax">The syntax identifier.</param>
public StringSyntaxAttribute(string syntax)
{
  this.Syntax = syntax;
  this.Arguments = Array.Empty<object>();
}

/// <summary>
/// Initializes a new instance of the <see cref="StringSyntaxAttribute"/> class.
/// </summary>
/// <param name="syntax">The syntax identifier.</param>
/// <param name="arguments">Optional arguments associated with the specific syntax employed.</param>
public StringSyntaxAttribute(string syntax, params object[] arguments)
{
  this.Syntax = syntax;
  this.Arguments = arguments;
}

/// <summary>Gets the identifier of the syntax used.</summary>
/// <value>The identifier of the syntax used</value>
public string Syntax { get; }

/// <summary>Gets the optional arguments associated with the specific syntax employed.</summary>
/// <value>The arguments associated with the specific syntax</value>
public object[] Arguments { get; }
}
#endif