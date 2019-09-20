using System;
using System.Collections.Generic;
using ConnectApp.Constants;
using ConnectApp.Models.Api;
using ConnectApp.Models.Model;
using ConnectApp.Utils;
using Newtonsoft.Json;
using RSG;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace ConnectApp.Api {
    public static class ChannelApi {
        public static Promise<FetchChannelsResponse> FetchChannels(int page) {
            var promise = new Promise<FetchChannelsResponse>();
            var para = new Dictionary<string, object> {
                {"discover", "true"},
                {"discoverPage", page},
                {"joined", "true"},
            };
            var request = HttpManager.GET($"{Config.apiAddress}/api/connectapp/channels", parameter: para);
            HttpManager.resume(request).Then(responseText => {
                var publicChannelsResponse = JsonConvert.DeserializeObject<FetchChannelsResponse>(responseText);
                promise.Resolve(publicChannelsResponse);
            }).Catch(exception => { promise.Reject(exception); });
            return promise;
        }
        
        public static Promise<FetchChannelMessagesResponse> FetchChannelMessages(
            string channelId, string before = null, string after = null) {
            D.assert(before == null || after == null);
            var promise = new Promise<FetchChannelMessagesResponse>();
            var para = new Dictionary<string, object> { };
            if (before != null) {
                para["before"] = before;
            } else if (after != null) {
                para["after"] = after;
            }
            var request = HttpManager.GET($"{Config.apiAddress}/api/channels/{channelId}/messages", parameter: para);
            HttpManager.resume(request).Then(responseText => {
                var channelMessagesResponse = JsonConvert.DeserializeObject<FetchChannelMessagesResponse>(responseText);
                promise.Resolve(channelMessagesResponse);
            }).Catch(exception => { promise.Reject(exception); });
            return promise;
        }
        
        public static Promise<FetchChannelMembersResponse> FetchChannelMembers(string channelId, int offset = 0) {
            var promise = new Promise<FetchChannelMembersResponse>();
            var request = HttpManager.GET($"{Config.apiAddress}/api/connectapp/channels/{channelId}/members",
                parameter: new Dictionary<string, object> {
                    {"offset", offset}
                });
            HttpManager.resume(request).Then(responseText => {
                var channelMemberResponse = JsonConvert.DeserializeObject<FetchChannelMembersResponse>(responseText);
                promise.Resolve(channelMemberResponse);
            }).Catch(exception => { promise.Reject(exception); });
            return promise;
        }
        
        public static Promise<List<JoinChannelResponse>> JoinChannel(string channelId, string groupId = null) {
            var promise = new Promise<List<JoinChannelResponse>>();
            var request = HttpManager.POST($"{Config.apiAddress}/api/channels/{channelId}/join",
                parameter: new Dictionary<string, string> {
                    {"channelId", channelId}
            });
//            if (!string.IsNullOrEmpty(groupId)) {
//                request = HttpManager.POST($"{Config.apiAddress}/api/group/{groupId}/requestJoin",
//                parameter: new Dictionary<string, string> {
//                    {"groupId", groupId}
//                });
//            }
            Debug.Log(request.uri);
            HttpManager.resume(request).Then(responseText => {
                var joinChannelResponse = JsonConvert.DeserializeObject<List<JoinChannelResponse>>(responseText);
                promise.Resolve(joinChannelResponse);
            }).Catch(exception => { promise.Reject(exception); });
            return promise;
        }
        
        public static Promise<LeaveChannelResponse> LeaveChannel(string channelId, string groupId = null) {
            var promise = new Promise<LeaveChannelResponse>();
            var request = HttpManager.POST($"{Config.apiAddress}/api/channels/{channelId}/leave",
                parameter: new Dictionary<string, string> {
                    {"channelId", channelId}
            });
//            if (!string.IsNullOrEmpty(groupId)) {
//                request = HttpManager.POST($"{Config.apiAddress}/api/group/{groupId}/deleteMember",
//                parameter: new Dictionary<string, string> {
//                    {"groupId", groupId}
//                });
//            }
            Debug.Log(request.uri);
            HttpManager.resume(request).Then(responseText => {
                var leaveChannelResponse = JsonConvert.DeserializeObject<LeaveChannelResponse>(responseText);
                promise.Resolve(leaveChannelResponse);
            }).Catch(exception => { promise.Reject(exception); });
            return promise;
        }
        
        public static Promise<FetchSendMessageResponse> SendImage(string channelId, string content, string nonce,
            string imageData, string parentMessageId = "") {
            var data = Convert.FromBase64String(imageData);
            var promise = new Promise<FetchSendMessageResponse>();
            var para = new List<List<object>> {
                new List<object>{"channel", channelId},
                new List<object>{"content", content},
                new List<object>{"parentMessageId", parentMessageId},
                new List<object>{"nonce", nonce},
                new List<object>{"size", $"{data.Length}"},
                new List<object>{"file", data}
            };
            var request = HttpManager.POST($"{Config.apiAddress}/api/channels/{channelId}/messages/attachments",
                para, true, "image.png", "image/png");
            HttpManager.resume(request).Then(responseText => {
                var sendMessageResponse = new FetchSendMessageResponse {
                    channelId = channelId,
                    content = content,
                    nonce = nonce
                };
                promise.Resolve(sendMessageResponse);
            }).Catch(exception => { promise.Reject(exception); });
            return promise;
        }
    }
}