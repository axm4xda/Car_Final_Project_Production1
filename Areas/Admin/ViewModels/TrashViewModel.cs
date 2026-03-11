using Car_Project.Models;

namespace Car_Project.Areas.Admin.ViewModels
{
    public class TrashViewModel
    {
        public IList<SellCarRequest> TrashedRequests { get; set; } = new List<SellCarRequest>();
        public IList<Car> TrashedCars { get; set; } = new List<Car>();
    }
}
