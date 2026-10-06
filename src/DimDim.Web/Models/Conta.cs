using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DimDim.Web.Models;

public class Conta
{
    public static readonly string[] TiposValidos = { "Corrente", "Poupanca", "Investimento" };

    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o número da conta.")]
    [StringLength(20)]
    [Display(Name = "Número")]
    public string Numero { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a agência.")]
    [StringLength(10)]
    [Display(Name = "Agência")]
    public string Agencia { get; set; } = "0001";

    [Required(ErrorMessage = "Escolha o tipo.")]
    [RegularExpression("^(Corrente|Poupanca|Investimento)$", ErrorMessage = "Tipo inválido.")]
    public string Tipo { get; set; } = "Corrente";

    [Range(0, 999_999_999, ErrorMessage = "O saldo não pode ser negativo.")]
    public decimal Saldo { get; set; }

    [Display(Name = "Aberta em")]
    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;

    [Range(1, int.MaxValue, ErrorMessage = "Escolha o cliente titular.")]
    [Display(Name = "Titular")]
    public int ClienteId { get; set; }

    [ValidateNever]
    public Cliente? Cliente { get; set; }
}
