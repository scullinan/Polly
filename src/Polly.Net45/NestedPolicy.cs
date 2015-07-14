using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Polly
{
    /// <summary>
    /// A nested Policy class that holds multiple internal <see cref="Policy"/>'s in a nested stack.
    /// </summary>
    public partial class NestedPolicy
    {
        private readonly Stack<Policy> policies = new Stack<Policy>();
        
        internal NestedPolicy(Policy firstPolicy)
        {
            if (firstPolicy == null) throw new ArgumentNullException("firstPolicy");
            this.policies.Push(firstPolicy);
        }

        /// <summary>
        /// Specifies the first policy of a nested policy stack. This policy will be evaulated first and any subsequent policies 
        /// will be evaluated there after in order.
        /// </summary>
        /// <param name="firstPolicy"></param>
        /// <returns>A <see cref="NestedPolicy"/> instance</returns>
        public static NestedPolicy First(Policy firstPolicy)
        {
            return new NestedPolicy(firstPolicy);
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
            if (nextPolicy == null) throw new ArgumentNullException("nextPolicy");
            policies.Push(nextPolicy);
            return this;
        }

        /// <summary>
        /// Executes the specified action within the nested policy stack and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<TResult> action)
        {
            return Execute(Policies.Pop().Execute, action);
        }

        private TResult Execute<TResult>(Func<Func<TResult>, TResult> execute, Func<TResult> action)
        {
            if (Policies.Count == 0)
                return execute(action);

            var top = Policies.Pop();
            return execute(() => Execute(top.Execute, action));
        }

        /// <summary>
        /// Executes the specified action within the nested policy stack.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public void Execute(Action action)
        {
            Execute(Policies.Pop().Execute, action);
        }

        private void Execute(Action<Action> execute, Action action)
        {
            if (Policies.Count == 0)
            {
                execute(action);
                return;
            }

            var top = Policies.Pop();
            execute(() => Execute(top.Execute, action));
        }
    }
}