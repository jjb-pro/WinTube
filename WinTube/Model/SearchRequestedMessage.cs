using CommunityToolkit.Mvvm.Messaging.Messages;
using Windows.System;

namespace WinTube.Model
{
    public sealed class SearchRequestedMessage
    {
        public string Query { get; }

        public SearchRequestedMessage(string query)
        {
            Query = query;
        }
    }
}