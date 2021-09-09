using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CiscoCUIC_XMPP
{
    public class IQResult
    {
        public IQ iq { get; set; }
    }
    public class IQ
    {
        [JsonProperty("@type")]
        public string type { get; set; }

        [JsonProperty("@id")]
        public string id { get; set; }

        [JsonProperty("@from")]
        public string from { get; set; }

        [JsonProperty("@to")]
        public string to { get; set; }

        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }
        public Pubsub pubsub { get; set; }
    }

    public class Pubsub
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }
        public Subscriptions subscriptions { get; set; }
    }

    public class Subscriptions
    {
        public Subscription[] subscription { get; set; }
    }

    public class Subscription
    {
        [JsonProperty("@node")]
        public string node { get; set; }

        [JsonProperty("@jid")]
        public string jid { get; set; }

        [JsonProperty("@subscription")]
        public string subscription { get; set; }
    }

}
