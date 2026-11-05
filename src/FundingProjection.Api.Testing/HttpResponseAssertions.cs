using System.Net;

namespace FundingProjection.Api.Testing;

public class HttpResponseAssertions(HttpResponseMessage message)
{
    private void AssertHttpStatusCode(HttpStatusCode statusCode) => Assert.AreEqual(statusCode, message.StatusCode);
    public void NotFound() => AssertHttpStatusCode(HttpStatusCode.NotFound);
    public void Ok() => AssertHttpStatusCode(HttpStatusCode.OK);
    public void Created() => AssertHttpStatusCode(HttpStatusCode.Created);
    public void NoContent() => AssertHttpStatusCode(HttpStatusCode.NoContent);
    public void BadRequest() => AssertHttpStatusCode(HttpStatusCode.BadRequest);
}