extern alias CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Analyzer = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<CodeAnalysis.Devlooped.WhatsApp.SendStringAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;
using SendStringAnalyzer = CodeAnalysis.Devlooped.WhatsApp.SendStringAnalyzer;

namespace Devlooped.WhatsApp;

public class AnalyzerTests
{
    [Fact]
    public async Task InvalidSendTextAsync()
    {
        var test = new CSharpAnalyzerTest<SendStringAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode =
                $$"""
                using System.Threading.Tasks;
                using {{nameof(Devlooped)}}.{{nameof(Devlooped.WhatsApp)}};
                            
                public class Handler
                {
                    public async Task Send({{nameof(IWhatsAppClient)}} client, string text)
                    {
                        await client.{{nameof(IWhatsAppClient.SendAsync)}}("1234", {|#0:text|});
                    }
                }
                """
        }.WithWhatsApp();

        var expected = Analyzer.Diagnostic(SendStringAnalyzer.Rule).WithLocation(0);

        test.ExpectedDiagnostics.Add(expected);

        await test.RunAsync();
    }
}
