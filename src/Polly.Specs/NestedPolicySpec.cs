using System;
using FluentAssertions;
using Xunit;

namespace Polly.Specs
{
    public class NestedPolicySpec
    {
        [Fact]
        public void Executing_the_policy_action_should_execute_the_specified_action()
        {
            bool executed = true;

            Policy.First(Policy
                    .Handle<DivideByZeroException>()
                    .Retry(1))
                .Then(Policy
                    .Handle<DivideByZeroException>()
                    .CircuitBreaker(2, TimeSpan.FromSeconds(5)))
                .Execute(() => { executed = true; });

            executed.Should().BeTrue();

        }

        [Fact]
        public void Executing_nested_policies_with_the_same_exception_predicates_should_apply_all_policies()
        {
            int executed = 0;

            Action action = () =>
                Policy.First(Policy
                        .Handle<DivideByZeroException>()
                        .Retry(1))
                    .Then(Policy
                        .Handle<DivideByZeroException>()
                        .Retry(1))
                    .Execute(() =>
                    {
                        executed++;
                        throw new DivideByZeroException();
                    });

            action.ShouldThrow<DivideByZeroException>();
            executed.Should().Be(4);
        }

        [Fact]
        public void Executing_nested_policies_with_the_different_exception_predicates_should_apply_only_matching_policies()
        {
            int executed = 0;

            Action action = () =>
                Policy.First(Policy
                        .Handle<ArgumentException>()
                        .Retry(1))
                    .Then(Policy
                        .Handle<DivideByZeroException>()
                        .Retry(1))
                    .Execute(() =>
                    {
                        executed++;
                        throw new DivideByZeroException();
                    });

            action.ShouldThrow<DivideByZeroException>();
            executed.Should().Be(2);
        }
    }
}