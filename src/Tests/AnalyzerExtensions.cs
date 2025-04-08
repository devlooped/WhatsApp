using Devlooped.WhatsApp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Testing;

namespace Devlooped.WhatsApp;

public static class AnalyzerExtensions
{
    public static TTest WithWhatsApp<TTest>(this TTest test) where TTest : AnalyzerTest<DefaultVerifier>
    {
        test.SolutionTransforms.Add((solution, projectId)
            => solution
                .GetProject(projectId)?
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(IWhatsAppClient).Assembly.Location))
                .Solution ?? solution);

        return test;
    }
}
