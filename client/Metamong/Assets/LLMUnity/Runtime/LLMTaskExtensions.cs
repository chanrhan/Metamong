using System;
using System.Threading;
using System.Threading.Tasks;

public static class LLMTaskExtensions
{
    // 반환값 있는 Task<T>용
    public static async Task<T> WithCancellation<T>(
        this Task<T> originalTask,
        CancellationToken cancellationToken)
    {
        // TaskCompletionSource: 토큰 취소 시 완료될 인스턴스
        var tcs = new TaskCompletionSource<bool>();
        using (cancellationToken.Register(
            s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
        {
            // originalTask와 tcs.Task 중 먼저 끝난 쪽을 선택
            if (originalTask != await Task.WhenAny(originalTask, tcs.Task))
            {
                // 토큰 쪽이 먼저 끝났다면
                throw new OperationCanceledException(cancellationToken);
            }
        }
        // originalTask가 완료될 때까지 대기 (예외나 결과 그대로 전파)
        return await originalTask;
    }

    // 반환값 없는 Task용
    public static async Task WithCancellation(
        this Task originalTask,
        CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();
        using (cancellationToken.Register(
            s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
        {
            if (originalTask != await Task.WhenAny(originalTask, tcs.Task))
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }
        await originalTask;
    }
}