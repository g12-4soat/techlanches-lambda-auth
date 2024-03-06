namespace TechLanchesLambda.Utils;

public class Resultado
{
    protected Resultado(bool sucesso, List<string> erros)
    {
        if (sucesso && erros.Any())
            throw new InvalidOperationException();
        if (!sucesso && !erros.Any())
            throw new InvalidOperationException();
        Sucesso = sucesso;
        Erros = erros;
    }

    public bool Sucesso { get; }
    public List<string> Erros { get; }
    public bool Falhou => !Sucesso;

    public static Resultado Falha(string message)
    {
        return new Resultado(false, new List<string> { message });
    }

    public static Resultado<T> Falha<T>(string message)
    {
        return new Resultado<T>(default, false, new List<string> { message });
    }

    public static Resultado<T> Falha<T>(T value, string message)
    {
        return new Resultado<T>(value, false, new List<string> { message });
    }

    public static Resultado FalhaRange(List<string> messages)
    {
        return new Resultado(false, messages);
    }

    public static Resultado<T> FalhaRange<T>(List<string> messages)
    {
        return new Resultado<T>(default, false, messages);
    }

    public static Resultado<T> FalhaRange<T>(T value, List<string> mensagem)
    {
        return new Resultado<T>(value, false, mensagem);
    }
    public static Resultado Ok()
    {
        return new Resultado(true, new List<string>());
    }

    public static Resultado<T> Ok<T>(T value)
    {
        return new Resultado<T>(value, true, new List<string>());
    }
}

public class Resultado<T> : Resultado
{
    protected internal Resultado(T value, bool sucesso, List<string> erros)
        : base(sucesso, erros)
    {
        Value = value;
    }

    public T Value { get; set; }
}