using Rewind.Store;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rewind.Sync.Requests
{
    public class ClientSyncRequest
    {
        public string UserId { get; set; }
        public StoreKey StoreKey { get; set; }

        public const string InvokeKey = "OnClientSync";
    }
}
