using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

/// <summary>
/// Summary description for ClientFactory
/// </summary>
public class ClientFactory : IMsalHttpClientFactory
{
    public ClientFactory()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public HttpClient GetHttpClient()
    {
        var handler = new HttpClientHandler
        {
            UseProxy = true,
            Proxy = new WebProxy
            {
                Address = new Uri("http://proxy.ups.com:8080/"),
                Credentials = new NetworkCredential("gscf-pro", "xES11JUT")
            }
        };
        return new HttpClient(handler, true);
    }
}