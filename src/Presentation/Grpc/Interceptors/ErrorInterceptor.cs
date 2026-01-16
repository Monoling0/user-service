using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Grpc.Interceptors;

public class ErrorInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            if (ex is RpcException)
            {
                throw;
            }

            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}