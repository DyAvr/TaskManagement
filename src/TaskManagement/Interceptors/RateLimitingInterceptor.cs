using Grpc.Core;
using Grpc.Core.Interceptors;
using HomeworkApp.Bll.Services.Interfaces;

namespace HomeworkApp.Interceptors;

public class RateLimitingInterceptor : Interceptor
{
    private readonly IRateLimitService _rateLimiterService;

    public RateLimitingInterceptor(IRateLimitService rateLimiterService)
    {
        _rateLimiterService = rateLimiterService;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, 
        ServerCallContext context, 
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        // only V1GetTask has redis operations 
        if (context.Method.EndsWith("/V1GetTask"))
        {
            var ipAddress = context.GetHttpContext().Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ipAddress) && 
                await _rateLimiterService.IsLimitExceeded(ipAddress, CancellationToken.None))
            {
                throw new RpcException(
                    new Status(StatusCode.ResourceExhausted, "Rate limit exceeded"));
            }
        }
        
        return await continuation(request, context);
    }
}