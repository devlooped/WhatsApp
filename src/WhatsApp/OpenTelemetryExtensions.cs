using System.Diagnostics;

namespace Devlooped.WhatsApp;

static class OpenTelemetryExtensions
{
    public static TagList RecordException(this Activity? activity, Exception e, bool enableSensitiveData, TagList tags = new())
    {
        // NOTE: we always append the tags since they are used for metrics too,
        // just just for span tracking.

        // https://opentelemetry.io/docs/specs/otel/trace/exceptions/
        tags.Add("exception.message", e.Message);
        tags.Add("exception.type", e.GetType().FullName);

        if (enableSensitiveData)
            tags.Add("exception.stacktrace", e.StackTrace);

        if (activity is null)
            return tags;

        activity.AddTag("error.type", e.GetType().FullName);
        activity.SetStatus(ActivityStatusCode.Error, e.Message);

        activity.AddEvent(new ActivityEvent("exception", tags: [.. tags]));

        return tags;
    }
}
