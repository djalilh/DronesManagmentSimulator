using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Position
    {
        public int PositionID { get; set; }
        public string Name { get; set; }
        public int ThreatLevel { get; set; }
        public string ThreatLevelColor { get; set; }
        public Drone ActiveDrone { get; set; }
    }
}
