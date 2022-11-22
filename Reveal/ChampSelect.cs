using System.Collections.Generic;

namespace Reveal
{
    public class ChampSelect
    {
        public List<Participant> Participants { get; set; }
    }

    public class Participant
    {
        public object ActivePlatform { get; set; }
        public string Cid { get; set; }
        public string GameName { get; set; }
        public string GameTag { get; set; }
        public bool Muted { get; set; }
        public string Name { get; set; }
        public string Pid { get; set; }
        public string Puuid { get; set; }
        public string Region { get; set; }
    }
}