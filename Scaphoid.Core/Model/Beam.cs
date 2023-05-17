namespace Scaphoid.Core.Model
{
    public class Beam
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public double Span { get; set; }
        public bool IsUniformDepth { get; set; }

        public int WebDepth { get; set; }
        public int WebDepthRight { get; set; }
        public bool WebLocalBuckle { get; set; } = true;

        public double WebThickness { get; set; }
        public SteelType WebSteel { get; set; }

        public int TopFlangeThickness { get; set;}
        public int TopFlangeWidth { get; set; }
        public SteelType TopFlangeSteel { get; set; }

        public int BottomFlangeThickness { get; set; }
        public int BottomFlangeWidth { get; set; }
        public SteelType BottomFlangeSteel { get; set; }

        public double SectionInertia => GetInertia(WebDepth);
        public double LeftInertia => GetInertia(WebDepth);
        public double RightInertia => GetInertia(WebDepthRight);

        public double MinorInertia => TopFlangeThickness * Math.Pow(TopFlangeWidth, Factor4) +
                                      BottomFlangeThickness * Math.Pow(BottomFlangeWidth, Factor4) / Factor5;

        public double TorsConst => (Factor3 * Math.Pow(WebThickness, Factor4) * WebDepth +
                           Factor3 * Math.Pow(TopFlangeThickness, Factor4) * TopFlangeWidth +
                           Factor3 * Math.Pow(BottomFlangeThickness, Factor4) * BottomFlangeWidth) / Factor2; 

        public double TorsConstRight => (Factor3 * Math.Pow(WebThickness, Factor4) * WebDepthRight +
                           Factor3 * Math.Pow(TopFlangeThickness, Factor4) * TopFlangeWidth +
                           Factor3 * Math.Pow(BottomFlangeThickness, Factor4) * BottomFlangeWidth) / Factor2;

        public double SurfAreaLeft => ((2 * (TopFlangeWidth + TopFlangeThickness + BottomFlangeWidth + BottomFlangeThickness)) +
                                       (2 * Factor7 * WebDepth)) / 1000;

        public double SurfAreaRight => (2 * (TopFlangeWidth + TopFlangeThickness + BottomFlangeWidth + BottomFlangeThickness + WebDepthRight)) / 1000;


        public double Web => WebThickness;
        public double XSectionArea => TopFlangeThickness * TopFlangeWidth + BottomFlangeThickness * BottomFlangeWidth + Web * WebDepth;

        public double XSectionAreaRight => TopFlangeThickness * TopFlangeWidth + BottomFlangeThickness * BottomFlangeWidth + Web * WebDepthRight;

        public double NoWebXSectionArea => TopFlangeThickness * TopFlangeWidth + BottomFlangeThickness * BottomFlangeWidth;

        public double EffxSectionArea => TopFlangeThickness * TopFlangeWidth + BottomFlangeThickness * BottomFlangeWidth + Web * WebDepth * Factor7;

        public double EffxSectionAreaRight => TopFlangeThickness * TopFlangeWidth + BottomFlangeThickness * BottomFlangeWidth + Web * Factor7 * WebDepthRight;

        public double SelfWeight => EffxSectionArea * 7850 / 1000000;
        public double SelfWeightRight => EffxSectionAreaRight * 7850 / 1000000;

        public double SurfPerLeft => 1000 * SurfAreaLeft / SelfWeight;
        public double SurfPerRight => 1000 * SurfAreaRight / SelfWeightRight;

        public double Hhh => WebDepth + TopFlangeThickness;
        public double HhhRight => WebDepthRight + TopFlangeThickness;

        public double Warping => ((TopFlangeThickness * Math.Pow(Hhh, 2) / 12) *
                                 (Math.Pow(TopFlangeWidth, 3) * Math.Pow(BottomFlangeWidth, 3)) /
                                 (Math.Pow(TopFlangeWidth, 3) + Math.Pow(BottomFlangeWidth, 3))) / Factor8;

        public double WarpingRight => ((TopFlangeThickness * Math.Pow(HhhRight, 2) / 12) *
                         (Math.Pow(TopFlangeWidth, 3) * Math.Pow(BottomFlangeWidth, 3)) /
                         (Math.Pow(TopFlangeWidth, 3) + Math.Pow(BottomFlangeWidth, 3))) / Factor8;

        public bool WarpingStatus => TopFlangeThickness == BottomFlangeThickness;


        public double momarray114 = 1;//ToDo
        public double momarray9914 = 1;//ToDo

        public double IY => Math.Pow(momarray114 / NoWebXSectionArea, 0.5) / 10;
        public double IYRight => Math.Pow(momarray9914 / NoWebXSectionArea, 0.5) / 10;

        public double IZ => Math.Pow(MinorInertia / NoWebXSectionArea, 0.5) / 10;
        public double IZRight => Math.Pow(MinorInertia / NoWebXSectionArea, 0.5) / 10;




        const double Factor1 = 0.5;
        const double Factor2 = 10000;
        const double Factor3 = 1 / 3;
        const double Factor4 = 3;
        const double Factor5 = 12;
        const double Factor6 = 2;
        const double Factor7 = (178 / 155);
        const double Factor8 = 1000000;

        public string GetBeamName()
        {
            var webcode = string.Empty;

            string beamName;

            if (IsUniformDepth == true)
            {
                beamName = "WT" + webcode + WebDepth + " / " + TopFlangeWidth + "x" + TopFlangeThickness;
            }
            else
            {
                beamName = "WT" + webcode + WebDepthRight + "-" + WebDepth + " / " + TopFlangeWidth + "x" + TopFlangeThickness;
            }

            if (TopFlangeWidth == BottomFlangeWidth && TopFlangeThickness == BottomFlangeThickness)
            {
                return beamName;
            }

            beamName = beamName + "+" + BottomFlangeWidth + "x" + BottomFlangeThickness;

            return beamName;
        }

        public double GetInertia(int webDepth)
        {
            var sectionArea = TopFlangeThickness * TopFlangeWidth + BottomFlangeThickness * BottomFlangeWidth;

            var nAxis = (BottomFlangeWidth * BottomFlangeThickness * BottomFlangeThickness * Factor1 +
                         TopFlangeThickness * TopFlangeWidth * (BottomFlangeThickness + webDepth + Factor1 * TopFlangeThickness)) / sectionArea;

            var topFlangeInertia = TopFlangeWidth * Math.Pow(TopFlangeThickness, Factor4) / Factor5 +
                                   TopFlangeWidth * TopFlangeThickness * Math.Pow(BottomFlangeThickness + webDepth + Factor1 * TopFlangeThickness - nAxis, Factor6);

            var bottomFlangeInertia = BottomFlangeWidth * Math.Pow(BottomFlangeThickness, Factor4) / Factor5 +
                                      BottomFlangeThickness * Math.Pow(Factor1 * BottomFlangeThickness - nAxis, Factor6);

            var inertia = (topFlangeInertia + bottomFlangeInertia) / Factor2;

            return inertia;
        }
    }

    public enum SteelType
    {
        S275 = 1, 
        S355 = 2
    }
}
