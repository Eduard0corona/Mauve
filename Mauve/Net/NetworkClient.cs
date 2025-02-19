﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Mauve.Extensibility;

namespace Mauve.Net
{
    /// <summary>
    /// Represents a <see langword="abstract"/> implementation of <see cref="INetworkClient"/> that handles exceptions and response codes natively.
    /// </summary>
    /// <inheritdoc/>
    public abstract class NetworkClient<TRequest, TIn> : INetworkClient<TRequest, TIn>
        where TRequest : INetworkRequest<TIn>
    {

        #region Properties

        public NetworkConnectionInformation ConnectionInformation { get; set; }

        #endregion

        public NetworkClient(NetworkConnectionInformation connectionInformation) =>
            ConnectionInformation = connectionInformation;

        #region Public Methods

        public virtual INetworkResponse<TOut> Execute<TOut>(TRequest request)
        {
            TOut response = default;
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string responseMessage = string.Empty;
            try
            {
                response = ExecuteRequest<TOut>(request);
                statusCode = HttpStatusCode.OK;
                responseMessage = "Success.";
            } catch (Exception e)
            {
                IEnumerable<Exception> flattenedExceptionTree = e.Flatten();
                IEnumerable<string> exeptionMessages = flattenedExceptionTree.Select(exception => exception.Message);
                responseMessage = string.Join(" ", exeptionMessages);
            }

            // Return the network response.
            return new NetworkResponse<TOut>
            {
                Content = response,
                StatusCode = statusCode,
                Message = responseMessage
            };
        }
        public async Task<INetworkResponse<TOut>> ExecuteAsync<TOut>(TRequest request) =>
            await ExecuteAsync<TOut>(request, CancellationToken.None);
        public async Task<INetworkResponse<TOut>> ExecuteAsync<TOut>(TRequest request, CancellationToken cancellationToken) =>
            await Task.Run(() => Execute<TOut>(request), cancellationToken);

        #endregion

        #region Protected Methods

        /// <summary>
        /// Executes the specified <see cref="INetworkRequest{T}"/>.
        /// </summary>
        /// <param name="request">The request to execute.</param>
        /// <returns>Returns the appropriate <see cref="TOut"/> instance representing the result of executing the specified <see cref="TRequest"/>.</returns>
        protected abstract TOut ExecuteRequest<TOut>(TRequest request);

        #endregion

    }
}
