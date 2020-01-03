<!--
GENERATED FILE - DO NOT EDIT
This file was generated by [MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets).
Source File: /readme.source.md
To change this file edit the source file and then run MarkdownSnippets.
-->

# <img src="/src/icon.png" height="30px"> NServiceBus.SqlServer.Native

SQL Server Transport Native is a shim providing low-level access to the [NServiceBus SQL Server Transport](https://docs.particular.net/transports/sql/) with no NServiceBus or SQL Server Transport reference required.

<!-- toc -->
## Contents

  * [Community backed](#community-backed)
    * [Sponsors](#sponsors)
    * [Patrons](#patrons)
  * [Usage scenarios](#usage-scenarios)
  * [Main Queue](#main-queue)
    * [Queue management](#queue-management)
    * [Sending messages](#sending-messages)
    * [Reading messages](#reading-messages)
    * [Consuming messages](#consuming-messages)
  * [Delayed Queue](#delayed-queue)
    * [Queue management](#queue-management-1)
    * [Sending messages](#sending-messages-1)
    * [Reading messages](#reading-messages-1)
    * [Consuming messages](#consuming-messages-1)
  * [Headers](#headers)
  * [Subscriptions](#subscriptions)
    * [Table management](#table-management)
  * [Deduplication](#deduplication)
    * [Table management](#table-management-1)
    * [Sending messages](#sending-messages-2)
    * [Deduplication cleanup](#deduplication-cleanup)
    * [JSON headers](#json-headers)
    * [Copied header constants](#copied-header-constants)
    * [Duplicated timestamp functionality](#duplicated-timestamp-functionality)
  * [ConnectionHelpers](#connectionhelpers)
  * [SqlServer.HttpPassthrough](#sqlserverhttppassthrough)<!-- endtoc -->

<!--- StartOpenCollectiveBackers -->

[Already a Patron? skip past this section](#endofbacking)


## Community backed

**It is expected that all developers [become a Patron](https://opencollective.com/nservicebusextensions/contribute/patron-6976) to use this tool. [Go to licensing FAQ](https://github.com/NServiceBusExtensions/Home/#licensingpatron-faq)**

Thanks to the current backers.

<img src="https://opencollective.com/nservicebusextensions/tiers/patron.svg?width=890&avatarHeight=60&button=false">

<a href="#" id="endofbacking"></a>

<!--- EndOpenCollectiveBackers -->


## NuGet package

https://nuget.org/packages/NServiceBus.SqlServer.Native/


## Usage scenarios

 * **Error or Audit queue handling**: Allows to consume messages from error and audit queues, for example to move them to a long-term archive. NServiceBus expects to have a queue per message type, so NServiceBus endpoints are not suitable for processing error or audit queues. SQL Native allows manipulation or consumption of queues containing multiple types of messages.
 * **Corrupted or malformed messages**: Allows to process poison messages which can't be deserialized by NServiceBus. In SQL Native message headers and body are treated as a raw string and byte array, so corrupted or malformed messages can be read and manipulated in code to correct any problems.
 * **Deployment or decommission**: Allows to perform common operational activities, similar to [operations scripts](https://docs.particular.net/transports/sql/operations-scripting#native-send-the-native-send-helper-methods-in-c). Running [installers](https://docs.particular.net/nservicebus/operations/installers) requires starting a full endpoint. This is not always ideal during the execution of a deployment or decommission. SQL Native allows creating or deleting of queues with no running endpoint, and with significantly less code. This also makes it a better candidate for usage in deployment scripting languages like PowerShell.
 * **Bulk operations**: SQL Native supports sending and receiving of multiple messages within a single `SQLConnection` and `SQLTransaction`.
 * **Explicit connection and transaction management**: NServiceBus abstracts the `SQLConnection` and `SQLTransaction` creation and management. SQL Native allows any consuming code to manage the scope and settings of both the `SQLConnection` and `SQLTransaction`.
 * **Message pass through**: SQL Native reduces the amount of boilerplate code and simplifies development.



## Main Queue


### Queue management

Queue management for the [native delayed delivery](https://docs.particular.net/transports/sql/native-delayed-delivery) functionality.

See also [SQL Server Transport - SQL statements](https://docs.particular.net/transports/sql/sql-statements#installation).


#### Create

The queue can be created using the following:

<!-- snippet: CreateQueue -->
<a id='snippet-createqueue'/></a>
```cs
var manager = new QueueManager("endpointTable", sqlConnection);
await manager.Create();
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Main/MainQueue.cs#L14-L19' title='File snippet `createqueue` was extracted from'>snippet source</a> | <a href='#snippet-createqueue' title='Navigate to start of snippet `createqueue`'>anchor</a></sup>
<!-- endsnippet -->


#### Delete

The queue can be deleted using the following:

<!-- snippet: DeleteQueue -->
<a id='snippet-deletequeue'/></a>
```cs
var manager = new QueueManager("endpointTable", sqlConnection);
await manager.Drop();
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Main/MainQueue.cs#L24-L29' title='File snippet `deletequeue` was extracted from'>snippet source</a> | <a href='#snippet-deletequeue' title='Navigate to start of snippet `deletequeue`'>anchor</a></sup>
<!-- endsnippet -->


### Sending messages

Sending to the main transport queue.


#### Single

Sending a single message.

<!-- snippet: Send -->
<a id='snippet-send'/></a>
```cs
var manager = new QueueManager("endpointTable", sqlConnection);
var message = new OutgoingMessage(
    id: Guid.NewGuid(),
    headers: headers,
    bodyBytes: body);
await manager.Send(message);
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Main/MainQueue.cs#L37-L46' title='File snippet `send` was extracted from'>snippet source</a> | <a href='#snippet-send' title='Navigate to start of snippet `send`'>anchor</a></sup>
<!-- endsnippet -->


#### Batch

Sending a batch of messages.

<!-- snippet: SendBatch -->
<a id='snippet-sendbatch'/></a>
```cs
var manager = new QueueManager("endpointTable", sqlConnection);
var messages = new List<OutgoingMessage>
{
    new OutgoingMessage(
        id: Guid.NewGuid(),
        headers: headers1,
        bodyBytes: body1),
    new OutgoingMessage(
        id: Guid.NewGuid(),
        headers: headers2,
        bodyBytes: body2),
};
await manager.Send(messages);
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Main/MainQueue.cs#L57-L73' title='File snippet `sendbatch` was extracted from'>snippet source</a> | <a href='#snippet-sendbatch' title='Navigate to start of snippet `sendbatch`'>anchor</a></sup>
<!-- endsnippet -->


### Reading messages

"Reading" a message returns the data from the database without deleting it.


#### Single

Reading a single message.

<!-- snippet: Read -->
<a id='snippet-read'/></a>
```cs
var manager = new QueueManager("endpointTable", sqlConnection);
var message = await manager.Read(rowVersion: 10);

if (message != null)
{
    Console.WriteLine(message.Headers);
    if (message.Body != null)
    {
        using var reader = new StreamReader(message.Body);
        var bodyText = await reader.ReadToEndAsync();
        Console.WriteLine(bodyText);
    }
}
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Main/MainQueue.cs#L78-L94' title='File snippet `read` was extracted from'>snippet source</a> | <a href='#snippet-read' title='Navigate to start of snippet `read`'>anchor</a></sup>
<!-- endsnippet -->


#### Batch

Reading a batch of messages.

<!-- snippet: ReadBatch -->
<a id='snippet-readbatch'/></a>
```cs
var manager = new QueueManager("endpointTable", sqlConnection);
var result = await manager.Read(
    size: 5,
    startRowVersion: 10,
    action: async message =>
    {
        Console.WriteLine(message.Headers);
        if (message.Body == null)
        {
            return;
        }

        using var reader = new StreamReader(message.Body);
        var bodyText = await reader.ReadToEndAsync();
        Console.WriteLine(bodyText);
    });

Console.WriteLine(result.Count);
Console.WriteLine(result.LastRowVersion);
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Main/MainQueue.cs#L99-L121' title='File snippet `readbatch` was extracted from'>snippet source</a> | <a href='#snippet-readbatch' title='Navigate to start of snippet `readbatch`'>anchor</a></sup>
<!-- endsnippet -->


#### RowVersion tracking

For many scenarios, it is likely to be necessary to keep track of the last message `RowVersion` that was read. A lightweight implementation of the functionality is provided by `RowVersionTracker`. `RowVersionTracker` stores the current `RowVersion` in a table containing a single column and row.

<!-- snippet: RowVersionTracker -->
<a id='snippet-rowversiontracker'/></a>
```cs
var versionTracker = new RowVersionTracker();

// create table
await versionTracker.CreateTable(sqlConnection);

// save row version
await versionTracker.Save(sqlConnection, newRowVersion);

// get row version
var startingRow = await versionTracker.Get(sqlConnection);
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Main/ProcessingLoop.cs#L20-L33' title='File snippet `rowversiontracker` was extracted from'>snippet source</a> | <a href='#snippet-rowversiontracker' title='Navigate to start of snippet `rowversiontracker`'>anchor</a></sup>
<!-- endsnippet -->

Note that this is only one possible implementation of storing the current `RowVersion`.


#### Processing loop

For scenarios where continual processing (reading and executing some code with the result) of incoming messages is required, `MessageProcessingLoop` can be used. 

An example use case is monitoring an [error queue](https://docs.particular.net/nservicebus/recoverability/configure-error-handling). Some action should be taken when a message appears in the error queue, but it should remain in that queue in case it needs to be retried. 

Note that in the below snippet, the above `RowVersionTracker` is used for tracking the current `RowVersion`.

<!-- snippet: ProcessingLoop -->
<a id='snippet-processingloop'/></a>
```cs
var rowVersionTracker = new RowVersionTracker();

var startingRow = await rowVersionTracker.Get(sqlConnection);

async Task Callback(
    DbTransaction transaction,
    IncomingMessage message,
    CancellationToken cancellation)
{
    if (message.Body == null)
    {
        return;
    }

    using var reader = new StreamReader(message.Body);
    var bodyText = await reader.ReadToEndAsync();
    Console.WriteLine($"Message received in error message:\r\n{bodyText}");
}

void ErrorCallback(Exception exception)
{
    Environment.FailFast("Message processing loop failed", exception);
}

Task<DbTransaction> TransactionBuilder(CancellationToken cancellation)
{
    return ConnectionHelpers.BeginTransaction(connectionString, cancellation);
}

Task PersistRowVersion(
    DbTransaction transaction,
    long rowVersion,
    CancellationToken token)
{
    return rowVersionTracker.Save(sqlConnection, rowVersion, token);
}

var processingLoop = new MessageProcessingLoop(
    table: "error",
    delay: TimeSpan.FromSeconds(1),
    transactionBuilder: TransactionBuilder,
    callback: Callback,
    errorCallback: ErrorCallback,
    startingRow: startingRow,
    persistRowVersion: PersistRowVersion);
processingLoop.Start();

Console.ReadKey();

await processingLoop.Stop();
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Main/ProcessingLoop.cs#L38-L91' title='File snippet `processingloop` was extracted from'>snippet source</a> | <a href='#snippet-processingloop' title='Navigate to start of snippet `processingloop`'>anchor</a></sup>
<!-- endsnippet -->


### Consuming messages

"Consuming" a message returns the data from the database and also deletes that message.


#### Single

Consume a single message.

<!-- snippet: Consume -->
<a id='snippet-consume'/></a>
```cs
var manager = new QueueManager("endpointTable", sqlConnection);
var message = await manager.Consume();

if (message != null)
{
    Console.WriteLine(message.Headers);
    if (message.Body != null)
    {
        using var reader = new StreamReader(message.Body);
        var bodyText = await reader.ReadToEndAsync();
        Console.WriteLine(bodyText);
    }
}
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Main/MainQueue.cs#L126-L142' title='File snippet `consume` was extracted from'>snippet source</a> | <a href='#snippet-consume' title='Navigate to start of snippet `consume`'>anchor</a></sup>
<!-- endsnippet -->


#### Batch

Consuming a batch of messages.

<!-- snippet: ConsumeBatch -->
<a id='snippet-consumebatch'/></a>
```cs
var manager = new QueueManager("endpointTable", sqlConnection);
var result = await manager.Consume(
    size: 5,
    action: async message =>
    {
        Console.WriteLine(message.Headers);
        if (message.Body == null)
        {
            return;
        }

        using var reader = new StreamReader(message.Body);
        var bodyText = await reader.ReadToEndAsync();
        Console.WriteLine(bodyText);
    });

Console.WriteLine(result.Count);
Console.WriteLine(result.LastRowVersion);
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Main/MainQueue.cs#L147-L168' title='File snippet `consumebatch` was extracted from'>snippet source</a> | <a href='#snippet-consumebatch' title='Navigate to start of snippet `consumebatch`'>anchor</a></sup>
<!-- endsnippet -->


#### Consuming loop

For scenarios where continual consumption (consuming and executing some code with the result) of incoming messages is required, `MessageConsumingLoop` can be used.

An example use case is monitoring an [audit queue](https://docs.particular.net/nservicebus/operations/auditing). Some action should be taken when a message appears in the audit queue, and it should be purged from the queue to free up the storage space. 

<!-- snippet: ConsumeLoop -->
<a id='snippet-consumeloop'/></a>
```cs
async Task Callback(
    DbTransaction transaction,
    IncomingMessage message,
    CancellationToken cancellation)
{
    if (message.Body != null)
    {
        using var reader = new StreamReader(message.Body);
        var bodyText = await reader.ReadToEndAsync();
        Console.WriteLine($"Reply received:\r\n{bodyText}");
    }
}

Task<DbTransaction> TransactionBuilder(CancellationToken cancellation)
{
    return ConnectionHelpers.BeginTransaction(connectionString, cancellation);
}

void ErrorCallback(Exception exception)
{
    Environment.FailFast("Message consuming loop failed", exception);
}

// start consuming
var consumingLoop = new MessageConsumingLoop(
    table: "endpointTable",
    delay: TimeSpan.FromSeconds(1),
    transactionBuilder: TransactionBuilder,
    callback: Callback,
    errorCallback: ErrorCallback);
consumingLoop.Start();

// stop consuming
await consumingLoop.Stop();
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Main/ConsumingLoop.cs#L14-L51' title='File snippet `consumeloop` was extracted from'>snippet source</a> | <a href='#snippet-consumeloop' title='Navigate to start of snippet `consumeloop`'>anchor</a></sup>
<!-- endsnippet -->


## Delayed Queue


### Queue management

Queue management for the [native delayed delivery](https://docs.particular.net/transports/sql/native-delayed-delivery) functionality.

See also [SQL Server Transport - SQL statements](https://docs.particular.net/transports/sql/sql-statements#create-delayed-queue-table).


#### Create

The queue can be created using the following:

<!-- snippet: CreateDelayedQueue -->
<a id='snippet-createdelayedqueue'/></a>
```cs
var manager = new DelayedQueueManager("endpointTable.Delayed", sqlConnection);
await manager.Create();
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Delayed/DelayedQueue.cs#L14-L19' title='File snippet `createdelayedqueue` was extracted from'>snippet source</a> | <a href='#snippet-createdelayedqueue' title='Navigate to start of snippet `createdelayedqueue`'>anchor</a></sup>
<!-- endsnippet -->


#### Delete

The queue can be deleted using the following:

<!-- snippet: DeleteDelayedQueue -->
<a id='snippet-deletedelayedqueue'/></a>
```cs
var manager = new DelayedQueueManager("endpointTable.Delayed", sqlConnection);
await manager.Drop();
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Delayed/DelayedQueue.cs#L24-L29' title='File snippet `deletedelayedqueue` was extracted from'>snippet source</a> | <a href='#snippet-deletedelayedqueue' title='Navigate to start of snippet `deletedelayedqueue`'>anchor</a></sup>
<!-- endsnippet -->


### Sending messages


#### Single

Sending a single message.

<!-- snippet: SendDelayed -->
<a id='snippet-senddelayed'/></a>
```cs
var manager = new DelayedQueueManager("endpointTable.Delayed", sqlConnection);
var message = new OutgoingDelayedMessage(
    due: DateTime.UtcNow.AddDays(1),
    headers: headers,
    bodyBytes: body);
await manager.Send(message);
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Delayed/DelayedQueue.cs#L37-L46' title='File snippet `senddelayed` was extracted from'>snippet source</a> | <a href='#snippet-senddelayed' title='Navigate to start of snippet `senddelayed`'>anchor</a></sup>
<!-- endsnippet -->


#### Batch

Sending a batch of messages.

<!-- snippet: SendDelayedBatch -->
<a id='snippet-senddelayedbatch'/></a>
```cs
var manager = new DelayedQueueManager("endpointTable.Delayed", sqlConnection);
var messages = new List<OutgoingDelayedMessage>
{
    new OutgoingDelayedMessage(
        due: DateTime.UtcNow.AddDays(1),
        headers: headers1,
        bodyBytes: body1),
    new OutgoingDelayedMessage(
        due: DateTime.UtcNow.AddDays(1),
        headers: headers2,
        bodyBytes: body2),
};
await manager.Send(messages);
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Delayed/DelayedQueue.cs#L56-L72' title='File snippet `senddelayedbatch` was extracted from'>snippet source</a> | <a href='#snippet-senddelayedbatch' title='Navigate to start of snippet `senddelayedbatch`'>anchor</a></sup>
<!-- endsnippet -->


### Reading messages

"Reading" a message returns the data from the database without deleting it.


#### Single

Reading a single message.

<!-- snippet: ReadDelayed -->
<a id='snippet-readdelayed'/></a>
```cs
var manager = new DelayedQueueManager("endpointTable", sqlConnection);
var message = await manager.Read(rowVersion: 10);

if (message != null)
{
    Console.WriteLine(message.Headers);
    if (message.Body != null)
    {
        using var reader = new StreamReader(message.Body);
        var bodyText = await reader.ReadToEndAsync();
        Console.WriteLine(bodyText);
    }
}
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Delayed/DelayedQueue.cs#L77-L93' title='File snippet `readdelayed` was extracted from'>snippet source</a> | <a href='#snippet-readdelayed' title='Navigate to start of snippet `readdelayed`'>anchor</a></sup>
<!-- endsnippet -->


#### Batch

Reading a batch of messages.

<!-- snippet: ReadDelayedBatch -->
<a id='snippet-readdelayedbatch'/></a>
```cs
var manager = new DelayedQueueManager("endpointTable", sqlConnection);
var result = await manager.Read(
    size: 5,
    startRowVersion: 10,
    action: async message =>
    {
        Console.WriteLine(message.Headers);
        if (message.Body == null)
        {
            return;
        }

        using var reader = new StreamReader(message.Body);
        var bodyText = await reader.ReadToEndAsync();
        Console.WriteLine(bodyText);
    });

Console.WriteLine(result.Count);
Console.WriteLine(result.LastRowVersion);
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Delayed/DelayedQueue.cs#L98-L120' title='File snippet `readdelayedbatch` was extracted from'>snippet source</a> | <a href='#snippet-readdelayedbatch' title='Navigate to start of snippet `readdelayedbatch`'>anchor</a></sup>
<!-- endsnippet -->


### Consuming messages

"Consuming" a message returns the data from the database and also deletes that message.


#### Single

Consume a single message.

<!-- snippet: ConsumeDelayed -->
<a id='snippet-consumedelayed'/></a>
```cs
var manager = new DelayedQueueManager("endpointTable", sqlConnection);
var message = await manager.Consume();

if (message != null)
{
    Console.WriteLine(message.Headers);
    if (message.Body != null)
    {
        using var reader = new StreamReader(message.Body);
        var bodyText = await reader.ReadToEndAsync();
        Console.WriteLine(bodyText);
    }
}
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Delayed/DelayedQueue.cs#L125-L141' title='File snippet `consumedelayed` was extracted from'>snippet source</a> | <a href='#snippet-consumedelayed' title='Navigate to start of snippet `consumedelayed`'>anchor</a></sup>
<!-- endsnippet -->


#### Batch

Consuming a batch of messages.

<!-- snippet: ConsumeDelayedBatch -->
<a id='snippet-consumedelayedbatch'/></a>
```cs
var manager = new DelayedQueueManager("endpointTable", sqlConnection);
var result = await manager.Consume(
    size: 5,
    action: async message =>
    {
        Console.WriteLine(message.Headers);
        if (message.Body == null)
        {
            return;
        }

        using var reader = new StreamReader(message.Body);
        var bodyText = await reader.ReadToEndAsync();
        Console.WriteLine(bodyText);
    });

Console.WriteLine(result.Count);
Console.WriteLine(result.LastRowVersion);
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Delayed/DelayedQueue.cs#L146-L167' title='File snippet `consumedelayedbatch` was extracted from'>snippet source</a> | <a href='#snippet-consumedelayedbatch' title='Navigate to start of snippet `consumedelayedbatch`'>anchor</a></sup>
<!-- endsnippet -->


## Headers

There is a headers helpers class `NServiceBus.Transport.SqlServerNative.Headers`.

It contains several [header](https://docs.particular.net/nservicebus/messaging/headers) related utilities.


## Subscriptions

Queue management for the [native publish subscribe](https://docs.particular.net/transports/sql/native-publish-subscribe) functionality.


### Table management


#### Create

The table can be created using the following:

<!-- snippet: CreateSubscriptionTable -->
<a id='snippet-createsubscriptiontable'/></a>
```cs
var manager = new SubscriptionManager("SubscriptionRouting", sqlConnection);
await manager.Create();
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Subscription/Subscription.cs#L11-L16' title='File snippet `createsubscriptiontable` was extracted from'>snippet source</a> | <a href='#snippet-createsubscriptiontable' title='Navigate to start of snippet `createsubscriptiontable`'>anchor</a></sup>
<!-- endsnippet -->


#### Delete

The table can be deleted using the following:

<!-- snippet: DeleteSubscriptionTable -->
<a id='snippet-deletesubscriptiontable'/></a>
```cs
var manager = new SubscriptionManager("SubscriptionRouting", sqlConnection);
await manager.Drop();
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Subscription/Subscription.cs#L21-L26' title='File snippet `deletesubscriptiontable` was extracted from'>snippet source</a> | <a href='#snippet-deletesubscriptiontable' title='Navigate to start of snippet `deletesubscriptiontable`'>anchor</a></sup>
<!-- endsnippet -->


## Deduplication

Some scenarios, such as HTTP message pass through, require message deduplication.


### Table management


#### Create

The table can be created using the following:

<!-- snippet: CreateDeduplicationTable -->
<a id='snippet-creatededuplicationtable'/></a>
```cs
var manager = new DedupeManager(sqlConnection, "DeduplicationTable");
await manager.Create();
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Deduplication/Deduplication.cs#L14-L19' title='File snippet `creatededuplicationtable` was extracted from'>snippet source</a> | <a href='#snippet-creatededuplicationtable' title='Navigate to start of snippet `creatededuplicationtable`'>anchor</a></sup>
<!-- endsnippet -->


#### Delete

The table can be deleted using the following:

<!-- snippet: DeleteDeduplicationTable -->
<a id='snippet-deletededuplicationtable'/></a>
```cs
var manager = new DedupeManager(sqlConnection, "DeduplicationTable");
await manager.Drop();
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Deduplication/Deduplication.cs#L24-L29' title='File snippet `deletededuplicationtable` was extracted from'>snippet source</a> | <a href='#snippet-deletededuplicationtable' title='Navigate to start of snippet `deletededuplicationtable`'>anchor</a></sup>
<!-- endsnippet -->


### Sending messages

Sending to the main transport queue with deduplication.


#### Single

Sending a single message with deduplication.

<!-- snippet: SendWithDeduplication -->
<a id='snippet-sendwithdeduplication'/></a>
```cs
var manager = new QueueManager(
    "endpointTable",
    sqlConnection,
    "DeduplicationTable");
var message = new OutgoingMessage(
    id: Guid.NewGuid(),
    headers: headers,
    bodyBytes: body);
await manager.Send(message);
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Deduplication/Deduplication.cs#L37-L49' title='File snippet `sendwithdeduplication` was extracted from'>snippet source</a> | <a href='#snippet-sendwithdeduplication' title='Navigate to start of snippet `sendwithdeduplication`'>anchor</a></sup>
<!-- endsnippet -->


#### Batch

Sending a batch of messages with deduplication.

<!-- snippet: SendBatchWithDeduplication -->
<a id='snippet-sendbatchwithdeduplication'/></a>
```cs
var manager = new QueueManager(
    "endpointTable",
    sqlConnection,
    "DeduplicationTable");
var messages = new List<OutgoingMessage>
{
    new OutgoingMessage(
        id: Guid.NewGuid(),
        headers: headers1,
        bodyBytes: body1),
    new OutgoingMessage(
        id: Guid.NewGuid(),
        headers: headers2,
        bodyBytes: body2),
};
await manager.Send(messages);
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Deduplication/Deduplication.cs#L84-L103' title='File snippet `sendbatchwithdeduplication` was extracted from'>snippet source</a> | <a href='#snippet-sendbatchwithdeduplication' title='Navigate to start of snippet `sendbatchwithdeduplication`'>anchor</a></sup>
<!-- endsnippet -->


### Deduplication cleanup

Deduplication records need to live for a period of time after the initial corresponding message has been send. In this way an subsequent message, with the same message id, can be ignored. This necessitates a periodic cleanup process of deduplication records. This is achieved by using `DeduplicationCleanerJob`:

At application startup, start an instance of `DeduplicationCleanerJob`.

<!-- snippet: DeduplicationCleanerJobStart -->
<a id='snippet-deduplicationcleanerjobstart'/></a>
```cs
var cleaner = new DedupeCleanerJob(
    table: "Deduplication",
    connectionBuilder: cancellation =>
    {
        return ConnectionHelpers.OpenConnection(connectionString, cancellation);
    },
    criticalError: exception => { },
    expireWindow: TimeSpan.FromHours(1),
    frequencyToRunCleanup: TimeSpan.FromMinutes(10));
cleaner.Start();
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Deduplication/Deduplication.cs#L55-L68' title='File snippet `deduplicationcleanerjobstart` was extracted from'>snippet source</a> | <a href='#snippet-deduplicationcleanerjobstart' title='Navigate to start of snippet `deduplicationcleanerjobstart`'>anchor</a></sup>
<!-- endsnippet -->

Then at application shutdown stop the instance.

<!-- snippet: DeduplicationCleanerJobStop -->
<a id='snippet-deduplicationcleanerjobstop'/></a>
```cs
await cleaner.Stop();
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Deduplication/Deduplication.cs#L70-L74' title='File snippet `deduplicationcleanerjobstop` was extracted from'>snippet source</a> | <a href='#snippet-deduplicationcleanerjobstop' title='Navigate to start of snippet `deduplicationcleanerjobstop`'>anchor</a></sup>
<!-- endsnippet -->


### JSON headers


#### Serialization

Serialize a `Dictionary<string, string>` to a JSON string.

<!-- snippet: Serialize -->
<a id='snippet-serialize'/></a>
```cs
var headers = new Dictionary<string, string>
{
    {Headers.EnclosedMessageTypes, "SendMessage"}
};
var serialized = Headers.Serialize(headers);
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Headers.cs#L9-L17' title='File snippet `serialize` was extracted from'>snippet source</a> | <a href='#snippet-serialize' title='Navigate to start of snippet `serialize`'>anchor</a></sup>
<!-- endsnippet -->


#### Deserialization

Deserialize a JSON string to a `Dictionary<string, string>`.

<!-- snippet: Deserialize -->
<a id='snippet-deserialize'/></a>
```cs
var headers = Headers.DeSerialize(headersString);
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/Headers.cs#L24-L28' title='File snippet `deserialize` was extracted from'>snippet source</a> | <a href='#snippet-deserialize' title='Navigate to start of snippet `deserialize`'>anchor</a></sup>
<!-- endsnippet -->


### Copied header constants

Contains all the string constants copied from `NServiceBus.Headers`.

 
### Duplicated timestamp functionality

A copy of the [timestamp format methods](https://docs.particular.net/nservicebus/messaging/headers#timestamp-format) `ToWireFormattedString` and `ToUtcDateTime`. 


## ConnectionHelpers

The APIs of this extension target either a `SQLConnection` and `SQLTransaction`. Given that in configuration those values are often expressed as a connection string, `ConnectionHelpers` supports converting that string to a `SQLConnection` or `SQLTransaction`. It provides two methods `OpenConnection` and `BeginTransaction` with the effective implementation of those methods being:

<!-- snippet: ConnectionHelpers -->
<a id='snippet-connectionhelpers'/></a>
```cs
public static async Task<DbConnection> OpenConnection(
    string connectionString,
    CancellationToken cancellation)
{
    var connection = new SqlConnection(connectionString);
    try
    {
        await connection.OpenAsync(cancellation);
        return connection;
    }
    catch
    {
        connection.Dispose();
        throw;
    }
}

public static async Task<DbTransaction> BeginTransaction(
    string connectionString,
    CancellationToken cancellation)
{
    var connection = await OpenConnection(connectionString, cancellation);
    return connection.BeginTransaction();
}
```
<sup><a href='/src/SqlServer.Native.Tests/Snippets/ConnectionHelpers.cs#L8-L35' title='File snippet `connectionhelpers` was extracted from'>snippet source</a> | <a href='#snippet-connectionhelpers' title='Navigate to start of snippet `connectionhelpers`'>anchor</a></sup>
<!-- endsnippet -->


## MARS <!-- include: mars. path: /docs/mdsource/mars.include.md -->

All [SqlConnection](https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlconnection.aspx)s must have [Multiple Active Result Sets (MARS)
](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/multiple-active-result-sets-mars) as multiple concurrent async request can be performed. <!-- end include: mars. path: /docs/mdsource/mars.include.md -->


## SqlServer.HttpPassthrough

SQL HTTP Passthrough provides a bridge between an HTTP stream (via JavaScript on a web page) and the [SQL Server transport](https://docs.particular.net/transports/sql/).

See [docs/http-passthrough.md](docs/http-passthrough.md).


## Release Notes

See [closed milestones](../../milestones?state=closed).


## Icon

[Spear](https://thenounproject.com/term/spear/814550/) designed by [Aldric Rodríguez](https://thenounproject.com/aldricroib2/) from [The Noun Project](https://thenounproject.com/).
