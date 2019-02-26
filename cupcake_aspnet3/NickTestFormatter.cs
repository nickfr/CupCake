using System;
using System.Text;
using System.Collections;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

public class NickTestFormatter : TextOutputFormatter
{
    public NickTestFormatter()
    {
        SupportedMediaTypes.Add(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"));
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }
    
    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var r = context.HttpContext.Response;
        var en = context.Object as IAsyncEnumerable<string>;

        await r.WriteAsync("[", selectedEncoding);
        await foreach (var s in en)
        {
            await r.WriteAsync($"\"{s}\",", selectedEncoding);
        }
        await r.WriteAsync("]", selectedEncoding);
    }

    private static Type GetTypeOf(object obj)
    {
        throw new NotImplementedException();
    }

    protected override bool CanWriteType(Type type)
    {
        return typeof(IAsyncEnumerable<string>).IsAssignableFrom(type);
    }
}