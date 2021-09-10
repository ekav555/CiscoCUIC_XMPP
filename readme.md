**Introduction**

The following project will explain how to connect to Cisco UCCX Realtime Notification Service and not rely on using 3rd party pay ware such as Matrix.Xmpp.Net. 
We will build a Microsoft .NET Console Application and used a 3rd party free library created by Smiley22. 
There was a lot of debugging needed to make things work. There was also two main code changes required inside Smiley22 Project. I quickly compiled this with not a lot of planning, but just to get a working example for the community.
Forgive the poor coding/error handling on my part :)

At a high level, we needed to change two things. 

**Cisco References**

https://developer.cisco.com/docs/finesse/#!about-cisco-finesse-notifications

https://www.cisco.com/c/en/us/support/docs/customer-collaboration/unified-contact-center-express-1151/211376-Technote-on-How-Bosh-Connection-Works-fo.html#anc6


**Step/Modification 1 (Subscribing to Events)**

Download https://github.com/smiley22/S22.Xmpp from GitHub (Do not install published NUGET package) - download and extract and add it to your project manually. 
The Send(Stanza stanza) method in XmppCore.cs needed a minor tweak. I had to make sure the code was not removing the namespace needed from Cisco documentation. 
See below for example. A simple find and replace fixed this for me and it allowed me programmatically subscribe to events. 

```csharp
    void Send(Stanza stanza) 
    {
        stanza.ThrowIfNull("stanza");
        //MAKE SURE WE HAVE THE NAMESPACE FOR CISCO
        var s = stanza.ToString();
        s = s.Replace("<pubsub>", @"<pubsub xmlns=""http://jabber.org/protocol/pubsub"">");
        Send(s);
    }
```    
    
**Step/Modification 2 (Processing Messages and Raising Events)**

Again, I had to change how some of the code was in this REPO that I downloaded in Step/Modification 1. I created a branch and modified the XmppIm.cs
file to allow for a message with an "Event" XML Tag. Previously it was only checking for "Body" while processing the event and Finesse XMPP is not sending the Stanza that way. 
Here is my Pull Request https://github.com/smiley22/S22.Xmpp/pull/21.


**Connecting to the Client**

Note: Here is where you add your event handlers. This is important as we want to receive events back from the notification service after we subscribe.
 
 ```csharp
private static void connectClient()
{
    try
    {
        _client = new XmppClient(_hostname, 5222, true);               
        _client.Username = _username;
        _client.Password = _password;
 
        _client.SubscriptionApproved += Client_SubscriptionApproved;
        _client.Message += Client_Message1;
        _client.Error += Client_Error;
        _client.Connect();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
}
```
**Connecting to the Server**

Note: Here is where you connect and make server side actions. 
I used this to subscribe and unsubscribe to the message controller. Also, I would look up to see if I had any previous subscriptions and then unsubscribe them

```csharp
private static void connectToServer()
{
    try
    {
        _server = new XmppCore(_hostname, 5222, true);
        _server.Username = _username;
        _server.Password = _password;
        _server.Connect();
    }
    catch(Exception ex)
    {
        Console.WriteLine(ex);
    }
}
```

**Subscribe to Topic**

This is used to Subscribe to a Topic. EG: /finesse/api/Team/26/Users. See the Cisco Docs to understand this in more detail. 

```csharp
private static Iq subscribe(IqType type, string to, string from, string topic)
{
    XmlDocument xml = new XmlDocument();
    XmlDocument xml2 = new XmlDocument();
    XmlElement iq2 = xml.CreateElement("iq");
    XmlElement pubsub2 = xml.CreateElement("pubsub");
    XmlElement sub2 = xml.CreateElement("subscribe");
    sub2.SetAttribute("node", topic);
    sub2.SetAttribute("jid", from);
    pubsub2.SetAttribute("xmlns", "http://jabber.org/protocol/pubsub");
    pubsub2.AppendChild(sub2);
    return _server.IqRequest(IqType.Set, to, from, pubsub2);
}
```
**Console Log will look like this**

![image](https://user-images.githubusercontent.com/36309818/132777756-22554e1e-1d58-485f-8da6-e8809fc33571.png)
