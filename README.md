[![NuGet Status](https://img.shields.io/nuget/v/AjaxNetProfessional?style=flat)](https://www.nuget.org/packages/AjaxNetProfessional/)

# Ajax.NET Professional

Ajax.NET Professional (AjaxPro) is one of the first AJAX frameworks available for Microsoft ASP.NET.

The framework will create proxy JavaScript classes that are used on client-side to invoke methods on the web server with full data type support working on all common web browsers including mobile devices. Return your own classes, structures, DataSets, enums,... as you are doing directly in .NET.

## Quick Guide

- Download the latest Ajax.NET Professional
- Add a reference to the AjaxPro.2.dll
- Add following lines to your web.config if you are using Integrated IIS pipeline mode:

```XML
<configuration>
	<location path="ajaxpro">
		<system.webServer>
			<handlers>
				<add name="AjaxPro" verb="GET,POST" path="*.ashx" type="AjaxPro.AjaxHandlerFactory,AjaxPro.2" />
			</handlers>
		</system.webServer>
	</location>
</configuration>
```

- If you are using Classic IIS pipeline mode, then please add following lines:

```XML
<configuration>
	<location path="ajaxpro">
		<system.web>
			<httpHandlers>
				<add verb="GET,POST" path="*.ashx" type="AjaxPro.AjaxHandlerFactory,AjaxPro.2"/>
			</httpHandlers>
		</system.web>
	</location>
</configuration>
```

- Now, you have to mark your .NET methods with an AjaxMethod attribute

```C#
[AjaxPro.AjaxMethod]
public static DateTime GetServerTime()
{
	return DateTime.Now;
}
```

- To use the .NET method on the client-side JavaScript you have to register the methods, this will be done to register a complete class to Ajax.NET

```C#
namespace MyDemo
{
	public class DefaultWebPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			AjaxPro.Utility.RegisterTypeForAjax(typeof(DefaultWebPage));
		}

		[AjaxPro.AjaxMethod]
		public static DateTime GetServerTime()
		{
			return DateTime.Now;
		}
	}
}
```

- If you start the web page two JavaScript includes are rendered to the HTML source
- To call a .NET method form the client-side JavaScript code you can use following syntax

```JavaScript
function getServerTime() {
	MyDemo.DefaultWebPage.GetServerTime(getServerTime_callback);  // asynchronous call
}

// This method will be called after the method has been executed
// and the result has been sent to the client.
function getServerTime_callback(res) {
	alert(res.value);
}
```

## Example Projects

I have created some example web projects that I am using also using for testing. Please download the code from [Ajax.NET Professional Starter Kit](https://github.com/michaelschwarz/Ajax.NET-Professional-Starter-Kit).

## Compiler Options

- `NET20` compiles .NET 4.8 assemblies AjaxPro.2.dll (otherwise original it was .NET 1.1, AjaxPro.dll)
- `JSONLIB` compiles JSON parser only (AjaxPro.JSON.2.dll or AjaxPro.JSON.dll)
- `NET20external` is setting the assembly name to AjaxPro.2.dll, compatibility
- `TRACE` is no longer used

## Security Settings

In web.config you can configure different security related settings.

One of the most important is to set a [Content-Security-Policy](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy) HTTP response header to ensure to trust only JavaScript and other resources that are coming from your web server or trusted locations. As [AjaxPro](https://www.ajaxpro.info) is generating some JavaScript files on-the-fly you can set the JavaScript nonce in your web.config:

```XML
<configuration>
	<ajaxNet>
		<ajaxSettings>
			<contentSecurityPolicy nonce="abcdefghijklmnopqrstuvwxyz" />
		</ajaxSettings>
	</ajaxNet>
	<system.webServer>
		<httpProtocol>
			<customHeaders>
				<add name="Content-Security-Policy" 
					 value="frame-ancestors www.mydomain.com; script-src 'self' https://www.mydomain.com 'unsafe-eval' 'unsafe-hashes' 'nonce-abcdefghijklmnopqrstuvwxyz';" />
			</customHeaders>
		</httpProtocol>
	</system.webServer>
</configuration>
```

## Serialization settings

[AjaxPro](https://www.ajaxpro.info) allows the deserialization of arbitrary .NET classes as long as they are a subtype of the expected class. This can be dangerous if the expected class is a base class like `System.Object` with a large number of derived classes. The .NET framework contains several "dangerous" classes that can be abused to execute arbitrary code during the deserialization process.   

For security reasons [AjaxPro](https://www.ajaxpro.info) provides the `jsonDeserializationCustomTypes` setting, which allows to restrict the classes that can be deserialized. The setting supports allow- and blocklists, the default behaviour is `deny`.

The following example shows an allow list configuration that only allows the deserialization of objects from a specific namespace: 

```XML
<configuration>
	<ajaxNet>
		<ajaxSettings>
			<jsonDeserializationCustomTypes default="deny">
				<allow>MyOwnNamespace.*</allow>
			</jsonDeserializationCustomTypes>
		</ajaxSettings>
	</ajaxNet>
  ...
</configuration>
```

The following example shows the block-list approach were only the deserialization of specifc "dangerous" classes gets blocked. This is not recommended as developers need to maintain a list of dangerous classes.

```XML
<configuration>
	<ajaxNet>
		<ajaxSettings>
			<jsonDeserializationCustomTypes default="allow">
				<deny>System.Configuration.Install.AssemblyInstaller</deny>
			</jsonDeserializationCustomTypes>
		</ajaxSettings>
	</ajaxNet>
  ...
</configuration>
```
