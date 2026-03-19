using System.Text.Json.Serialization;

namespace Lode.Cli;

[JsonSerializable(typeof(List<Dictionary<string, string?>>))]
internal partial class LodeJsonContext : JsonSerializerContext;
