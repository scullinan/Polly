using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Polly.Specs
{
    public class NestedPolicyAsyncSpecs
    {
        public static readonly Task CompletedTask = Task.FromResult(0);
       
        [Fact]
        public async Task Executing_the_policy_action_should_execute_the_specified_async_action()
        {
            bool executed = false;

            var policy = Policy.First(Policy
                    .Handle<DivideByZeroException>()
                    .RetryAsync((_, __) => { }))
                .Then(Policy
                    .Handle<ArgumentException>()
                    .RetryAsync((_, __) => { }));

            await policy.ExecuteAsync(() =>
            {
                executed = true;
                return Task.FromResult(true) as Task;
            });

            executed.Should()
                .BeTrue();
        }

        [Fact]
        public async Task Executing_the_policy_function_should_execute_the_specified_async_function_and_return_the_result()
        {
            var policy = Policy.First(Policy
                    .Handle<DivideByZeroException>()
                    .RetryAsync((_, __) => { }))
                .Then(Policy
                    .Handle<ArgumentException>()
                    .RetryAsync((_, __) => { }));

            int result = await policy.ExecuteAsync(() => Task.FromResult(2));

            result.Should()
                .Be(2);
        }
    }
}