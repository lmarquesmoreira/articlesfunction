 #r "Microsoft.Azure.Documents.Client"
 #r "Newtonsoft.Json"
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Collections.Generic;
using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
public static class Settings
{
    public static string EndpointPrimary = @"https://articlesdb03.documents.azure.com:443/";
    public static string PrimaryKey = @"oC6dhM7uMnWTaabQudDF8moCZUUVtws5WFwyHvCvbWFLfeGZ2I4cdI4AHOJWKw9UDyy4Jv5K8eu0dZNIOG1C4A==";
    public static List<string> PrimaryLocations = new List<string> { "East US" ,"West US" };

    public static string EndpointSecondary = @"https://articlesdb02.documents.azure.com:443/";
    public static string SecondaryKey = @"EhvQgvndYubS0NawvFB1MAkYvGEBQnXFf8eKTVEDEGHryUpxYNOzdRfpsNDjGaDIaDvNtDzizsrmNTJbGPxCMg==";
    public static List<string> SecondaryLocations = new List<string> { "West US", "East US" };

    public static string DatabaseId = "ToDoList";
    public static string CollectionId = "Items";

    public enum DbType {
        Primary, Second
    }

    public static DocumentClient GetDocumentClient(DbType type)
    {

        if(type == DbType.Primary) {

            ConnectionPolicy policy = new ConnectionPolicy();
            foreach (var l in PrimaryLocations)
            {
                Console.WriteLine(string.Format("Adding location: {0}", l));
                policy.PreferredLocations.Add(l);
            }

            return new DocumentClient(new Uri(EndpointPrimary), PrimaryKey, policy);

        } else  {
        
            ConnectionPolicy policy = new ConnectionPolicy();
            foreach (var l in SecondaryLocations)
            {
                Console.WriteLine(string.Format("Adding location: {0}", l));
                policy.PreferredLocations.Add(l);
            }
            return new DocumentClient(new Uri(EndpointSecondary), SecondaryKey, policy);
        
        }
    }
}

public class Item 
{
    public string id { get; set; }
    public string text { get; set; }
    public string location { get; set; }
    public string fromEndpoint { get; set; }
}

public class ItemComparer : EqualityComparer<Item>
{
    public override bool Equals(Item x, Item y)
    {
        var isEqual = false;

        if( x.id == y.id )
        {
            isEqual = true;
        }
        return isEqual;
    }


    public override int GetHashCode(Item s)
    {
        return base.GetHashCode();
    }
}

public static var primaryClient = Settings.GetDocumentClient(Settings.DbType.Primary);
public static var secondaryClient = Settings.GetDocumentClient(Settings.DbType.Second);

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");
    
    var client = primaryClient;
    var response = "";

    var value = new Item();
    value.id = DateTime.Now.Ticks.ToString();
    value.location = System.Net.Dns.GetHostName();
    value.fromEndpoint = client.WriteEndpoint.ToString();

    // parse query parameter
    string name = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
        .Value;

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set name to query string or body data
    name = name ?? data?.name;
    value.text = "name";

    try { 
        var uri = UriFactory.CreateDocumentCollectionUri(Settings.DatabaseId, Settings.CollectionId);
        log.Info(string.Format( "DocDB Uri is: {0}", uri));

        var doc = await client.CreateDocumentAsync(uri, value);
        doc.Resource.SetPropertyValue("location", client.WriteEndpoint.ToString());
        await client.UpsertDocumentAsync(uri, doc.Resource);
        response = $"Added data in ${client.WriteEndpoint} ";
    } catch (Exception ex){
        req.CreateResponse(HttpStatusCode.BadRequest, ex.ToString());
    }

    return req.CreateResponse(HttpStatusCode.OK, response);

    // return name == null
    //     ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
    //     : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
}
