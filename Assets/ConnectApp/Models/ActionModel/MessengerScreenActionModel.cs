using System;
using RSG;

namespace ConnectApp.Models.ActionModel {
    public class MessengerScreenActionModel : BaseActionModel {
        public Action pushToNotifications;
        public Action pushToDiscoverChannels;
        public Action updateNewNotification;
        public Action<string> pushToChannel;
        public Action<string> pushToChannelDetail;
        public Func<int, bool, IPromise> fetchChannels;
        public Func<IPromise> fetchCreateChannelFilterIds;
        public Action<string> startJoinChannel;
        public Action<string, string> joinChannel;
    }
}