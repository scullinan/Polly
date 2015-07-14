using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Polly.Extensions;

namespace Polly
{
    public partial class NestedPolicy
    {
        /// <summary>
        ///     Executes the specified asynchronous action within the nested policy stack.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Task> action)
        {
            if (Policies.Count == 0) throw new InvalidOperationException("There are no Policies to execute");

            return ExecuteAsync(Policies.Pop().ExecuteAsync, action);
        }

        private Task ExecuteAsync(Func<Func<Task>, Task> execute, Func<Task> action)
        {
            if (Policies.Count == 0)
                return execute(action);

            return execute(async () => await ExecuteAsync(Policies.Pop().ExecuteAsync, action));
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the nested policy stack and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action)
        {
            if (Policies.Count == 0) throw new InvalidOperationException("There are no Policies to execute");

            return await ExecuteAsync(Policies.Pop().ExecuteAsync, action);
        }

        private async Task<TResult> ExecuteAsync<TResult>(Func<Func<Task<TResult>>, Task<TResult>> execute, Func<Task<TResult>> action)
        {
            if (Policies.Count == 0)
                return await execute(async () => await action().NotOnCapturedContext())
                    .NotOnCapturedContext();

            var top = Policies.Pop();
            return await execute(async () => await ExecuteAsync(top.ExecuteAsync, action))
                .NotOnCapturedContext();
        }
    }
}