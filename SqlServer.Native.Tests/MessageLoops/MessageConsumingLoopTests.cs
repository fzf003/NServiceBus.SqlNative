﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SqlServer.Native;
using Xunit;

public class MessageConsumingLoopTests
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    string table = "MessageConsumingLoopTests";

    static MessageConsumingLoopTests()
    {
        DbSetup.Setup();
    }

    [Fact]
    public async Task Should_get_correct_count()
    {
        var resetEvent = new ManualResetEvent(false);
        await SqlHelpers.Drop(Connection.ConnectionString, table);
        await QueueCreator.Create(Connection.ConnectionString, table);
        await SendMessages(table);

        var count = 0;

        Task Callback(IncomingMessage message, CancellationToken cancellation)
        {
            count++;
            if (count == 5)
            {
                resetEvent.Set();
            }

            return Task.CompletedTask;
        }

        using (var loop = new MessageConsumingLoop(
            table: table,
            connectionBuilder: Connection.OpenAsyncConnection,
            callback: Callback,
            errorCallback: exception => { }))
        {
            loop.Start();
            resetEvent.WaitOne(TimeSpan.FromSeconds(30));
        }

        Assert.Equal(5, count);
    }

    static async Task SendMessages(string table)
    {
        var sender = new Sender(table);

        await sender.Send(
            Connection.ConnectionString,
            new List<OutgoingMessage>
            {
                BuildMessage("00000000-0000-0000-0000-000000000001"),
                BuildMessage("00000000-0000-0000-0000-000000000002"),
                BuildMessage("00000000-0000-0000-0000-000000000003"),
                BuildMessage("00000000-0000-0000-0000-000000000004"),
                BuildMessage("00000000-0000-0000-0000-000000000005")
            });
    }

    static OutgoingMessage BuildMessage(string guid)
    {
        return new OutgoingMessage(new Guid(guid), "theCorrelationId", "theReplyToAddress", dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }
}