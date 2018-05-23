﻿using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NServiceBus.Transport.SqlServerNative;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    public class PassThroughConfiguration
    {
        internal Func<CancellationToken, Task<SqlConnection>> connectionFunc;
        internal string originatingMachine = Environment.MachineName;
        internal string originatingEndpoint = "SqlHttpPassThrough";
        internal Func<string, Table> convertDestination = destination => destination;
        internal Action<HttpContext, PassthroughMessage> sendCallback = (context, message) => { };
        internal Table deduplicationTable = "Deduplication";
        internal Table attachmentsTable = new Table("MessageAttachments");

        public PassThroughConfiguration(
            Func<CancellationToken, Task<SqlConnection>> connectionFunc)
        {
            Guard.AgainstNull(connectionFunc, nameof(connectionFunc));
            this.connectionFunc = connectionFunc.WrapFunc(nameof(connectionFunc));
        }

        public void OriginatingInfo(string endpoint, string machine)
        {
            Guard.AgainstNullOrEmpty(endpoint, nameof(endpoint));
            Guard.AgainstNullOrEmpty(machine, nameof(machine));
            originatingMachine = machine;
            originatingEndpoint = endpoint;
        }

        public void SendingCallback(Action<HttpContext, PassthroughMessage> callback)
        {
            Guard.AgainstNull(callback, nameof(callback));
            sendCallback = callback.WrapFunc(nameof(SendingCallback));
        }

        public void DestinationConverter(Func<string, Table> convert)
        {
            Guard.AgainstNull(convert, nameof(convert));
            convertDestination = convert.WrapFunc(nameof(DestinationConverter));
        }

        public void Deduplication(Table table)
        {
            Guard.AgainstNull(table, nameof(table));
            deduplicationTable = table;
        }

        public void Attachments(Table table)
        {
            Guard.AgainstNull(table, nameof(table));
            attachmentsTable = table;
        }
    }
}