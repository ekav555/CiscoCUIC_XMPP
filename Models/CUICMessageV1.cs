using Newtonsoft.Json;
using System;

namespace CiscoCUIC_XMPP
{


    public class CuicMessageV1
    {
        public Message message { get; set; }
    }

    public class Message
    {
        [JsonProperty("@from")]
        public string from { get; set; }
        [JsonProperty("@to")]
        public string to { get; set; }
        [JsonProperty("@id")]
        public string id { get; set; }
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }
        [JsonProperty("event")]
        public Event _event { get; set; }
    }

    public class Event
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }
        public Items items { get; set; }
    }

    public class Items
    {
        [JsonProperty("@node")]
        public string node { get; set; }
        public Item item { get; set; }
    }

    public class Item
    {
        [JsonProperty("@id")]
        public string id { get; set; }
        public Notification notification { get; set; }
    }

    public class Notification
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }
        [JsonProperty("#text")]
        public string text { get; set; }
    }



}

//TEXT PART OF XML


public class CuicMessageV1Text
{
    public Update Update { get; set; }
}

public class Update
{
    public Data data { get; set; }
    public string _event { get; set; }
    public string requestId { get; set; }
    public string source { get; set; }
}

public class Data
{
    public User user { get; set; }
}

public class User
{
    public string dialogs { get; set; }
    public string extension { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string loginId { get; set; }
    public string mediaType { get; set; }
    public string pendingState { get; set; }
    public string state { get; set; }
    public DateTime stateChangeTime { get; set; }
    public Reasoncode reasonCode { get; set; }
    public string uri { get; set; }
}

public class Reasoncode
{
    public string category { get; set; }
    public string code { get; set; }
    public string forAll { get; set; }
    public string id { get; set; }
    public string label { get; set; }
    public string systemCode { get; set; }
    public string uri { get; set; }
}
