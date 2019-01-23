using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Models
{
    [JsonConverter(typeof(EnvelopeConverter))]
    public abstract class EnvelopeBase
    {
        public string EnvelopePropertyName { get; set; }
        public int? Count { get; set; }
        public object Content { get; set; }
    }

    public class Envelope<T> : EnvelopeBase
    {
        [Required]
        public new T Content
        {
            get { return (T)base.Content; }
            set { base.Content = value; }
        }
    }

    public class EnvelopeConverter : JsonConverter<EnvelopeBase>
    {
        public override EnvelopeBase ReadJson(JsonReader reader, Type objectType, EnvelopeBase existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var contentType = objectType.GenericTypeArguments.First();

            string propName = null;
            object content = null;

            if(reader.TokenType == JsonToken.StartObject)
            {
                reader.Read();
                if(reader.TokenType == JsonToken.PropertyName)
                {
                    propName = (string)reader.Value;
                    reader.Read();
                    content = serializer.Deserialize(reader, contentType);
                }
            }
            var ob = (EnvelopeBase)Activator.CreateInstance(objectType);
            ob.Content = content;
            ob.EnvelopePropertyName = propName;
            return ob;
        }

        public override void WriteJson(JsonWriter writer, EnvelopeBase ev, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(ev.EnvelopePropertyName);
            serializer.Serialize(writer, ev.Content);
            if (ev.Count.HasValue)
            {
                writer.WritePropertyName($"{ev.EnvelopePropertyName}Count");
                writer.WriteValue(ev.Count.Value);
            }
            writer.WriteEndObject();
        }
    }
}
