using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Polly
{
    /// <summary>
    /// Builder class that holds multiple internal <see cref="PolicyBuilder"/>'s.
    /// </summary>
    public partial class NestedPolicy
    {
        private readonly Stack<Policy> policies = new Stack<Policy>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstPolicy"></param>
        public NestedPolicy(Policy firstPolicy)
        {
            this.policies.Push(firstPolicy);
        }

        internal Stack<Policy> Policies
        {
            get { return policies; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nextPolicy"></param>
        /// <returns></returns>
        public NestedPolicy Then(Policy nextPolicy)
        {
            policies.Push(nextPolicy);
            return this;
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        public TResult Execute<TResult>(Func<TResult> action)
        {
            return Execute(Policies.Pop().Execute, action);
        }

        private TResult Execute<TResult>(Func<Func<TResult>, TResult> execute, Func<TResult> action)
        {
            if (Policies.Peek() == null)
                execute(action);

            var top = Policies.Pop();
            return execute(() => Execute(top.Execute, action));
        }

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        public void Execute(Action action)
        {
            Execute(Policies.Pop().Execute, action);
        }

        private void Execute(Action<Action> execute, Action action)
        {
            if (Policies.Peek() == null)
                execute(action);

            var top = Policies.Pop();
            execute(() => Execute(top.Execute, action));
        }
    }
}