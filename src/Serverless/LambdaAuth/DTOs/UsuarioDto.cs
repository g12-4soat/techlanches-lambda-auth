namespace TechLanchesLambda.DTOs;

public record class UsuarioDto(string Cpf, string Email = "", string Nome = "");