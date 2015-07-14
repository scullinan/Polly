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
        private readonly List<Policy> policies = new List<Policy>();

        internal NestedPolicy(Policy firstPolicy)
        {
            if (firstPolicy == null) throw new ArgumentNullException("firstPolicy");
            this.policies.Add(firstPolicy);
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

        internal IEnumerable<Policy> Policies
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
            policies.Add(nextPolicy);
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
            var stack = new Stack<Policy>(Policies);
            return Execute(stack, stack.Pop().Execute, action);
        }

        private TResult Execute<TResult>(Stack<Policy> stack, Func<Func<TResult>, TResult> execute, Func<TResult> action)
        {
            if (stack.Count == 0)
                return execute(action);

            var top = stack.Pop();
            return execute(() => Execute(stack, top.Execute, action));
        }

        /// <summary>
        /// Executes the specified action within the nested policy stack.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        //[DebuggerStepThrough]
        public void Execute(Action action)
        {
            var stack = new Stack<Policy>(Policies);
            Execute(stack, stack.Pop().Execute, action);
        }

        private void Execute(Stack<Policy> stack, Action<Action> execute, Action action)
        {
            if (stack.Count == 0)
            {
                execute(action);
                return;
            }
            var top = stack.Pop();
            execute(() => Execute(stack, top.Execute, action));
        }
    }
}