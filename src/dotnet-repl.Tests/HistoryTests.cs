using System;
using System.Threading;
using System.Threading.Tasks;
using dotnet_repl.LineEditorCommands;
using FluentAssertions;
using Microsoft.DotNet.Interactive.Commands;
using RadLine;
using Xunit;
using Xunit.Abstractions;

namespace dotnet_repl.Tests
{
    public class HistoryTests : ReplInteractionTests
    {
        public HistoryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void previous_does_not_clear_buffer_when_there_is_no_history()
        {
            var buffer = new LineBuffer("hi");
            var context = new LineEditorContext(buffer, ServiceProvider);

            context.Execute(new PreviousHistory(Repl));

            context.Buffer.Content.Should().Be("hi");
        }

        [Fact]
        public void previous_replaces_buffer_with_last_submission()
        {
            Repl.TryAddToHistory(new SubmitCode("1"));

            var buffer = new LineBuffer("hi");
            var context = new LineEditorContext(buffer, ServiceProvider);

            context.Execute(new PreviousHistory(Repl));

            context.Buffer.Content.Should().Be("1");
            Repl.HistoryIndex.Should().Be(0);
        }

        [Fact]
        public async Task previous_submissions_contain_top_level_magics()
        {
            await Kernel.SendAsync(new SubmitCode("#!csharp\n123"), CancellationToken.None);

            var buffer = new LineBuffer("hi");
            var context = new LineEditorContext(buffer, ServiceProvider);

            context.Execute(new PreviousHistory(Repl));

            context.Buffer.Content.Should().Be("#!csharp\n123");
            Repl.HistoryIndex.Should().Be(0);
        }

        [Fact]
        public void invoking_previous_twice_replaces_buffer_with_submission_before_last()
        {
            Repl.TryAddToHistory(new SubmitCode("1"));
            Repl.TryAddToHistory(new SubmitCode("2"));

            var buffer = new LineBuffer("hi");
            var context = new LineEditorContext(buffer, ServiceProvider);

            context.Execute(new PreviousHistory(Repl));
            context.Execute(new PreviousHistory(Repl));

            context.Buffer.Content.Should().Be("1");
            Repl.HistoryIndex.Should().Be(0);
        }

        [Fact]
        public void invoking_previous_repeatedly_stops_at_last_submission()
        {
            Repl.TryAddToHistory(new SubmitCode("1"));

            var buffer = new LineBuffer("hi");
            var context = new LineEditorContext(buffer, ServiceProvider);

            context.Execute(new PreviousHistory(Repl));
            context.Execute(new PreviousHistory(Repl));

            context.Buffer.Content.Should().Be("1");
            Repl.HistoryIndex.Should().Be(0);
        }

        [Fact]
        public void next_does_not_clear_buffer_when_there_is_no_history()
        {
            var buffer = new LineBuffer("hi");
            var context = new LineEditorContext(buffer, ServiceProvider);

            context.Execute(new NextHistory(Repl));

            context.Buffer.Content.Should().Be("hi");
            Repl.HistoryIndex.Should().Be(-1);
        }

        [Fact]
        public void previous_then_next_returns_to_original_buffer()
        {
            Repl.TryAddToHistory(new SubmitCode("1"));

            var buffer = new LineBuffer("hi");
            var context = new LineEditorContext(buffer, ServiceProvider);

            context.Execute(new PreviousHistory(Repl));
            context.Execute(new NextHistory(Repl));

            context.Buffer.Content.Should().Be("hi");
            Repl.HistoryIndex.Should().Be(1);
        }

        [Fact]
        public void previous_twice_then_next_returns_to_original_buffer()
        {
            Repl.TryAddToHistory(new SubmitCode("1"));
            Repl.TryAddToHistory(new SubmitCode("2"));

            var buffer = new LineBuffer("hi");
            var context = new LineEditorContext(buffer, ServiceProvider);

            context.Execute(new PreviousHistory(Repl));
            context.Execute(new PreviousHistory(Repl));
            context.Execute(new NextHistory(Repl));

            context.Buffer.Content.Should().Be("2");
            Repl.HistoryIndex.Should().Be(1);
        }

        [Fact]
        public void Submitting_an_entry_resets_history_index()
        {
            Repl.TryAddToHistory(new SubmitCode("1"));
            Repl.TryAddToHistory(new SubmitCode("2"));
            Repl.TryAddToHistory(new SubmitCode("3"));

            var buffer = new LineBuffer("hi");
            var context = new LineEditorContext(buffer, ServiceProvider);

            context.Execute(new PreviousHistory(Repl));
            context.Execute(new PreviousHistory(Repl));

            context.Submit(SubmitAction.Submit);

            context.Buffer.Content.Should().Be("2");

            Repl.HistoryIndex.Should().Be(1);
        }

        [Fact]
        public async Task Repeating_a_submission_resets_history_index()
        {
            await Kernel.SendAsync(new SubmitCode("1"), CancellationToken.None);
            await Kernel.SendAsync(new SubmitCode("1"), CancellationToken.None);
            
            Repl.History.Count.Should().Be(1);
        }
    }
}