namespace DimDim.Web.Models;

public class DashboardViewModel
{
    public int TotalClientes { get; set; }
    public int TotalContas { get; set; }
    public decimal SaldoTotal { get; set; }
    public List<Conta> UltimasContas { get; set; } = new();
}
