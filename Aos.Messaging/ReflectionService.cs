//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aos.Messaging
{
    /// <summary>
    /// Service which is handling messages and uses reflection to invoke properties/methods/events.
    /// </summary>
    /// <remarks>
    /// Members of the reflection-object (rootObject) decorated with RoutesAttribute can be used via 
    /// messaging. The RoutesAttribute is supported on properties/methods and events.
    /// For more information please have a look at the unit-tests.
    /// </remarks>
    public class ReflectionService : IService
    {
        private object rootObject;
        private Dictionary<string, List<SubscriptionInfo>> subscriptionInfoMap = new Dictionary<string, List<SubscriptionInfo>>();

        public ReflectionService(object rootObject)
        {
            this.rootObject = rootObject;
        }

        public static Delegate ConvertDelegate(Delegate originalDelegate, Type targetDelegateType)
        {
            return Delegate.CreateDelegate(
                targetDelegateType,
                originalDelegate.Target,
                originalDelegate.Method);
        }

        public void Request(Message message, Action<Message> onResponse, Action<Message> onError)
        {
            try
            {
                if (TryHandleProperty(message, onResponse))
                {
                    return;
                }

                if (TryHandleMethod(message, onResponse))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex);
                var response = Message.CreateFailedResponse(message, JsonRpcErrors.ServerException);
                response.Error["message"] = ex.InnerException?.Message ?? ex.Message;
                response.Error["data"] = ex.ToString();
                onResponse(response);
                return;
            }
            onResponse(Message.CreateFailedResponse(message, JsonRpcErrors.MethodNotFound));
        }

        public List<string> GetRoutes()
        {
            Dictionary<string, bool> routes = new Dictionary<string, bool>();
            AddPropertyRoutes(routes);

            AddMemberRoutes(routes, this.rootObject.GetType().GetEvents());
            AddMemberRoutes(routes, this.rootObject.GetType().GetMethods());

            return routes.Keys.ToList();
        }

        public void Subscribe(string messageCommand, Action<Message> onMessage)
        {
            var eventInfo = FindEventInfo(messageCommand);
            if (eventInfo == null)
            {
                Log.Debug($"No eventInfo found for subscription {messageCommand}");
                return;
            }

            List<SubscriptionInfo> subscriptionInfos;
            this.subscriptionInfoMap.TryGetValue(messageCommand, out subscriptionInfos);
            if (subscriptionInfos == null)
            {
                subscriptionInfos = new List<SubscriptionInfo>();
                this.subscriptionInfoMap[messageCommand] = subscriptionInfos;
            }

            var subscription = subscriptionInfos.FirstOrDefault(s => s.NotificationChannel == onMessage);
            if (subscription != null)
            {
                subscription.SubscriptionCount++;
            }
            else
            {
                subscriptionInfos.Add(CreateSubbsSubscriptionInfo(messageCommand, onMessage, eventInfo));
            }

            // For some subscription an initial notification is sent, with that the clients do not need a get upfront.
            TrySendInitialNotification(messageCommand, onMessage, eventInfo);
        }

        public void Unsubscribe(string messageCommand, Action<Message> onMessage)
        {
            List<SubscriptionInfo> subscriptionInfos;
            if (!this.subscriptionInfoMap.TryGetValue(messageCommand, out subscriptionInfos) || subscriptionInfos == null)
            {
                return;
            }

            var subscriptionInfo = subscriptionInfos.FirstOrDefault(s => s.NotificationChannel == onMessage);
            if (subscriptionInfo == null)
            {
                return;
            }

            subscriptionInfo.SubscriptionCount--;
            if (subscriptionInfo.SubscriptionCount == 0)
            {
                subscriptionInfos.Remove(subscriptionInfo);
                RemoveEventHandler(messageCommand, subscriptionInfo);
            }

            if (subscriptionInfos?.Count == 0)
            {
                this.subscriptionInfoMap.Remove(messageCommand);
            }
        }

        private bool TryHandleMethod(Message message, Action<Message> onResponse)
        {
            MethodInfo methodInfo = FindMethodInfo(message.Method);
            if (methodInfo == null)
            {
                return false;
            }

            var parameters = GetParameters(message, methodInfo);
            var result = methodInfo.Invoke(this.rootObject, parameters);

            Message response = methodInfo.ReturnType == typeof(void)
                ? Message.CreateResponse(message, "OK")
                : Message.CreateResponse(message, result);

            onResponse(response);
            return true;
        }

        private object[] GetParameters(Message message, MethodInfo methodInfo)
        {
            object[] parameters = null;
            var methodParams = methodInfo.GetParameters();
            if (methodParams.Length == 1)
            {
                parameters = new object[1];
                parameters[0] = Message.ToType(methodParams[0].ParameterType, message.Params);
            }
            return parameters;
        }

        private MethodInfo FindMethodInfo(string method)
        {
            return FindMemberInfo(method, this.rootObject.GetType().GetMethods());
        }

        private PropertyInfo FindProperty(string method)
        {
            if (method.EndsWith("/set") || method.EndsWith("/get"))
            {
                method = method.Substring(0, method.Length - "/set".Length);
            }
            return FindMemberInfo(method, this.rootObject.GetType().GetProperties());
        }

        private bool TryHandleProperty(Message message, Action<Message> onResponse)
        {
            var property = FindProperty(message.Method);
            if (property == null)
            {
                return false;
            }

            Message response;
            if (message.Params == null || message.Method.EndsWith("/get"))
            {
                object value = property.GetValue(this.rootObject);
                response = Message.CreateResponse(message, value);
            }
            else
            {
                var value = Message.ToType(property.PropertyType, message.Params);
                property.SetValue(this.rootObject, value);
                response = Message.CreateResponse(message, "OK");
            }
            onResponse(response);
            return true;
        }

        private void TrySendInitialNotification(string messageCommand, Action<Message> onMessage, EventInfo eventInfo)
        {
            var methodInfo = FindMethodInfo(messageCommand);
            var routingAttribute = eventInfo.GetCustomAttributes(typeof(RoutingAttribute)).FirstOrDefault() as RoutingAttribute;
            if (methodInfo != null && methodInfo.ReturnType != typeof(void) && routingAttribute != null && routingAttribute.AutoPublishOnSubscribe)
            {
                var result = methodInfo.Invoke(this.rootObject, null);
                var notification = Message.CreateNotification(messageCommand);
                notification.Params = Message.ToJObject(result);
                onMessage(notification);
            }
        }

        private T FindMemberInfo<T>(string messageCommand, T[] infoList) where T : MemberInfo
        {
            foreach (var info in infoList)
            {
                var routingAttribute = info.GetCustomAttributes(typeof(RoutingAttribute)).FirstOrDefault() as RoutingAttribute;
                if (routingAttribute != null && routingAttribute.Route == messageCommand)
                {
                    return info;
                }
            }
            return null;
        }

        private EventInfo FindEventInfo(string messageCommand)
        {
            return FindMemberInfo(messageCommand, this.rootObject.GetType().GetEvents());
        }

        private SubscriptionInfo CreateSubbsSubscriptionInfo(string messageCommand, Action<Message> onMessage, EventInfo eventInfo)
        {
            Action<object, object> handler = (s, e) => HandleEvent(s, e, onMessage, messageCommand);
            Delegate convertedhandler = ConvertDelegate(handler, eventInfo.EventHandlerType);
            SubscriptionInfo subscriptionInfo = new SubscriptionInfo { Handler = convertedhandler, NotificationChannel = onMessage, SubscriptionCount = 1 };
            eventInfo.AddEventHandler(this.rootObject, convertedhandler);
            return subscriptionInfo;
        }

        private void RemoveEventHandler(string messageCommand, SubscriptionInfo subscriptionInfo)
        {
            var eventInfo = FindEventInfo(messageCommand);
            eventInfo?.RemoveEventHandler(this.rootObject, subscriptionInfo.Handler);
        }

        public void HandleEvent(object sender, object eventArgs, Action<Message> publish, string method)
        {
            Message notification = null;
            if (eventArgs.GetType() == typeof(EventArgs))
            {
                notification = Message.CreateNotification(method);
            }
            else
            {
                notification = Message.CreateNotification(method);
                notification.Params = Message.ToJObject(GetValue(eventArgs));
            }
            publish(notification);
        }

        private object GetValue(object eventArgs)
        {
            var valueInfo = eventArgs.GetType().GetProperty("Value");
            return valueInfo != null ? valueInfo.GetValue(eventArgs) : eventArgs;
        }

        private void AddMemberRoutes(Dictionary<string, bool> routes, MemberInfo[] memberInfos)
        {
            foreach (var memberInfo in memberInfos)
            {
                var routingAttribute = memberInfo.GetCustomAttributes(typeof(RoutingAttribute)).FirstOrDefault() as RoutingAttribute;
                if (routingAttribute != null)
                {
                    routes[routingAttribute.Route] = true;
                }
            }
        }

        private void AddPropertyRoutes(Dictionary<string, bool> routes)
        {
            var properties = this.rootObject.GetType().GetProperties();
            foreach (var propertyInfo in properties)
            {
                var routingAttribute = propertyInfo.GetCustomAttributes(typeof(RoutingAttribute)).FirstOrDefault() as RoutingAttribute;
                if (routingAttribute != null)
                {
                    if (propertyInfo.CanRead)
                    {
                        routes[routingAttribute.Route + "/get"] = true;
                    }
                    if (propertyInfo.CanWrite)
                    {
                        routes[routingAttribute.Route + "/set"] = true;
                    }
                }
            }
        }

        private class SubscriptionInfo
        {
            public Action<Message> NotificationChannel { get; set; }
            public int SubscriptionCount { get; set; }
            public Delegate Handler { get; set; }
        }
    }
}