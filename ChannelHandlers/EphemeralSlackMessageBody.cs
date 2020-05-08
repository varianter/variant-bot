using Newtonsoft.Json;

namespace VariantBot.ChannelHandlers
{
    public class EphemeralSlackMessageBody
    {
        [JsonProperty("channel")] public string Channel { get; set; }

        [JsonProperty("user")] public string User { get; set; }

        [JsonProperty("blocks")] public Block[] Blocks { get; set; }
    }

    public class Block
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public Text Text { get; set; }

        [JsonProperty("elements", NullValueHandling = NullValueHandling.Ignore)]
        public Element[] Elements { get; set; }
    }

    public class Element
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("text")] public Text Text { get; set; }

        [JsonProperty("value")] public string Value { get; set; }
    }

    public class Text
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("text")] public string TextText { get; set; }
    }
}