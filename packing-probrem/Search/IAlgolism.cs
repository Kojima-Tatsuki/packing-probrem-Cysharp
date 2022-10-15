using System;
using System.Collections.Generic;
using System.Text;
using packing_probrem.domain;

namespace packing_probrem.Search
{
    interface IAlgolism
    {
        (int score, IReadOnlyList<Rect> pushed) Cal(IReadOnlyList<Box> rects);
    }
}
