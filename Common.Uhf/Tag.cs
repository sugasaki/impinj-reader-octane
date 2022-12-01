namespace Common.Uhf
{
    public class Tag
    {
        /// <summary></summary>
        public string? Tid { set; get; } = null;

        /// <summary></summary>
        public ushort AntennaId { set; get; } = 1;

        /// <summary></summary>
        public ushort? Crc { set; get; } = null;

        /// <summary></summary>
        public ushort? PcBits { set; get; } = null;

        /// <summary></summary>
        public string Epc { set; get; } = string.Empty;

        /// <summary></summary>
        public double? PhaseAngle { set; get; } = null;

        /// <summary></summary>
        public double? PeakRssi { set; get; } = null;

        /// <summary>
        /// リーダーが最後にタグを見た時間。
        /// </summary>
        public DateTime ReceivedAt { set; get; } = DateTime.Now;


        public string Message
        {
            get
            {
                //var message = string.Format("Tid: {0}, AntennaId: {1}, Crc: {2}, PcBits: {3}, Epc: {4}, PeakRssi: {5}, ReceivedAt: {6}",
                //    Tid, AntennaId, Crc, PcBits, Epc, PeakRssi, ReceivedAt);
                var message = string.Format(" Epc: {4}, Received: {6}",
                    Tid, AntennaId, Crc, PcBits, Epc, PeakRssi, ReceivedAt);
                return message;
            }
        }


    }
}
