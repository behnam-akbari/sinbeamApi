using Scaphoid.Core.Model;

namespace Scaphoid.Infrastructure.Repositories
{
    public class WebThicknessRepository
    {
        public List<WebThickness> Get()
        {
            var list = new List<WebThickness>()
            {
                new WebThickness("WT0", 1.5),
                new WebThickness("WTA", 2),
                new WebThickness("WTB", 2.5),
                new WebThickness("WTC", 3),
                new WebThickness("WTD", 4),
                new WebThickness("WTE", 5),
                new WebThickness("WTF", 6)
            };

            return list;
        } 
    }
    public class FlangeRepository
    {
        public List<Flange> Get()
        {
            var list = new List<Flange>()
            {
                new Flange(160, 6, 2.5),
                new Flange(180, 6, 2.8),
                new Flange(200, 6, 3.1),

                new Flange(160, 8, 5.6),
                new Flange(180, 8, 6.3),
                new Flange(200, 8, 7),
                new Flange(220, 8, 7.7),

                new Flange(200, 10,13.5),
                new Flange(220, 10, 14.8),
                new Flange(250, 10, 16.8),

                new Flange(200, 12, 23.2),
                new Flange(220, 12, 25.5),
                new Flange(250, 12, 29),
                new Flange(300, 12, 34.7),

                new Flange(220, 15, 49.7),
                new Flange(250, 15, 56.4),
                new Flange(300, 15, 67.7),
                new Flange(350, 15, 78.9),

                new Flange(250, 20, 133.5),
                new Flange(300, 20, 160.2),
                new Flange(350, 20, 186.8),
                new Flange(400, 20, 213.5),

                new Flange(300, 25, 312.7),
                new Flange(350, 25, 364.8),
                new Flange(400, 25, 416.8),
                new Flange(430, 25, 448.1),
                new Flange(450, 25, 468.9),

                new Flange(350, 30, 630.2),
                new Flange(400, 30, 720.2),
                new Flange(430, 30, 774.2),
                new Flange(450, 30, 810.2),
            };

            return list;
        }
    }

    public class WebSectionRepository
    {
        private readonly FlangeRepository _flangeRepository;
        private readonly WebThicknessRepository _webThicknessRepository;

        public WebSectionRepository(FlangeRepository flangeRepository, WebThicknessRepository webThicknessRepository)
        {
            _flangeRepository = flangeRepository;
            _webThicknessRepository = webThicknessRepository;
        }

        public IEnumerable<WebSection> Get(SteelType steelType)
        {
            var flanges = _flangeRepository.Get();

            var webThicknesses = _webThicknessRepository.Get();

            var webHeightCollection = Constants.NewWebHeightCollection;

            foreach (var webHeight in webHeightCollection)
            {
                foreach (var flange in flanges)
                {
                    foreach (var webThickness in webThicknesses)
                    {
                        yield return new WebSection
                        {
                            FlangeThickness = flange.Thickness,
                            FlangeWidth = flange.Width,
                            WebHeight = webHeight,
                            WebThickness = webThickness.Value,
                            WebThicknessKey = webThickness.Key,
                            It = flange.It,
                            SteelType = steelType
                        };
                    }
                }
            }
        }

        public WebSection Get(SteelType steelType, string id)
        {
            var sections = Get(steelType);

            var section = sections.FirstOrDefault(e=>e.Id == id);

            return section;
        }
    }
}
