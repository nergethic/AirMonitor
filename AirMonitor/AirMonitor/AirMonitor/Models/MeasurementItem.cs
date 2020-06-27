using System;
using System.Collections.Generic;
using System.Text;

namespace AirMonitor.Models
{
    public class MeasurementItem
    {
        public List<MeasurementValue> Values { get; set; }
        public List<AirQualityIndex> Indexes { get; set; }
    }
}
