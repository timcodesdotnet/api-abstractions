﻿using TimCodes.ApiAbstractions.Http.Responses;

namespace TimCodes.ApiAbstractions.Http.UnitTests.Serialization;

[Collection("http")]
public class StreamHttpApiResponseDeserializerShould
{
    private readonly TestFixture _host;
    private readonly DefaultJsonHttpApiClient _client;

    public StreamHttpApiResponseDeserializerShould(TestFixture host)
    {
        _host = host;

        _client = new DefaultJsonHttpApiClient(host.Services.GetRequiredService<ILogger<DefaultJsonHttpApiClient>>(), new HttpClient(TestFixture.MockHttp), _host.Services)
        {
            DefaultResponeDeserlializer = _host.Services.GetRequiredService<StreamHttpApiResponseDeserializer>()
        };
    }

    [Fact]
    public async Task ProcessRequestMessage()
    {
        TestFixture.MockHttp.Clear();
        TestFixture.MockHttp.When(HttpMethod.Get, "http://timcodes.net/*")
            .Respond(MediaTypes.Text, "test");

        var resolver = _client.GetResolver<TestResponse>();
        var request = new HttpApiGetRequest(new Uri("http://timcodes.net"), resolver);

        using var response = await _client.SendAsync(request);

        Assert.True(response.Success);
        var result = response as HttpApiGenericResponse<Stream>;
        Assert.NotNull(result?.Content);
        using var sr = new StreamReader(result.Content);
        Assert.Equal("test", await sr.ReadToEndAsync());
    }
}
