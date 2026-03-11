using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    /// <summary>
    /// Kredit hesablama (LoanCalculation) ?m?liyyatlar? üçün servis interfeysi.
    /// </summary>
    public interface ILoanCalculationService
    {
        // ??? PUBLIC (site) metodlar? ???????????????????????????????????????????

        /// <summary>
        /// Verilmi? parametrl?r? gör? ayl?q öd?ni?, ümumi faiz v? ümumi m?bl??i hesablay?r.
        /// N?tic?ni bazaya yazm?r — yaln?z hesablama apar?r.
        /// </summary>
        /// <param name="carPrice">Avtomobilin qiym?ti</param>
        /// <param name="downPayment">?lkin öd?ni? m?bl??i</param>
        /// <param name="annualInterestRate">?llik faiz d?r?c?si (m?s: 12.5 ? 12.5%)</param>
        /// <param name="loanTermMonths">Kredit müdd?ti (ay say?)</param>
        LoanCalculation Calculate(
            decimal carPrice,
            decimal downPayment,
            decimal annualInterestRate,
            int loanTermMonths);

        /// <summary>
        /// Hesablanm?? kredit n?tic?sini bazaya yaz?r (ist?y? ba?l? — tarixç? üçün).
        /// </summary>
        Task<LoanCalculation> SaveAsync(LoanCalculation calculation);

        // ??? ADMIN metodlar? ???????????????????????????????????????????????????

        /// <summary>
        /// Admin paneli üçün bütün saxlan?lan kredit hesablamalar?n? qaytar?r.
        /// </summary>
        Task<IList<LoanCalculation>> GetAllAdminAsync();

        /// <summary>
        /// Verilmi? id-y? gör? t?k hesablaman? qaytar?r. Tap?lmasa null qaytar?r.
        /// </summary>
        Task<LoanCalculation?> GetByIdAsync(int id);

        /// <summary>
        /// Hesablama qeydini silir.
        /// </summary>
        Task DeleteAsync(int id);
    }
}
