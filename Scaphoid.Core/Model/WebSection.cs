using System.Text.Json.Serialization;

namespace Scaphoid.Core.Model
{
    public class Resource
    {
        [JsonPropertyName("_links")]
        public List<Link> Links { get; set; } = new List<Link>();
    }

    public class Link
    {
        public Link(string rel, string href, string method)
        {
            Rel = rel;
            Href = href;
            Method = method;
        }
        public string Href { get; set; }
        public string Rel { get; set; }
        public string Method { get; set; }
    }

    public class WebSection : Resource
    {
        public string Id => $"{WebThicknessKey.Replace(" ", "-" )}-{WebHeight}-{FlangeWidth}-{FlangeThickness}".ToLower();

        public string Key => $"{WebThicknessKey} {WebHeight} / {FlangeWidth} X {FlangeThickness}";

        /// <summary>
        /// Weight/m
        /// </summary>
        public double Weight => Math.Round((2 * (FlangeWidth / 1000) * (FlangeThickness / 1000) + (WebHeight / 1000) * (WebThickness / 1000) * 1.1951951951952) * 7850, 3);

        /// <summary>
        /// WT
        /// </summary>
        public double WebHeight { get; set; }

        /// <summary>
        /// WT0/WTA/WTB/...
        /// </summary>
        public double WebThickness { get; set; }

        [JsonIgnore]
        public string WebThicknessKey { get; set; }

        /// <summary>
        /// bg
        /// </summary>
        public double FlangeWidth { get; set; }

        /// <summary>
        /// tg
        /// </summary>
        public double FlangeThickness { get; set; }

        /// <summary>
        /// Cross Section Values - 2Ag 
        /// </summary>
        public double SectionPerimeter => Math.Round(2 * FlangeWidth * FlangeThickness / 100, 3);

        public double SectionArea => (2 * FlangeWidth * FlangeThickness) + (WebHeight * WebThickness);

        public double SelfWeight => Math.Round((SectionArea * 7850 * 9.81d / 1000000) / 1000, 2);

        /// <summary>
        /// U
        /// </summary>
        [JsonIgnore]
        public double SurfaceAreaPerM => 4 * (FlangeWidth + FlangeThickness) / 1000 + 1.1951951951952 * 2 * 1 * WebHeight / 1000;



        /// <summary>
        /// U
        /// </summary>
        [JsonIgnore]
        public double SurfaceAreaPerT => SurfaceAreaPerM * 1000 / Weight;

        /// <summary>
        /// Cross Section Values - Iy 
        /// </summary>
        public double MomentOfInertiaIy => Math.Round(Math.Pow(FlangeWidth * FlangeThickness / 100, 2) / (2 * FlangeWidth * FlangeThickness / 100) * Math.Pow(DimensionsZ / 10, 2), 3);

        /// <summary>
        /// Cross Section Values - Iz
        /// </summary>
        public double MomentOfInertiaIz => Math.Round((FlangeThickness / 10) * Math.Pow(FlangeWidth / 10, 3) / 6, 3);

        /// <summary>
        /// Mgg
        /// </summary>
        public double BendingCapacity => Math.Round(Ngg * DimensionsZ / 2000, 3);
        public double ShearCapacity { get; set; }
        public double AxialCapacity { get; set; }


        [JsonIgnore]
        public double DimensionsZ => WebHeight + FlangeThickness;

        [JsonIgnore]
        public SteelType SteelType { get; set; }

        [JsonIgnore]
        public double Fyk => SteelType == SteelType.S235 ? 23.50 : 35.50;

        [JsonIgnore]
        public double Ngg => Fyk * SectionPerimeter;

        /// <summary>
        /// Cross Section Values - Iw
        /// </summary>
        [JsonIgnore]
        public double Iw => (SectionPerimeter / 2) * Math.Pow(FlangeWidth / 10, 2) * Math.Pow((DimensionsZ / 10), 2) / 24;

        [JsonIgnore]
        public double It { get; set; }
    }
}
