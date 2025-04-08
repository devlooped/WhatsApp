using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Devlooped.WhatsApp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SendStringAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule { get; } = new(
        id: "WA001",
        title: "Invalid Payload Type",
        messageFormat: $"The second parameter of '{nameof(IWhatsAppClient)}.{nameof(IWhatsAppClient.SendAsync)}' should not be a string.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        description: "The payload parameter is serialized and sent as JSON over HTTP. Use an object instead.",
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.InvocationExpression);
    }

    static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name.Identifier.Text == nameof(IWhatsAppClient.SendAsync))
        {
            var methodSymbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;
            if (methodSymbol?.ContainingSymbol.Name == nameof(IWhatsAppClient) && invocation.ArgumentList.Arguments.Count == 2)
            {
                var secondArgument = invocation.ArgumentList.Arguments[1];
                var argumentType = context.SemanticModel.GetTypeInfo(secondArgument.Expression).Type;
                if (argumentType?.SpecialType == SpecialType.System_String)
                {
                    var diagnostic = Diagnostic.Create(Rule, secondArgument.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
