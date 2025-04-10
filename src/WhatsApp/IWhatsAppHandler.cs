﻿using System.Threading.Tasks;

namespace Devlooped.WhatsApp;

/// <summary>
/// Interface for handling incoming WhatsApp messages.
/// </summary>
public interface IWhatsAppHandler
{
    /// <summary>
    /// Handles the incoming message from WhatsApp.
    /// </summary>
    /// <param name="message">The received message.</param>
    /// <remarks>
    /// If this method throws, it will be retried up to the max dequeue count. The default 
    /// is 5, and can be configured in <c>host.json</c> as follows:
    /// <code>
    /// {
    ///   "version": "2.0",
    ///   "extensions": {
    ///     "queues": {
    ///       "maxDequeueCount": 5
    ///     }
    ///   }
    /// }
    /// </code>
    /// After the max dequeue retries, the message will be moved to the <c>whatsapp-poison</c> 
    /// queue.
    /// </remarks>
    Task HandleAsync(Message message);
}
