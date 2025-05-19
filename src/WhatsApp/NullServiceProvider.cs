namespace Devlooped.WhatsApp;

class NullServiceProvider : IServiceProvider
{
    public static IServiceProvider Default { get; } = new NullServiceProvider();
    NullServiceProvider() { }
    public object? GetService(Type serviceType) => null;
}