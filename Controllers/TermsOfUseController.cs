using Car_Project.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class TermsOfUseController : Controller
    {
        public IActionResult Index()
        {
            var model = new TermsOfUseViewModel
            {
                Sections = new List<TermsSectionViewModel>
                {
                    new TermsSectionViewModel
                    {
                        AnchorId = "section1",
                        Title = "1. Terms",
                        Paragraphs = new List<string>
                        {
                            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer sed euismod justo, sit amet efficitur dui. Aliquam sodales vestibulum velit, eget sollicitudin quam. Donec non aliquam eros. Etiam sit amet lectus vel justo dignissim condimentum.",
                            "In malesuada neque quis libero laoreet posuere. In consequat vitae ligula quis rutrum. Morbi dolor orci, maximus a pulvinar sed, bibendum ac lacus. Suspendisse in consectetur lorem. Aliquam elementum, est sed interdum cursus, felis ex pharetra nisi, ut elementum tortor urna eu nulla.",
                            "Etiam eleifend metus at nunc ultricies facilisis. Morbi finibus tristique interdum. Nullam vel eleifend est, eu posuere risus. Vestibulum ligula ex, ullamcorper sit amet molestie."
                        }
                    },
                    new TermsSectionViewModel
                    {
                        AnchorId = "section2",
                        Title = "2. Limitations",
                        Paragraphs = new List<string>
                        {
                            "Etiam eleifend metus at nunc ultricies facilisis. Morbi finibus tristique interdum. Nullam vel eleifend est, eu posuere risus. Vestibulum ligula ex, ullamcorper sit amet molestie a, finibus nec ex.",
                            "Etiam eleifend metus at nunc ultricies facilisis. Morbi finibus tristique interdum. Nullam vel eleifend est, eu posuere risus. Vestibulum ligula ex, ullamcorper sit amet molestie."
                        },
                        BulletPoints = new List<string>
                        {
                            "Aliquam elementum, est sed interdum cursus, felis ex pharetra nisi, ut elementum tortor urna eu nulla. Donec rhoncus in purus quis blandit.",
                            "Etiam eleifend metus at nunc ultricies facilisis.",
                            "Nullam vel eleifend est, eu posuere risus. Vestibulum ligula ex, ullamcorper sit amet molestie a, finibus nec ex."
                        }
                    },
                    new TermsSectionViewModel
                    {
                        AnchorId = "section3",
                        Title = "3. Revisions And Errata",
                        Paragraphs = new List<string>
                        {
                            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer sed euismod justo, sit amet efficitur dui. Aliquam sodales vestibulum velit, eget sollicitudin quam. Donec non aliquam eros.",
                            "In malesuada neque quis libero laoreet posuere. In consequat vitae ligula quis rutrum. Morbi dolor orci, maximus a pulvinar sed, bibendum ac lacus. Aliquam elementum, est sed interdum cursus, felis ex pharetra nisi.",
                            "Etiam eleifend metus at nunc ultricies facilisis. Morbi finibus tristique interdum. Nullam vel eleifend est, eu posuere risus. Vestibulum ligula ex, ullamcorper sit amet molestie a, finibus nec ex."
                        }
                    },
                    new TermsSectionViewModel
                    {
                        AnchorId = "section4",
                        Title = "4. Site Terms Of Use Modifications",
                        Paragraphs = new List<string>
                        {
                            "Etiam eleifend metus at nunc ultricies facilisis. Morbi finibus tristique interdum. Nullam vel eleifend est, eu posuere risus. Vestibulum ligula ex, ullamcorper sit amet molestie.",
                            "Etiam eleifend metus at nunc ultricies facilisis. Morbi finibus tristique interdum. Nullam vel eleifend est, eu posuere risus. Vestibulum ligula ex, ullamcorper sit amet molestie."
                        },
                        BulletPoints = new List<string>
                        {
                            "Aliquam elementum, est sed interdum cursus, felis ex pharetra nisi, ut elementum tortor urna eu nulla. Donec rhoncus in purus quis blandit.",
                            "Etiam eleifend metus at nunc ultricies facilisis.",
                            "Nullam vel eleifend est, eu posuere risus. Vestibulum ligula ex, ullamcorper sit amet molestie a, finibus nec ex."
                        }
                    },
                    new TermsSectionViewModel
                    {
                        AnchorId = "section5",
                        Title = "5. Risks",
                        Paragraphs = new List<string>
                        {
                            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer sed euismod justo, sit amet efficitur dui. Aliquam sodales vestibulum velit, eget sollicitudin quam.",
                            "In malesuada neque quis libero laoreet posuere. In consequat vitae ligula quis rutrum. Morbi dolor orci, maximus a pulvinar sed, bibendum ac lacus. Aliquam elementum, est sed interdum cursus.",
                            "Etiam eleifend metus at nunc ultricies facilisis. Morbi finibus tristique interdum. Nullam vel eleifend est, eu posuere risus. Vestibulum ligula ex, ullamcorper sit amet molestie a, finibus nec ex."
                        }
                    }
                }
            };

            return View(model);
        }
    }
}
