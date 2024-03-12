using TechLanchesLambda.DTOs;

namespace TechLanchesLambda.Utils;

public class Resultado
{
    protected Resultado(bool sucesso, List<NotificacaoDto> notificacoes)
    {
        if (sucesso && notificacoes.Any())
            throw new InvalidOperationException();
        if (!sucesso && !notificacoes.Any())
            throw new InvalidOperationException();
        Sucesso = sucesso;
        Notificacoes = notificacoes;
    }

    public bool Sucesso { get; }
    public List<NotificacaoDto> Notificacoes { get; }
    public bool Falhou => !Sucesso;

    public static Resultado Falha(string message)
    {
        return new Resultado(false, new List<NotificacaoDto> { new NotificacaoDto(message) });
    }

    public static Resultado<T> Falha<T>(string message)
    {
        return new Resultado<T>(default, false, new List<NotificacaoDto> { new NotificacaoDto(message) });
    }

    public static Resultado<T> Falha<T>(T value, string message)
    {
        return new Resultado<T>(value, false, new List<NotificacaoDto> { new NotificacaoDto(message) });
    }

    public static Resultado FalhaRange(List<NotificacaoDto> messages)
    {
        return new Resultado(false, messages);
    }

    public static Resultado<T> FalhaRange<T>(List<NotificacaoDto> messages)
    {
        return new Resultado<T>(default, false, messages);
    }

    public static Resultado<T> FalhaRange<T>(T value, List<NotificacaoDto> mensagem)
    {
        return new Resultado<T>(value, false, mensagem);
    }
    public static Resultado Ok()
    {
        return new Resultado(true, new List<NotificacaoDto>());
    }

    public static Resultado<T> Ok<T>(T value)
    {
        return new Resultado<T>(value, true, new List<NotificacaoDto>());
    }
}

public class Resultado<T> : Resultado
{
    protected internal Resultado(T value, bool sucesso, List<NotificacaoDto> notificacoes)
        : base(sucesso, notificacoes)
    {
        Value = value;
    }

    public T Value { get; set; }
}