using System;
using System.Collections.Generic;
using System.Text;

namespace packing_probrem.domain
{
    class Section : Rect
    {
        public List<Rect> Spaces { get; }
        private readonly Box SectionSize;

        public Section(int left, int right, int top, int bottom, Box sectionSize) : base(left, right, top, bottom)
        {
            Spaces = new List<Rect>();
            SectionSize = sectionSize;
            Spaces.Add(new Rect(0, SectionSize.Width, SectionSize.Height, 0));
        }

        /// <summary>母材内に入るスペースがあるかを返す</summary>
        /// <param name="rect">調べる長方形</param>
        /// <param name="useSectionHeight">母材の高さを使用するか</param>
        /// <returns></returns>
        public bool IsAppendable(Rect rect, bool useSectionHeight)
        {
            if (Left > rect.Left)
                return false;
            if (Right < rect.Right)
                return false;
            if (Bottom > rect.Bottom)
                return false;
            if (Top < rect.Top && useSectionHeight)
                return false;
            return true;
        }
    }
}
