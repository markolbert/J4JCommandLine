using System.Collections.Generic;
using J4JSoftware.Logging;

#pragma warning disable 8618

namespace J4JSoftware.Binder.Tests
{
    public class LogChannelsConfig : LogChannels
    {
        public DebugConfig Debug { get; set; }

        public override IEnumerator<IChannelConfig> GetEnumerator()
        {
            yield return Debug;
        }
    }
}
