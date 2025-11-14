using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp.Flows;

/// <summary>
/// Base record for all Flow API requests.
/// </summary>
public abstract record FlowRequest;

/// <summary>
/// Base record for all Flow API responses.
/// </summary>
public abstract record FlowResponse;

/// <summary>
/// Request to create a new Flow.
/// </summary>
/// <param name="Name">Flow name.</param>
/// <param name="Categories">A list of Flow categories.</param>
/// <param name="FlowJson">Flow's JSON encoded as string.</param>
/// <param name="Publish">Indicates whether Flow should also get published.</param>
/// <param name="CloneFlowId">ID of source Flow to clone.</param>
/// <param name="EndpointUri">The URL of the WA Flow Endpoint.</param>
public record CreateFlowRequest(string Name, string[] Categories, string? FlowJson = null, bool? Publish = null, string? CloneFlowId = null, string? EndpointUri = null) : FlowRequest;

/// <summary>
/// Response from creating a Flow.
/// </summary>
/// <param name="Id">The ID of the created Flow.</param>
/// <param name="Success">Indicates if the operation was successful.</param>
/// <param name="ValidationErrors">List of validation errors, if any.</param>
public record CreateFlowResponse(string Id, bool Success, ValidationError[]? ValidationErrors = null) : FlowResponse;

/// <summary>
/// Validation error from Flow JSON.
/// </summary>
/// <param name="Error">Error code.</param>
/// <param name="ErrorType">Type of error.</param>
/// <param name="Message">Error message.</param>
/// <param name="LineStart">Start line of the error.</param>
/// <param name="LineEnd">End line of the error.</param>
/// <param name="ColumnStart">Start column of the error.</param>
/// <param name="ColumnEnd">End column of the error.</param>
/// <param name="Pointers">Detailed pointers to the error location.</param>
public record ValidationError(string Error, string ErrorType, string Message, int? LineStart = null, int? LineEnd = null, int? ColumnStart = null, int? ColumnEnd = null, ValidationPointer[]? Pointers = null);

/// <summary>
/// Pointer to a specific location in the Flow JSON.
/// </summary>
/// <param name="LineStart">Start line.</param>
/// <param name="LineEnd">End line.</param>
/// <param name="ColumnStart">Start column.</param>
/// <param name="ColumnEnd">End column.</param>
/// <param name="Path">Path to the property.</param>
public record ValidationPointer(int LineStart, int LineEnd, int ColumnStart, int ColumnEnd, string Path);

/// <summary>
/// Request to update Flow metadata.
/// </summary>
/// <param name="Id">Flow identifier.</param>
/// <param name="Name">Flow name.</param>
/// <param name="Categories">A list of Flow categories.</param>
/// <param name="EndpointUri">The URL of the WA Flow Endpoint.</param>
/// <param name="ApplicationId">The ID of the Meta application.</param>
public record UpdateFlowMetadataRequest([property: JsonIgnore] string Id, string? Name = null, string[]? Categories = null, string? EndpointUri = null, string? ApplicationId = null) : FlowRequest;

/// <summary>
/// Response from a Flow action that only reports success/failure.
/// </summary>
/// <param name="Success">Indicates if the operation was successful.</param>
record SuccessResponse(bool Success) : FlowResponse;

/// <summary>
/// Response from updating Flow JSON.
/// </summary>
/// <param name="Success">Indicates if the operation was successful.</param>
/// <param name="ValidationErrors">List of validation errors, if any.</param>
public record UpdateFlowJsonResponse(bool Success, ValidationError[]? ValidationErrors = null) : FlowResponse;

/// <summary>
/// Preview information for a Flow.
/// </summary>
/// <param name="PreviewUrl">Link for the preview page.</param>
/// <param name="ExpiresAt">Time when the link expires.</param>
public record FlowPreview(string PreviewUrl, DateTimeOffset ExpiresAt);

/// <summary>
/// Response from getting Flow preview.
/// </summary>
/// <param name="Preview">Preview details.</param>
record GetFlowPreviewResponse(FlowPreview Preview) : FlowResponse;

/// <summary>
/// A Flow.
/// </summary>
/// <param name="Id">The unique ID of the Flow.</param>
/// <param name="Name">The user-defined name of the Flow.</param>
/// <param name="Status">The status of the Flow.</param>
/// <param name="Categories">A list of flow categories.</param>
/// <param name="ValidationErrors">A list of errors in the Flow.</param>
public record Flow(string Id, string Name, FlowStatus Status, string[] Categories, ValidationError[] ValidationErrors);

/// <summary>
/// Cursors for paging.
/// </summary>
/// <param name="Before">Cursor for before.</param>
/// <param name="After">Cursor for after.</param>
record Cursors(string Before, string After);

/// <summary>
/// Paging information.
/// </summary>
/// <param name="Cursors">Cursors for pagination.</param>
record Paging(Cursors Cursors);

/// <summary>
/// Response from getting list of Flows.
/// </summary>
/// <param name="Data">List of Flows.</param>
/// <param name="Paging">Paging information.</param>
record GetFlowsResponse(Flow[] Data, Paging? Paging = null) : FlowResponse;

/// <summary>
/// Detailed Flow information.
/// </summary>
/// <param name="Id">The unique ID of the Flow.</param>
/// <param name="Name">The user-defined name of the Flow.</param>
/// <param name="Status">The status of the Flow.</param>
/// <param name="Categories">A list of flow categories.</param>
/// <param name="ValidationErrors">A list of errors in the Flow.</param>
/// <param name="JsonVersion">The version specified in the Flow JSON.</param>
/// <param name="DataApiVersion">The version of the Data API.</param>
/// <param name="EndpointUri">The URL of the WA Flow Endpoint.</param>
public record FlowDetails(string Id, string Name, FlowStatus Status, string[] Categories, ValidationError[] ValidationErrors, string? JsonVersion = null, string? DataApiVersion = null, string? EndpointUri = null);

/// <summary>
/// Asset attached to a Flow.
/// </summary>
/// <param name="Name">Asset name.</param>
/// <param name="AssetType">Asset type.</param>
/// <param name="DownloadUrl">URL to download the asset.</param>
public record FlowAsset(string Name, string AssetType, string DownloadUrl);

/// <summary>
/// Response from getting Flow assets.
/// </summary>
/// <param name="Data">List of assets.</param>
/// <param name="Paging">Paging information.</param>
record GetFlowAssetsResponse(FlowAsset[] Data, Paging? Paging = null) : FlowResponse;

/// <summary>
/// Migrated Flow information.
/// </summary>
/// <param name="SourceName">Source Flow name.</param>
/// <param name="SourceId">Source Flow ID.</param>
/// <param name="MigratedId">Migrated Flow ID.</param>
public record MigratedFlow(string SourceName, string SourceId, string MigratedId);

/// <summary>
/// Failed migration information.
/// </summary>
/// <param name="SourceName">Source Flow name.</param>
/// <param name="ErrorCode">Error code.</param>
/// <param name="ErrorMessage">Error message.</param>
public record FailedFlow(string SourceName, int ErrorCode, string ErrorMessage);