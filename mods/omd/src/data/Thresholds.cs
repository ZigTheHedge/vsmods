using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace omd.src.data
{
    class Thresholds
    {
        public List<ThresholdItem> list = new List<ThresholdItem>();

        public void SortByAmounts()
        {
            list = list.OrderBy(o => o.amount).ToList();
        }

        public ThresholdItem GetSuitableThreshold(int amount)
        {
            int preferredIndex = -1;
            for (int idx = 0; idx < list.Count; idx++)
            {
                if (list.ElementAt(idx).amount == amount)
                {
                    preferredIndex = idx;
                    if (list.ElementAt(idx).exact)
                    {
                        break;
                    }
                }
                if (list.ElementAt(idx).amount < amount)
                {
                    if (!list.ElementAt(idx).exact)
                    {
                        preferredIndex = idx;
                    }
                }
            }
            if (preferredIndex == -1)
                return null;
            else
                return list.ElementAt(preferredIndex);
        }

    }
}
