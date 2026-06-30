namespace DomainLayer.DTOs;

public class DoctorFinancialReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal TotalEarnings { get; set; }
    public int TotalAppointments { get; set; }
    public List<DoctorDailyEarningDto> DailyBreakdown { get; set; } = new();
    
    public List<decimal> PrefixSums { get; set; } = new();
}

public class DoctorDailyEarningDto
{
    public DateTime Date { get; set; }
    public decimal Earnings { get; set; }
    public int AppointmentCount { get; set; }
}
