[logo]: https://raw.githubusercontent.com/Geeksltd/Zebble.WebView/master/Shared/Icon.png "Zebble.WebView"


## Zebble.WebView

![logo]

A Zebble plugin that allow you to show web pages in your application.


[![NuGet](https://img.shields.io/nuget/v/Zebble.WebView.svg?label=NuGet)](https://www.nuget.org/packages/Zebble.WebView/)

> A WebView component (sub-class of View) allows you to render a piece of Html or a whole URL in an embedded browser component.

<br>


### Setup
* Available on NuGet: [https://www.nuget.org/packages/Zebble.WebView/](https://www.nuget.org/packages/Zebble.WebView/)
* Install in your platform client projects.
* Available for iOS, Android and UWP.
<br>


### Api Usage

#### To show an external URL, use:
```xml
<WebView Url="http://www.Google.com" />
```
#### To show some html directly, use:
```xml
<WebView Html="@GetHtml...()" />
```
The value of HTML can be a full html page (wrapped in a <html>...</html> tag), or a partial html piece such as  `"<b>something</b>"`.

In the above example GetHtml() is a method in the code behind. Of course the data can come from the database, your domain objects, etc. 

#### To show a html file from your Resources folder, use:
```xml
<WebView Url="MyPages/SomePage.html" />
```
The above example assumes you have a file named SomePage.html under `App.UI\Resources\MyPages` folder.

Javascript, Image and CSS Files
In the HTML page, you can reference Javascript, Image or CSS files in your Resources folder.
You can also reference web-hosted resources by specifying the full url. For example:
```html
...
<script type="text/javascript" src="MyPages/MyScript.js"></script>
<script type="text/javascript" src="http://mywebsite.com/MyScript.js"></script>
...
<img src="Images/Icons/Hello.png">
```
#### Running Javascript from Zebble
The `EvaluateJavascript()` method allows you to run some Javascript code on the rendered page, and then get the value of its evaluation back.
```csharp
MyWebView.EvaluateJavascript("$('.some-object').attr('value')");
```
##### Important Tips
When generating a full page html, remember to set the viewport meta tag, otherwise the content will be rendered at the top left corner in UWP.
```html
<head>
    <meta name="viewport" content="width=device-width", initial-scale="1">
</head>
```
If using a local html file, it should be a valid xhtml. For example all tags should be closed and lowercase.
UWP does not support loading html content from local storage. However, Zebble solves that limitation. This is how it works:
It reads the HTML file and parses the html.
It then looks for `<script>` and `<link css>` tags which reference a local file.
It will then load those files and copies their content as embedded code inside the html page.
It will then load the combined html on the native device browser component.
#### Value:
The Value property determines the HTML content of current page.

### Properties
| Property     | Type         | Android | iOS | Windows |
| :----------- | :----------- | :------ | :-- | :------ |
| Url            | string           | x       | x   | x       |
| Html            | string           | x       | x   | x       |
| Value            | object           | x       | x   | x       |
| MergeExternalResources            | bool           | x       | x   | x       |


### Events
| Event             | Type                                          | Android | iOS | Windows |
| :-----------      | :-----------                                  | :------ | :-- | :------ |
| BrowserNavigated               | AsyncEvent    | x       | x   | x       |
| BrowserNavigating              | AsyncEvent<NavigatingEventArgs&gt;    | x       | x   | x       |
| LoadingError              | AsyncEvent<string&gt;    | x       | x   | x       |
| LoadFinished              | AsyncEvent    | x       | x   | x       |
| SourceChanged              | AsyncEvent    | x       | x   | x       |

### Methods
| Method       | Return Type  | Parameters                          | Android | iOS | Windows |
| :----------- | :----------- | :-----------                        | :------ | :-- | :------ |
| SetHtml         | Task| value -> string| x       | x   | x       |
| SetUrl         | Task| value -> string| x       | x   | x       |
| EvaluateJavaScript         | void| script-> string| x       | x   | x       |
| EvaluateJavaScriptFunction         | void| function -> string, args-> string[]| x       | x   | x       |
| GetExecutableHtml         | string| -| x       | x   | x       |
