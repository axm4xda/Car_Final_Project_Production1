using Car_Project.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class FAQsController : Controller
    {
        public IActionResult Index()
        {
            var groups = new List<FAQGroupViewModel>
            {
                new FAQGroupViewModel
                {
                    GroupTitle = "How To Buy?",
                    Items = new List<FAQItemViewModel>
                    {
                        new FAQItemViewModel
                        {
                            Question = "Steps to purchase a car from our dealership?",
                            Answer = "To purchase a car from our dealership, start by exploring our inventory online or visiting us in person to find the vehicle that suits your needs. Schedule a test drive to ensure it's the right fit, then review financing or leasing options with our team. Provide the necessary documents, such as your ID, proof of insurance, and income verification. Once terms are agreed upon, finalize the paperwork, inspect the car, and drive away with your new vehicle!",
                            IsOpen = true
                        },
                        new FAQItemViewModel
                        {
                            Question = "Required documents for financing or leasing?",
                            Answer = "To purchase a car from our dealership, start by exploring our inventory online or visiting us in person. You will need proof of identity, income verification, and proof of insurance to proceed with financing or leasing."
                        },
                        new FAQItemViewModel
                        {
                            Question = "Options for reserving or pre-ordering a vehicle?",
                            Answer = "An auto loan is a sum of money that you borrow in order to buy a car. The person or organization lending you the money is known as the lender, and the borrower agrees to pay back the full amount by a certain date. They also pay interest, which is a percentage of the loan amount, usually via monthly payments."
                        },
                        new FAQItemViewModel
                        {
                            Question = "Available payment methods and financing plans?",
                            Answer = "We offer a variety of payment methods including cash, bank transfer, and auto loan financing. Our financing plans are flexible with terms ranging from 12 to 84 months depending on your eligibility."
                        },
                        new FAQItemViewModel
                        {
                            Question = "How to schedule a test drive before buying?",
                            Answer = "You can schedule a test drive by contacting us via phone, email, or by filling out the test drive form on our website. Our team will confirm a convenient date and time for you."
                        }
                    }
                },
                new FAQGroupViewModel
                {
                    GroupTitle = "Exchanges & Returns",
                    Items = new List<FAQItemViewModel>
                    {
                        new FAQItemViewModel
                        {
                            Question = "Policies on vehicle exchanges after purchase?",
                            Answer = "We allow vehicle exchanges within 7 days of purchase provided the car has not exceeded 500 additional miles and is in its original condition. Please bring all original documentation when visiting our dealership to initiate an exchange."
                        },
                        new FAQItemViewModel
                        {
                            Question = "Conditions for returning a rental car early?",
                            Answer = "Rental cars returned early may be subject to an early return fee. Please review your rental agreement or contact our rental department for specific terms applicable to your booking."
                        },
                        new FAQItemViewModel
                        {
                            Question = "Timeframes for initiating an exchange or return?",
                            Answer = "Exchanges and returns must be initiated within 7 calendar days from the date of purchase or rental. After this period, standard trade-in policies apply."
                        },
                        new FAQItemViewModel
                        {
                            Question = "Documentation needed for processing exchanges?",
                            Answer = "You will need to provide your original purchase agreement, vehicle title, government-issued ID, and the vehicle in its original condition with all accessories included."
                        }
                    }
                },
                new FAQGroupViewModel
                {
                    GroupTitle = "Refund Questions",
                    Items = new List<FAQItemViewModel>
                    {
                        new FAQItemViewModel
                        {
                            Question = "Eligibility for refunds on purchases or deposits?",
                            Answer = "Refunds on deposits are available if a cancellation is made within 48 hours of the initial deposit. For full vehicle purchases, refunds are evaluated on a case-by-case basis in accordance with our return policy."
                        },
                        new FAQItemViewModel
                        {
                            Question = "How refunds are processed for canceled rentals?",
                            Answer = "Canceled rental reservations made more than 24 hours in advance are eligible for a full refund. Cancellations within 24 hours may incur a cancellation fee as outlined in your rental agreement."
                        },
                        new FAQItemViewModel
                        {
                            Question = "Timeframes for receiving a refund?",
                            Answer = "Approved refunds are typically processed within 5–10 business days depending on your payment method. Credit card refunds may take additional time to appear on your statement."
                        }
                    }
                }
            };

            return View(groups);
        }
    }
}
