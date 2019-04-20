using System;
using System.Runtime.InteropServices;

namespace Projecticate
{
    public class PlaceholderVersionInfo
    {
        public byte[] ProviderId { get; }
        public byte[] ContentId { get; }

        public PlaceholderVersionInfo(byte[] providerId, byte[] contentId)
        {
            ProviderId = providerId;
            ContentId = contentId;
        }
    }
}