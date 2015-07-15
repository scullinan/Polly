using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            if (!Policies.Any()) throw new InvalidOperationException("There are no Policies to execute");

            var stack = new Stack<Policy>(Policies);
            var top = stack.Pop();
            return ExecuteAsync(stack, top.ExecuteAsync, action);
        }

        private Task ExecuteAsync(Stack<Policy> stack, Func<Func<Task>, Task> execute, Func<Task> action)
        {
            if (!stack.Any())
                return execute(action);

            var top = stack.Pop();
            return execute(async () => await ExecuteAsync(stack, top.ExecuteAsync, action));
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
            if (!Policies.Any()) throw new InvalidOperationException("There are no Policies to execute");

            var stack = new Stack<Policy>(Policies);
            var top = stack.Pop();
            return await ExecuteAsync(stack, top.ExecuteAsync, action);
        }

        private async Task<TResult> ExecuteAsync<TResult>(Stack<Policy> stack, Func<Func<Task<TResult>>, Task<TResult>> execute, Func<Task<TResult>> action)
        {
            if (stack.Count == 0)
                return await execute(async () => await action().NotOnCapturedContext())
                    .NotOnCapturedContext();

            var top = stack.Pop();
            return await execute(async () => await ExecuteAsync(stack, top.ExecuteAsync, action))
                .NotOnCapturedContext();
        }
    }
}