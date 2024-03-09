using FluentValidation;
using TechLanchesLambda.DTOs;
using TechLanchesLambda.Utils;

namespace TechLanchesLambda.Validations;

public class UsuarioAuthValidation : AbstractValidator<UsuarioDto>
{
    public UsuarioAuthValidation()
    {
        
    }
}

public class UsuarioCadastroValidation : AbstractValidator<UsuarioDto>
{
    public UsuarioCadastroValidation()
    {
        RuleFor(x => x.Cpf)
           .NotEmpty().WithMessage("CPF deve ser informado.")
           .NotNull().WithMessage("CPF deve ser informado.");

        When(x => !string.IsNullOrEmpty(x.Cpf) && !string.IsNullOrWhiteSpace(x.Cpf), () =>
        {
            RuleFor(x => x.Cpf)
            .Must(ValidatorCPF.Validar).WithMessage("O CPF informado está inválido.");
        });

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email deve ser informado.")
            .NotNull().WithMessage("Email deve ser informado.");

        When(x => !string.IsNullOrEmpty(x.Email) && !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
            .Must(ValidatorEmail.Validar).WithMessage("O Email informado está inválido.");
        });

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome deve ser informado.")
            .NotNull().WithMessage("Nome deve ser informado.");
    }
}